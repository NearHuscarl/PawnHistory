using HarmonyLib;
using System.Linq;
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
    LogEntry_DamageResult LastDamageEntry,
    Pawn Subject,
    Pawn Initiator,
    CasualtyType Casualty,
    HediffDef CulpritHediff) : GameEventBase;

[HarmonyPatch(typeof(BattleLog), nameof(BattleLog.Add))]
public static class BattleLog_Add_Patch
{
    public static void Postfix(BattleLog __instance, LogEntry entry)
    {
        if (entry is not BattleLogEntry_StateTransition transitionEntry) return;

        var battle = __instance.Battles.FirstOrDefault(b => b.Entries.Contains(transitionEntry));
        var transitionIndex = battle.Entries.IndexOf(transitionEntry);
        var subject = Accessor.BattleLogEntry_StateTransition.SubjectPawn(transitionEntry);
        var initiator = Accessor.BattleLogEntry_StateTransition.Initiator(transitionEntry);
        var casualtyType = transitionEntry.IconFromPOV(null) == LogEntry.Skull ? CasualtyType.Killed : CasualtyType.Downed;
        var damageResultEntry = battle.Entries.Skip(transitionIndex + 1).FirstOrDefault(e => e is LogEntry_DamageResult && e.Concerns(subject)) as LogEntry_DamageResult;
        var culpritHediff = Accessor.BattleLogEntry_StateTransition.CulpritHediff(transitionEntry);

        GameEventBus.Publish(new CasualtyLogAddedEvent(battle, transitionEntry, damageResultEntry, subject, initiator, casualtyType, culpritHediff));
    }
}
