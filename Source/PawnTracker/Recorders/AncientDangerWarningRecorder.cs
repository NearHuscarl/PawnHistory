using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class AncientDangerWarningRecorder : RecorderBase<Pawn>
{
    private static readonly string LetterLabel = "LetterLabelAncientShrineWarning".Translate();

    public override void Register()
    {
        GameEventBus.Subscribe<ReceiveLetterEvent>(e =>
        {
            if (e.Label.Resolve() != LetterLabel)
                return;

            if (e.Pawns.FirstOrDefault() == null)
                return;

            CreateRecord(e.Pawns.FirstOrDefault());
        });
    }

    public override void CreateRecord(Pawn pawn)
    {
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

        scenario.SpeedUp();
        scenario.Pawn(3)
            .Colonist()
            .Do(p => p.Position = CellFinder.RandomEdgeCell(map)) // so pawn does not end up in the ancient temple
            .Execute();
        scenario.Map().GenerateAncientTemple(8, 8).Execute();
        scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.Goto, map.Center)
            .Execute();

        Expect.AnyPawnOnMap().Eventually().ToHaveHistoryRecord("[PAWN] felt a deep sense of foreboding while approaching an ancient structure, sensing great danger within.");

        return () => scenario.SlowDown();
    }
}