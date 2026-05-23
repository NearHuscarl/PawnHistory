using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_RemovePart : SurgeryComp
{
    public override bool Match(BuildInput input) => input.Event is SurgeryRemoveBodyPartEvent;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.BodyPartRemoved;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = (SurgeryRemoveBodyPartEvent)input.Event;
        return builder
            .AddRule("Part", e.Part.Label.Colorize(HediffDefOf.MissingBodyPart.defaultLabelColor))
            .AddRule("BadHediff", e.BadHediff?.LabelNounInBracket())
            .AddConstant("intent", e.Intent);
    }

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = (SurgeryRemoveBodyPartEvent)input.Event;
        return builder
            .AddRule("Part", e.Part)
            .AddConstant("intent", e.Intent);
    }

    public void TestHarvest(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(scenario, RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Lung);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartRemoved,
            Description = "[PAWN]'s left lung was harvested by [Doctor].",
            Concerns = [doctor],
        });
    }

    public void TestAmputate(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            RecipeDefOf.RemoveBodyPart,
            BodyPartDefOf.Leg,
            patient => patient.AddHediff(HediffDefOf.WoundInfection, BodyPartDefOf.Leg, h => h.Severity = 0.8f));

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartRemoved,
            Description = "[PAWN]'s left leg was amputated by [Doctor] due to an infection (extreme).",
            Concerns = [doctor],
        });
    }

    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            RecipeDefOf.RemoveBodyPart,
            BodyPartDefOf.Lung,
            surgeryOutcome: SurgeryOutcomes.Death);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] fatally botched a harvest on [PAWN]'s left lung.",
            Concerns = [doctor],
        });
        Expect.That(patient).ToHaveHistoryRecord(HistoryRecordDefOf.Death, "[PAWN] died.");
        var historyRecords = patient.HistoryRecords.TakeLast(2).Select(r => r.def);
        Expect.That(historyRecords).SequenceEqual([HistoryRecordDefOf.BotchedSurgery, HistoryRecordDefOf.Death]);
    }
}
