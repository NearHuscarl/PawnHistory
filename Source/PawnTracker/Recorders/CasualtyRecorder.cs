using HarmonyLib;
using PawnHistory.Source.DebugTools;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class CasualtyRecorder : RecorderBase
{
    [NearDebugAction]
    public static void EveryoneOnFire()
    {
        var pawns = Find.CurrentMap.mapPawns.AllPawnsSpawned
            .Where(p => p != null && p.relations != null && p.RaceProps?.Humanlike == true)
            .ToList();

        foreach (var pawn in pawns)
            FireUtility.TryAttachFire(pawn, 1.75f, null);

        Messages.Message("Everyone is now on fire!", MessageTypeDefOf.NeutralEvent);
    }

    [NearDebugAction]
    public static void OneBigFamily()
    {
        var pawns = Find.CurrentMap.mapPawns.AllPawnsSpawned
            .Where(p => p != null && p.relations != null && p.RaceProps?.Humanlike == true)
            .ToList();

        if (pawns.Count < 2)
            return;

        var possibleRelations = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where(def => def != null && def.defName != "Bond").ToList();

        foreach (var pawn in pawns)
        {
            var other = pawns.Where(p => p != pawn).RandomElementWithFallback(null);
            if (other == null)
                continue;

            var relation = possibleRelations.RandomElement();

            if (pawn.relations.DirectRelationExists(relation, other))
                continue;

            try
            {
                pawn.relations.AddDirectRelation(relation, other);
            }
            catch (Exception ex)
            {
                Log.Warning($"OneBigFamily: Failed to add relation {relation.defName} between {pawn} and {other}: {ex}");
            }
        }

        Messages.Message("Everyone is now related to one random person on map!", MessageTypeDefOf.NeutralEvent);
    }

    static readonly AccessTools.FieldRef<BattleLogEntry_RangedImpact, Pawn> OriginalTargetPawnRef =
        AccessTools.FieldRefAccess<BattleLogEntry_RangedImpact, Pawn>("originalTargetPawn");

    public override void Register()
    {
        GameEventBus.Subscribe<CasualtyLogAddedEvent>(e =>
        {
            // We intercept BattleLog.Add() but it runs before DamageWorker.AssociateWithLog() which is required to populate bodyPart data so we can get the
            // exact in-game combat log. But since DamageWorker.AssociateWithLog() is not always used we need to pick BattleLog.Add and fallback.
            TickDelayManager.Delay(0, () =>
            {
                var combatLogText = e.Subject?.health.hediffSet.hediffs.FirstOrDefault(h => h.combatLogEntry?.Target?.LogID == e.LastDamageEntry?.LogID)?.combatLogText;
                Pawn originalTargetPawn = null;

                if (e.LastDamageEntry is BattleLogEntry_RangedImpact rangedEntry)
                    originalTargetPawn = OriginalTargetPawnRef(rangedEntry);

                if (e.Casualty == CasualtyType.Killed)
                    HandleKillEvent(e, combatLogText, originalTargetPawn);
                if (e.CulpritHediff != HediffDefOf.Anesthetic)
                    HandleDownOrDeathEvent(e, combatLogText, originalTargetPawn);
            });
        });
    }

    private void HandleDownOrDeathEvent(CasualtyLogAddedEvent e, string combatLogText, Pawn originalTarget)
    {
        if (!RecorderManager.ShouldRecord(e.Subject))
            return;

        var isKillLog = e.Casualty == CasualtyType.Killed;
        var eventDef = isKillLog ? HistoryRecordDefOf.Death : HistoryRecordDefOf.Downed;

        if (!isKillLog && CompHistoryManager.GetComp(e.Subject).records.LastOrDefault()?.def == HistoryRecordDefOf.Death)
        {
            Log.Warning($"[PawnHistory] Received downed transition from {e.Subject.NameShortColored}, but they were already dead. Skipping..");
            return;
        }

        var transitionText = e.TransitionEntry.ToGameStringFromPOV(e.Subject);
        string desc;
        // combatLogText is null when:
        // - Log entry is not associated with any active battle. Non-combat dead needs to be handled manually (e.g. BloodLoss, ToxicBuildup...)
        // - LastDamageEntry may not match any current hediff if the same hediff was linked to an earlier combat log entry.
        if (combatLogText == null)
        {
            var hediffInt = e.Subject.health.hediffSet.hediffs.Where(h => h.def == e.CulpritHediff).OrderBy(h => h.ageTicks).FirstOrDefault();
            var rootKeyword = isKillLog ? "KilledEntry" : "DownedEntry";
            desc = eventDef.ResolveDescription(rootKeyword, e.Subject)
                .AddRule("Hediff", hediffInt, hediffInt?.Part)
                .AddConstantIf(e.CulpritHediff != null, "reason", "true")
                .Resolve();
        }
        else
            desc = $"{combatLogText} {transitionText}";

        AddRecord(new HistoryRecord(eventDef, e.Subject, desc, [e.Initiator, originalTarget]));

        if (isKillLog)
            HandleRelativeDeathEvent(e.Subject, e.Initiator, originalTarget, combatLogText, transitionText, desc);
    }

    private void HandleRelativeDeathEvent(Pawn deceased, Pawn initiator, Pawn originalTarget, string combatLogText, string transitionText, string deathDesc)
    {
        var eventDef = HistoryRecordDefOf.RelativeDeath;
        var deceasedName = deceased.NameShortColored.Resolve();

        foreach (var relative in deceased.relations.PotentiallyRelatedPawns)
        {
            if (relative == null || !RecorderManager.ShouldRecord(relative))
                continue;

            var relationDef = relative.GetMostImportantRelation(deceased);
            if (relationDef == null) continue;

            var relativePov = eventDef.ResolveDescription("RelativePov", relative)
                .AddRule("Relation", relationDef.GetGenderSpecificLabel(deceased))
                .AddRule("Subject", deceased)
                .Resolve();

            // "A died" -> "C's brother, A, died"
            var desc = combatLogText != null
                ? transitionText.ReplaceFirst(deceasedName, relativePov) + " " + combatLogText
                : deathDesc.ReplaceFirst(deceasedName, relativePov);

            AddRecord(new HistoryRecord(eventDef, relative, desc, [deceased, initiator, originalTarget]));
        }
    }

    private void HandleKillEvent(CasualtyLogAddedEvent e, string combatLogText, Pawn originalTarget)
    {
        if (!RecorderManager.ShouldRecord(e.Initiator))
            return;

        var transitionText = e.TransitionEntry.ToGameStringFromPOV(e.Initiator);
        var desc = $"{combatLogText} {transitionText}";
        AddRecord(new HistoryRecord(HistoryRecordDefOf.Kill, e.Initiator, desc, [e.Subject, originalTarget]));
    }
}
