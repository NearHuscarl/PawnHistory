using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

// Exists only to host pagination/manual UI tests for the recorder-based test runner.
public class PaginationRecorder : RecorderBase<ScenarioStartEvent>
{
    public override void Register()
    {
    }

    public override void CreateRecord(ScenarioStartEvent input)
    {
    }

    [SkipTest]
    public void TestPaginationUi(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .ThatMatches(ShouldRecord)
            .CreateSingle();
        var quest = scenario.Quest(QuestScriptDefOf.OpportunitySite_ItemStash).Execute();

        CompHistoryManager.GetComp(pawn).ClearAll();

        var startTick = GenTicks.TicksAbs - GenDate.TicksPerDay * 120;
        for (var i = 0; i < 120; i++)
        {
            var def = (i % 3) switch
            {
                0 => HistoryRecordDefOf.NewArrival,
                1 => HistoryRecordDefOf.Birthday,
                _ => HistoryRecordDefOf.SkillLeveledUp,
            };
            var detail = i % 4 == 0
                ? " This row is intentionally longer so the table has mixed heights and wrapped descriptions for manual pagination checks."
                : "";
            var record = new HistoryRecord(
                def,
                pawn,
                $"Pagination record {i + 1} for history tab verification.{detail}",
                quest: i % 8 == 0 ? quest : null
                );
            record.date = startTick + i * GenDate.TicksPerHour;
            pawn.HistoryRecords.Add(record);
        }

        scenario.OpenHistoryRecordTab(pawn);
    }

    [SkipTest]
    public void TestHistoryInlineEditUi(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .ThatMatches(ShouldRecord)
            .CreateSingle();
        var quest = scenario.Quest(QuestScriptDefOf.OpportunitySite_ItemStash).Execute();

        CompHistoryManager.GetComp(pawn).ClearAll();

        var records = new[]
        {
            new HistoryRecord(
                HistoryRecordDefOf.NewArrival,
                pawn,
                "Short inline edit target.",
                quest: quest
            ),
            new HistoryRecord(
                HistoryRecordDefOf.Birthday,
                pawn,
                "Raw <color=red>tagged</color> inline edit target.\nPress Shift+Enter here to confirm the row grows with multiline text."
            ),
            new HistoryRecord(
                HistoryRecordDefOf.SkillLeveledUp,
                pawn,
                "Whitespace save rejection target."
            )
        };

        var startTick = GenTicks.TicksAbs - GenDate.TicksPerDay * 3;
        for (var i = 0; i < records.Length; i++)
        {
            records[i].date = startTick + i * GenDate.TicksPerHour;
            pawn.HistoryRecords.Add(records[i]);
        }

        scenario.OpenHistoryRecordTab(pawn);
    }
}
