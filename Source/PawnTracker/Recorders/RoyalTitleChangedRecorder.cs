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
        var pawn = scenario.Pawn().Colonist().SetRoyalTitle(RoyalTitleDefOf.Count).CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.TitleGained, "[PAWN] gained the royal title of Archon from [Faction].");
    }

    [RequiresRoyalty]
    public void TestLoss(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist()
            .SetRoyalTitle(Extra.RoyalTitleDefOf.Praetor)
            .SetRoyalTitle(null)
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.TitleLost, "[PAWN] lost the royal title of Praetor from [Faction].");
    }

    [RequiresRoyalty]
    public void TestDemotion(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist()
            .SetRoyalTitle(Extra.RoyalTitleDefOf.Praetor)
            .SetRoyalTitle(RoyalTitleDefOf.Knight)
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.TitleLost, "[PAWN] was demoted from Praetor to Knight by [Faction].");
    }
}
