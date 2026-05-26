using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_IudRemoved : SurgeryComp
{
    public override bool Match(BuildInput input) => input.Event.Recipe == Extra.RecipeDefOf.RemoveIUD;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.IudRemoved;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input) => builder;

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input) => builder;

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.RemoveIUD,
            buildPatient: patient => patient.AddHediff(HediffDefOf.ImplantedIUD, BodyPartDefOf.Torso));

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.IudRemoved,
            Description = "[PAWN]'s IUD was removed by [Doctor].",
            Concerns = [doctor],
        });
    }

    [RequiresBiotech]
    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.RemoveIUD,
            buildPatient: patient => patient.AddHediff(HediffDefOf.ImplantedIUD, BodyPartDefOf.Torso),
            surgeryOutcome: SurgeryOutcomes.SterilizedFailure);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] botched an IUD removal on [PAWN], leaving [Him] permanently sterile.",
            Concerns = [doctor],
        });
    }
}
