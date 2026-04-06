using LudeonTK;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
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
        var part = e.Pawn.health.hediffSet.hediffs.LastOrDefault(h => h.def == e.Condition && h.ageTicks == 0).Part;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Condition", e.Condition.LabelNounColored())
            .AddRule("Cause", e.Cause?.LabelNounInBracket())
            .AddRule("Part", part)
            .AddConstant("hediff", e.Condition.defName)
            .AddConstant("hasCause", e.Cause != null)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc);
    }

    public void LogAllHediffGiverSubClasses()
    {
        var baseType = typeof(HediffGiver);
        var types = GenTypes.AllSubclassesNonAbstract(baseType);

        DebugTables.MakeTablesDialog(types,
            new TableDataGetter<Type>("Class Name", t => t.Name)
        );
    }

    public void TestWithCause(TestScenario scenario)
    {
        var cause = (Hediff)null;
        var cause2 = (Hediff)null;
        var pawn = scenario.Pawn()
            .AddHediff("WakeUpTolerance", hediffCreated: h => cause = h)
            .AddHediff("AlcoholTolerance", hediffCreated: h => cause2 = h)
            .CreateSingle();
        var giver = cause.def.hediffGivers
            .OfType<HediffGiver_RandomDrugEffect>()
            .FirstOrDefault();
        var giver2 = cause2.def.hediffGivers
            .OfType<HediffGiver_RandomDrugEffect>()
            .FirstOrDefault();

        cause.Severity = 1f;
        cause2.Severity = 1f;

        if (giver.TryApply(pawn))
            Accessor.HediffGiver.SendLetter(giver, pawn, cause);
        if (giver2.TryApply(pawn))
            Accessor.HediffGiver.SendLetter(giver2, pawn, cause2);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] developed chemical damage in [His] kidney. It was caused by wake-up tolerance (massive).", -2);
        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] developed cirrhosis. It was caused by alcohol tolerance (massive).", -1);
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

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] developed a heart attack.", -2 /* offset incapacitated record */, exactMatch: true);
    }
}