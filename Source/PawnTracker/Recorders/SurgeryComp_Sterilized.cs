using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_Sterilized : SurgeryComp
{
    private static readonly HashSet<string> SterilizedRecipes = [nameof(Extra.RecipeDefOf.TubalLigation), nameof(Extra.RecipeDefOf.Vasectomy)];
    public override bool Match(BuildInput input) => SterilizedRecipes.Contains(input.Event.Recipe?.defName) && input.Event.Data is SurgeryAddHediffData;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.Sterilized;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = input.Event;
        var data = (SurgeryAddHediffData)e.Data;
        return builder.AddRule("AddedHediff", data.HediffToAdd, addSubsymbols: true);
    }

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var data = (SurgeryAddHediffData)input.Event.Data;
        return builder.AddRule("AddedHediff", data.HediffToAdd, addSubsymbols: true);
    }

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(scenario, Extra.RecipeDefOf.TubalLigation);
        var (patient2, doctor2) = SurgeryRecorder.DoSurgery(scenario, Extra.RecipeDefOf.Vasectomy);
        
        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.Sterilized,
            Description = "[PAWN] received a tubal ligation from [Doctor]",
            Concerns = [doctor],
        });
        Expect.That(patient2).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.Sterilized,
            Description = "[PAWN] received a vasectomy from [Doctor]",
            Concerns = [doctor2],
        });
    }

    [RequiresBiotech]
    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(scenario, Extra.RecipeDefOf.TubalLigation, surgeryOutcome: SurgeryOutcomes.MinorFailure);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] slightly botched a tubal ligation procedure on [PAWN], causing [NewInjuries].",
            Concerns = [doctor],
        });
    }
}
