using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MentalBreakComp_RunWild : MentalBreakComp
{
    public override bool Match(BuildInput input) => input.MentalBreak == Extra.MentalBreakDefOf.RunWild;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder.AddRule("Faction", input.Pawn.Faction);
    }

    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Do(p => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.RunWild))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(
            HistoryRecordDefOf.MentalBreak,
            $"[PAWN] was fed up with civilization. [PAWN_pronoun] decided to leave [FACTION] to live with the animals in the wild. {MentalBreakRecorder.MoodReasonTemplate}");
    }
}
