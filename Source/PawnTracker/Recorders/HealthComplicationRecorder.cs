using HarmonyLib;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class HealthComplicationRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<HealthComplicationEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;
            HandleHealthComplicationEvent(e);
        });
    }

    private void HandleHealthComplicationEvent(HealthComplicationEvent e)
    {
        var recordDef = HistoryRecordDefOf.HealthComplication;
        var desc = recordDef.Description(e.Pawn)
            .AddRule("Condition", e.Condition)
            .AddRule("Cause", e.Cause?.LabelNounInBracket())
            .AddConstant("hasCause", e.Cause != null)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc);
    }

    public void TestWithCause(TestScenario scenario)
    {
        var cause = (Hediff)null;
        var pawn = scenario.Pawn()
            .AddHediff("SmokeleafTolerance", hediffCreated: h => cause = h)
            .CreateSingle();
        var giver = cause.def.hediffGivers
            .OfType<HediffGiver_RandomDrugEffect>()
            .FirstOrDefault();

        cause.Severity = 1f;
        if (giver.TryApply(pawn))
            Accessor.HediffGiver.SendLetter(giver, pawn, cause);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] developed a health condition: Asthma. It was caused by smokeleaf tolerance (massive).");
    }

    public void TestNoCause(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .AddHediff("HeartAttack")
            .CreateSingle();
        var giver = pawn.def.race.hediffGiverSets
            .SelectMany(set => set.hediffGivers)
            .OfType<HediffGiver_RandomAgeCurved>()
            .FirstOrDefault();

        if (giver.TryApply(pawn))
            Accessor.HediffGiver.SendLetter(giver, pawn, null);

        // TODO: why does this pass with Eventually() & no index offset??
        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] developed a health condition: Heart attack.", -2 /* offset incapacitated record */, exactMatch: true);
    }
}