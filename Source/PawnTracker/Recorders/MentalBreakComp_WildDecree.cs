using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MentalBreakComp_WildDecree : MentalBreakComp
{
    public override bool Match(BuildInput input) => ModsConfig.RoyaltyActive && input.MentalBreak == Extra.MentalBreakDefOf.WildDecree;
    
    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder
            .AddRule("Title", input.Pawn.royalty.MainTitle(), addSubsymbols: true)
            .AddRule("Quest", input.Quest.name.Colorize(ColoredText.GeneColor));
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        Expect.Assertions(1);
        GameEventBus.SubscribeOnce<MentalBreakStartedEvent>(e =>
        {
            Expect.That(e.Pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.MentalBreak,
                Description = $"[PAWN] issued a wild royal decree, demanding that the colony undertake [Quest]. {MentalBreakRecorder.MoodReasonTemplate}",
                Quest = e.Quest,
            });
        });

        scenario.Pawn()
            .Colonist()
            .SetRoyalTitle(RoyalTitleDefOf.Count)
            .Do(p => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.WildDecree))
            .CreateSingle();
    }

    [RequiresRoyalty]
    public void TestRandom(TestScenario scenario)
    {
        Expect.Assertions(1);
        GameEventBus.SubscribeOnce<MentalBreakStartedEvent>(e =>
        {
            Expect.That(e.Pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.MentalBreak,
                Description = "[PAWN], to assert [His] authority as an Archon, issued a wild royal decree, demanding that the colony undertake [Quest].",
                Quest = e.Quest,
            });
        });

        scenario.Pawn()
            .Colonist()
            .SetRoyalTitle(RoyalTitleDefOf.Count)
            .Do(p => p.royalty.IssueDecree(false))
            .CreateSingle();
    }
}
