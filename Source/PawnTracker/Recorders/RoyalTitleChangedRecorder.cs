using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RoyalTitleChangedRecorder : RecorderBase<RoyalTitleChangedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<RoyalTitleChangedEvent>(CreateRecord);
    }

    public override void CreateRecord(RoyalTitleChangedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var isGained = IsTitleGained(e);
        if (isGained)
        {
            var recordDef = HistoryRecordDefOf.TitleGained;
            var desc = recordDef.Description(e.Pawn)
                .AddRule("NewTitle", e.NewTitle)
                .AddRule("Faction", e.Faction)
                .Resolve();

            AddRecord(recordDef, e.Pawn, desc);
        }
        else
        {
            var recordDef = HistoryRecordDefOf.TitleLost;
            var desc = recordDef.Description(e.Pawn)
                .AddRule("OldTitle", e.PreviousTitle)
                .AddRule("NewTitle", e.NewTitle)
                .AddRule("Faction", e.Faction)
                .AddConstant("hasNewTitle", e.NewTitle != null)
                .Resolve();

            AddRecord(recordDef, e.Pawn, desc);
        }
    }

    private static bool IsTitleGained(RoyalTitleChangedEvent e)
    {
        if (e.PreviousTitle == null)
            return true;
        if (e.NewTitle == null)
            return false;

        var titles = e.Faction.def.RoyalTitlesAwardableInSeniorityOrderForReading;
        return titles.IndexOf(e.PreviousTitle) < titles.IndexOf(e.NewTitle);
    }

    [RequiresRoyalty]
    public void TestGain(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();

        pawn.royalty.SetTitle(Faction.OfEmpire, RoyalTitleDefOf.Count, grantRewards: false);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] gained the royal title of Archon from [Faction].", HistoryRecordDefOf.TitleGained);
    }

    [RequiresRoyalty]
    public void TestLoss(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();

        pawn.royalty.SetTitle(Faction.OfEmpire, DefLookup.RoyalTitle.Praetor, grantRewards: false);
        pawn.royalty.SetTitle(Faction.OfEmpire, null, grantRewards: false);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] lost the royal title of Praetor from [Faction].", HistoryRecordDefOf.TitleLost);
    }

    [RequiresRoyalty]
    public void TestDemotion(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();

        pawn.royalty.SetTitle(Faction.OfEmpire, DefLookup.RoyalTitle.Praetor, grantRewards: false);
        pawn.royalty.SetTitle(Faction.OfEmpire, RoyalTitleDefOf.Knight, grantRewards: false);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] was demoted from Praetor to Knight by [Faction].", HistoryRecordDefOf.TitleLost);
    }
}
