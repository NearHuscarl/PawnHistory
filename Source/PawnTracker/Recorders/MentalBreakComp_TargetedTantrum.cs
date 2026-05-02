using System.Collections.Generic;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MentalBreakComp_TargetedTantrum : MentalBreakComp
{
    public override bool Match(BuildInput input) => input.MentalState is MentalState_TargetedTantrum;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var tantrum = (MentalState_TargetedTantrum)input.MentalState;
        return builder.AddRule("Thing", tantrum.target.Label.Colorize(ColoredText.SubtleGrayColor));
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return ((MentalState_TargetedTantrum)input.MentalState).target;
    }

    public void Test(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6)
            .WithThing(ThingDefOf.ComponentIndustrial, 100)
            .Execute();

        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Do(p => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.TargetedTantrum))
            .CreateSingle();

        var mentalState = (MentalState_TargetedTantrum)pawn.MentalState;
        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.MentalBreak,
            Description = $"[PAWN] had a tantrum. [He] was going to destroy [Thing]. {MentalBreakRecorder.MoodReasonTemplate}",
            Concerns = [mentalState.target],
        });
    }
}
