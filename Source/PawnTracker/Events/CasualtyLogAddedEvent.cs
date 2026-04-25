using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum CasualtyType
{
    Killed,
    Downed,
}

public record CasualtyLogAddedEvent(
    Battle Battle,
    BattleLogEntry_StateTransition TransitionEntry,
    Pawn Subject,
    Pawn Initiator,
    CasualtyType Casualty,
    HediffDef CulpritHediff) : GameEventBase;

[HarmonyPatch(typeof(BattleLog), nameof(BattleLog.Add))]
internal class BattleLog_Add_Patch
{
    public static void Postfix(BattleLog __instance, LogEntry entry)
    {
        if (entry is not BattleLogEntry_StateTransition transitionEntry) return;

        var battle = __instance.Battles.FirstOrDefault(b => b.Entries.Contains(transitionEntry));
        var subject = Accessor.BattleLogEntry_StateTransition.SubjectPawn(transitionEntry);
        var initiator = Accessor.BattleLogEntry_StateTransition.Initiator(transitionEntry);
        var casualtyType = transitionEntry.IconFromPOV(null) == LogEntry.Skull ? CasualtyType.Killed : CasualtyType.Downed;
        var culpritHediff = Accessor.BattleLogEntry_StateTransition.CulpritHediff(transitionEntry);

        GameEventBus.Publish(new CasualtyLogAddedEvent(battle, transitionEntry, subject, initiator, casualtyType, culpritHediff));
    }
}

// Some in-game LogEntry_DamageResult entries are created without affected body parts.
// Normally, DamageResult.AssociateWithLog(logEntry) should attach those parts.
// These patches recover the newly added damage hediffs, attach their body parts to the log entry,
// and link the hediffs back to the log so PawnHistory can generate accurate records.

internal static class CasualtyLogAddedContext
{
    public static void AssociateWithLog(LogEntry_DamageResult log, Pawn pawn, HediffDef damageHediff)
    {
        var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.ageTicks == 0 && h.def == damageHediff).ToList();
        var parts = hediffs.Select(h => h.Part).ToList();

        List<BodyPartRecord> list = null;
        List<bool> recipientPartsDestroyed = null;
        if (!parts.NullOrEmpty())
        {
            list = parts.Distinct().ToList();
            recipientPartsDestroyed = list.Select((BodyPartRecord part) => pawn.health.hediffSet.GetPartHealth(part) <= 0f).ToList();
        }
        
        log.FillTargets(list, recipientPartsDestroyed, false);
        
        foreach (var h in hediffs)
        {
            h.combatLogEntry = new Verse.WeakReference<LogEntry>(log);
            h.combatLogText = log.ToGameStringFromPOV(null);
        }
    }
}

[HarmonyPatch(typeof(Projectile_Liquid), "DoImpact")]
internal class Projectile_Liquid_DoImpact_Patch
{
    public static void Postfix(Projectile_Liquid __instance, Thing hitThing, IntVec3 cell)
    {
        try
        {
            var thingList = cell.GetThingList(__instance.Map);
            var subjectToEntry = Find.BattleLog.Battles.SelectMany(b => b.Entries)
                .Where(e => e.Tick == GenTicks.TicksAbs)
                .OfType<BattleLogEntry_RangedImpact>()
                .Where(e => Accessor.BattleLogEntry_RangedImpact.RecipientPawn(e) != null)
                .ToDictionary(e => Accessor.BattleLogEntry_RangedImpact.RecipientPawn(e), e => e);

            foreach (var thing in thingList)
            {
                if (thing is not Pawn pawn)
                    continue;

                if (!subjectToEntry.TryGetValue(pawn, out var entry))
                    continue;

                CasualtyLogAddedContext.AssociateWithLog(entry, pawn, __instance.DamageDef.hediff);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[PawnHistory] {ex}");
        }
    }
}
