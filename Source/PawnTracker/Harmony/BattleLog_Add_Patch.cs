using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(BattleLog), nameof(BattleLog.Add))]
public static class BattleLog_Add_Patch
{
    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, HediffDef> CulpritHediffRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, HediffDef>("culpritHediffDef");
    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, BodyPartRecord> CulpritHediffTargetPartRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, BodyPartRecord>("culpritHediffTargetPart");

    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, Pawn> SubjectPawnRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, Pawn>("subjectPawn");
    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, Pawn> InitiatorRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, Pawn>("initiator");

    static readonly AccessTools.FieldRef<BattleLogEntry_RangedImpact, Pawn> OriginalTargetPawnRef =
        AccessTools.FieldRefAccess<BattleLogEntry_RangedImpact, Pawn>("originalTargetPawn");

    public static void Postfix(BattleLog __instance, LogEntry entry)
    {
        if (entry is not BattleLogEntry_StateTransition transitionEntry) return;

        // Right after the log is added, DamageWorker.AssociateWithLog() is not called yet so we will miss the
        // information about affected body part if we publish the event now.
        TickDelayManager.Delay(0, () =>
        {
            var battle = __instance.Battles.FirstOrDefault(b => b.Entries.Contains(transitionEntry));
            var transitionIndex = battle.Entries.IndexOf(transitionEntry);
            var initiator = InitiatorRef(transitionEntry);
            var subject = SubjectPawnRef(transitionEntry);
            var isKillLog = transitionEntry.IconFromPOV(null) == LogEntry.Skull;
            var damageResultEntry = battle.Entries.Skip(transitionIndex + 1).FirstOrDefault(e => e is LogEntry_DamageResult && e.Concerns(subject));
            var combatLogText = subject?.health.hediffSet.hediffs.FirstOrDefault(h => h.combatLogEntry?.Target?.LogID == damageResultEntry?.LogID)?.combatLogText;
            var culpritHediff = CulpritHediffRef(transitionEntry);
            Pawn originalTargetPawn = null;

            if (damageResultEntry is BattleLogEntry_RangedImpact rangedEntry)
                originalTargetPawn = OriginalTargetPawnRef(rangedEntry);

            if (isKillLog)
                HandleKillEvent(initiator, subject, combatLogText, transitionEntry, originalTargetPawn);
            if (!isKillLog && culpritHediff == HediffDefOf.Anesthetic)
                HandleAnesthetizedEvent(initiator, subject, culpritHediff);
            if (culpritHediff != HediffDefOf.Anesthetic)
                HandleDownOrDeathEvent(initiator, subject, combatLogText, transitionEntry, originalTargetPawn);
        });
    }

    private static void HandleAnesthetizedEvent(Pawn initiator, Pawn subject, HediffDef anestheticHediff)
    {
        if (!PawnTracker.ShouldTrack(subject))
            return;

        var resolvedDesc = PawnEventDefOf.Anesthetized.description.Formatted(
            subject.NameShortColored.Named("PAWN"),
            anestheticHediff.label.Colorize(anestheticHediff.defaultLabelColor).Named("ANESTHETIC")
        ).Resolve();

        GameEventListener.Publish(new GameEvent(subject, PawnEventDefOf.Anesthetized, resolvedDesc)
        {
            relatedPawns = [initiator],
        });
    }

    private static void HandleDownOrDeathEvent(Pawn initiator, Pawn subject, string combatLogText, BattleLogEntry_StateTransition transitionEntry, Pawn originalTarget)
    {
        if (!PawnTracker.ShouldTrack(subject))
            return;

        var isKillLog = transitionEntry.IconFromPOV(null) == LogEntry.Skull;
        var eventDef = isKillLog ? PawnEventDefOf.Death : PawnEventDefOf.Downed;

        if (!isKillLog && CompHistoryManager.GetComp(subject).records.LastOrDefault()?.eventDef == PawnEventDefOf.Death)
        {
            Log.Warning($"[PawnHistory] Received downed transition from {subject.NameShortColored}, but they were already dead. Skipping..");
            return;
        }

        var transitionText = transitionEntry.ToGameStringFromPOV(null);
        string resolvedDesc;
        // log entry is not associated with any active battle. Non-combat dead needs to be handled manually (e.g. BloodLoss, ToxicBuildup...)
        if (combatLogText == null)
        {
            var culpritHediff = CulpritHediffRef(transitionEntry);
            var reason = culpritHediff.label.Colorize(culpritHediff.defaultLabelColor);
            resolvedDesc = eventDef.description.Formatted(subject.NameShortColored.Named("PAWN"), reason.Named("REASON")).Resolve();
        }
        else
            resolvedDesc = $"{combatLogText} {transitionText}";

        GameEventListener.Publish(new GameEvent(subject, eventDef, resolvedDesc)
        {
            relatedPawns = [initiator, originalTarget],
        });
    }

    private static void HandleKillEvent(Pawn initiator, Pawn subject, string combatLogText, BattleLogEntry_StateTransition transitionEntry, Pawn originalTarget)
    {
        if (!PawnTracker.ShouldTrack(initiator))
            return;
        
        var transitionText = transitionEntry.ToGameStringFromPOV(null);
        GameEventListener.Publish(new GameEvent(initiator, PawnEventDefOf.Kill, $"{combatLogText} {transitionText}")
        {
            relatedPawns = [subject, originalTarget],
        });
    }
}
