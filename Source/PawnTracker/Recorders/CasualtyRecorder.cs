using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class CasualtyRecorder : RecorderBase
{
    static readonly AccessTools.FieldRef<BattleLogEntry_RangedImpact, Pawn> OriginalTargetPawnRef =
        AccessTools.FieldRefAccess<BattleLogEntry_RangedImpact, Pawn>("originalTargetPawn");

    public override void Register()
    {
        GameEventListener.Subscribe<CasualtyLogAddedEvent>(e =>
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
        var eventDef = isKillLog ? PawnEventDefOf.Death : PawnEventDefOf.Downed;

        if (!isKillLog && CompHistoryManager.GetComp(e.Subject).records.LastOrDefault()?.eventDef == PawnEventDefOf.Death)
        {
            Log.Warning($"[PawnHistory] Received downed transition from {e.Subject.NameShortColored}, but they were already dead. Skipping..");
            return;
        }

        var transitionText = e.TransitionEntry.ToGameStringFromPOV(e.Subject);
        string desc;
        // log entry is not associated with any active battle. Non-combat dead needs to be handled manually (e.g. BloodLoss, ToxicBuildup...)
        if (combatLogText == null)
        {
            desc = eventDef.ResolveDescription(e.Casualty.ToString(), e.Subject)
                .AddRule("HEDIFF", e.CulpritHediff)
                .AddConstantIf(e.CulpritHediff != null, "reason", "true")
                .Resolve();
        }
        else
            desc = $"{combatLogText} {transitionText}";

        AddRecord(new HistoryRecord(eventDef, e.Subject, desc, [e.Initiator, originalTarget]));
    }

    private void HandleKillEvent(CasualtyLogAddedEvent e, string combatLogText, Pawn originalTarget)
    {
        if (!RecorderManager.ShouldRecord(e.Initiator))
            return;

        var transitionText = e.TransitionEntry.ToGameStringFromPOV(e.Initiator);
        var desc = $"{combatLogText} {transitionText}";
        AddRecord(new HistoryRecord(PawnEventDefOf.Kill, e.Initiator, desc, [e.Subject, originalTarget]));
    }
}
