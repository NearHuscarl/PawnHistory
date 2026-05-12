using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.HistoryBackfill;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RoyalTitleChangedRecorder : RecorderBase<RoyalTitleChangedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<RoyalTitleChangedEvent>(CreateRecord);
    }

    internal override IEnumerable<HistoryBackfillDefinition> GetBackfillDefinitions()
    {
        if (!ModsConfig.RoyaltyActive)
            yield break;

        yield return new HistoryBackfillDefinition(HistoryRecordDefOf.TitleGained)
            .AddHard(
                new MinimumAgeRule(13f),
                new MaximumCountRule(1),
                new LogicalGateRule((_, _) => ModsConfig.RoyaltyActive),
                new OrderBeforeRule(GenDate.TicksPerDay, HistoryRecordDefOf.PsylinkLevelGained))
            .AddSoft(
                new AgeCurveSoftRule([
                    new CurvePoint(13f, 0.02f),
                    new CurvePoint(18f, 0.25f),
                    new CurvePoint(25f, 1f),
                    new CurvePoint(35f, 1.2f),
                    new CurvePoint(55f, 0.75f),
                    new CurvePoint(90f, 0.2f)
                ]),
                new PreferGapBeforePlacedRule(
                    GenDate.DaysToTicks(3f),
                    GenDate.DaysToTicks(14f),
                    HistoryRecordDefOf.PsylinkLevelGained));
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
