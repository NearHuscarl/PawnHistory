using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class AncientDangerWarningRecorder : RecorderBase<AncientDangerWarningEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<AncientDangerWarningEvent>(CreateRecord);
    }

    public override void CreateRecord(AncientDangerWarningEvent e)
    {
        var pawn = e.Pawn;
        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.AncientDangerWarning;
        var desc = recordDef.Description(pawn)
            .Resolve();

        AddRecord(recordDef, pawn, desc, null, RecordLocation.Of(pawn));
    }

    [TestTag("Flaky")]
    public Action Test(TestScenario scenario)
    {
        var map = Find.CurrentMap;

        Expect.Assertions(1);
        scenario.RunOnceOn<AncientDangerWarningEvent>(e =>
        {
            Expect.That(e.Pawn).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord()
            {
                Def = HistoryRecordDefOf.AncientDangerWarning,
                Description = "[PAWN] felt a deep sense of foreboding while approaching an ancient structure, sensing great danger within.",
                Location = RecordLocation.Of(e.Pawn)
            });
        });
        scenario.SpeedUp();
        var startCell = CellFinder.RandomClosewalkCellNear(new IntVec3(1, 0, 1), map, 3);
        var destinationCell = CellFinder.RandomClosewalkCellNear(new IntVec3(map.Size.x - 2, 0, map.Size.z - 2), map, 3);
        var pawns = scenario.Pawn(3)
            .Colonist()
            .Position(startCell, 0)
            .Execute();
        scenario.Map().GenerateAncientTemple(8, 8).Execute();
        scenario.Pawn(pawns[0])
            .Colonist()
            .StartJob(JobDefOf.Goto, destinationCell)
            .CreateSingle();

        return () => scenario.SlowDown();
    }
}
