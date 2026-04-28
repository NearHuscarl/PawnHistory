using LudeonTK;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class HealthComplicationRecorder : RecorderBase<HealthComplicationEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<HealthComplicationEvent>(CreateRecord);
    }

    public override void CreateRecord(HealthComplicationEvent e)
    {
        var (pawn, condition, cause) = e;

        if (!ShouldRecord(pawn))
            return;
        var recordDef = HistoryRecordDefOf.HealthComplication;
        var part = pawn.health.hediffSet.hediffs.LastOrDefault(h => h.def == condition && h.ageTicks == 0)?.Part;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Condition", condition.LabelNounColored())
            .AddRule("Cause", cause?.LabelNounInBracket())
            .AddRule("Part", part)
            .AddConstant("hediff", condition.defName)
            .AddConstant("hasCause", cause != null)
            .Resolve();

        AddRecord(recordDef, pawn, desc);
    }

    public void LogAllHediffGiverSubClasses()
    {
        var baseType = typeof(HediffGiver);
        var types = baseType.AllSubclassesNonAbstract();

        DebugTables.MakeTablesDialog(types,
            new TableDataGetter<Type>("Class Name", t => t.Name)
        );
    }

    public void TestWithCause(TestScenario scenario)
    {
        var cause = (Hediff)null;
        var cause2 = (Hediff)null;
        var pawn = scenario.Pawn()
            .AddHediff(Extra.HediffDefOf.WakeUpTolerance, hediffCreated: h => cause = h)
            .AddHediff(Extra.HediffDefOf.AlcoholTolerance, hediffCreated: h => cause2 = h)
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

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.HealthComplication, "[PAWN] developed chemical damage in [His] kidney. It was caused by wake-up tolerance (massive).", index: -2);
        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.HealthComplication, "[PAWN] developed cirrhosis. It was caused by alcohol tolerance (massive).", index: -1);
    }

    public void TestNoCause(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .AddHediff(Extra.HediffDefOf.HeartAttack)
            .CreateSingle();
        var giver = pawn.def.race.hediffGiverSets
            .SelectMany(set => set.hediffGivers)
            .OfType<HediffGiver_RandomAgeCurved>()
            .FirstOrDefault();

        if (giver.TryApply(pawn))
            Accessor.HediffGiver.SendLetter(giver, pawn, null);

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.HealthComplication, "[PAWN] developed a heart attack.", exactMatch: true);
    }
}
