using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_PregnancyTerminated : SurgeryComp
{
    public override bool Match(BuildInput input) => input.Event.Recipe == Extra.RecipeDefOf.TerminatePregnancy;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.PregnancyTerminated;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input) => builder;

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input) => builder;

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.TerminatePregnancy,
            buildPatient: patient => patient.AddHediff(HediffDefOf.PregnantHuman));

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PregnancyTerminated,
            Description = "[PAWN]'s pregnancy was terminated by [Doctor].",
            Concerns = [doctor],
        });
    }

    [RequiresBiotech]
    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.TerminatePregnancy,
            buildPatient: patient => patient.AddHediff(HediffDefOf.PregnantHuman),
            surgeryOutcome: SurgeryOutcomes.MinorFailure);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] slightly botched a pregnancy termination on [PAWN], causing [NewInjuries].",
            Concerns = [doctor],
        });
    }
}
