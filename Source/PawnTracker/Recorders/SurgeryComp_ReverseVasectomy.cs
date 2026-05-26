using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_ReverseVasectomy : SurgeryComp
{
    public override bool Match(BuildInput input) => input.Event.Recipe == Extra.RecipeDefOf.ReverseVasectomy && input.Event.Data is SurgeryRemoveHediffData;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.ReverseVasectomy;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = input.Event;
        var data = (SurgeryRemoveHediffData)e.Data;
        return builder.AddRule("RemovedHediff", data.HediffToRemove);
    }

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var data = (SurgeryRemoveHediffData)input.Event.Data;
        return builder.AddRule("RemovedHediff", data.HediffToRemove, addSubsymbols: true);
    }

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.ReverseVasectomy,
            buildPatient: patient => patient.AddHediff(HediffDefOf.Vasectomy, BodyPartDefOf.Torso));
        
        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.ReverseVasectomy,
            Description = "[PAWN]'s vasectomy was reversed by [Doctor]",
            Concerns = [doctor],
        });
    }

    [RequiresBiotech]
    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.ReverseVasectomy,
            buildPatient: patient => patient.AddHediff(HediffDefOf.Vasectomy, BodyPartDefOf.Torso),
            surgeryOutcome: SurgeryOutcomes.SterilizedFailure);
        
        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] botched a vasectomy reversal on [PAWN], leaving [Him] permanently sterile.",
            Concerns = [doctor],
        });
    }
}
