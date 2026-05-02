using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MentalBreakComp_BingingDrug : MentalBreakComp
{
    public override bool Match(BuildInput input) => input.MentalState is MentalState_BingingDrug;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder.AddRule("Drug", ((MentalState_BingingDrug)input.MentalState).chemical.label);
    }

    public void Test(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, "DrugRoom")
            .WithThing(ThingDefOf.GoJuice) // Binging_DrugExtreme
            .WithThing(ThingDefOf.Beer) // Binging_DrugMajor
            .Execute();

        TestBingingDrug(scenario, Extra.MentalBreakDefOf.Binging_DrugExtreme, "[PAWN] binged on [Drug] during an extreme mental break.");
        TestBingingDrug(scenario, Extra.MentalBreakDefOf.Binging_DrugMajor, "[PAWN] binged on [Drug] during a major mental break.");
    }

    private static void TestBingingDrug(TestScenario scenario, MentalBreakDef mentalBreakDef, string descriptionPrefix)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Do(p => p.StartMentalBreakWithMadeUpThought(mentalBreakDef))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.MentalBreak, $"{descriptionPrefix} {MentalBreakRecorder.MoodReasonTemplate}");
    }
}
