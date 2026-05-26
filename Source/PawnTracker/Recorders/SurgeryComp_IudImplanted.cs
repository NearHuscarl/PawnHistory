using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_IudImplanted : SurgeryComp
{
    public override bool Match(BuildInput input) => input.Event.Recipe == Extra.RecipeDefOf.ImplantIUD;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.IudImplanted;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input) => builder;

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input) => builder;

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(scenario, Extra.RecipeDefOf.ImplantIUD);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.IudImplanted,
            Description = "[PAWN] had an IUD implanted by [Doctor].",
            Concerns = [doctor],
        });
    }

    [RequiresBiotech]
    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(scenario, Extra.RecipeDefOf.ImplantIUD, surgeryOutcome: SurgeryOutcomes.SterilizedFailure);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] botched an IUD implantation on [PAWN], leaving [Him] permanently sterile.",
            Concerns = [doctor],
        });
    }
}
