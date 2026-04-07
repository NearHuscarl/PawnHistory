using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class AncientDangerWarningRecorder : RecorderBase
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

            HandleAncientDangerWarningEvent(e.Pawns.FirstOrDefault());
        });
    }

    private void HandleAncientDangerWarningEvent(Pawn pawn)
    {
        var recordDef = HistoryRecordDefOf.AncientDangerWarning;
        var desc = recordDef.Description(pawn)
            .Resolve();

        AddRecord(recordDef, pawn, desc, storeLocation: true);
    }

    public Action Test(TestScenario scenario)
    {
        var map = Find.CurrentMap;

        scenario.SpeedUp();
        scenario.Map().GenerateAncientTemple(8, 8).Execute();
        scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.Goto, map.Center)
            .Execute();

        Expect.AnyPawnOnMap().Eventually().ToHaveHistoryRecord("[PAWN] felt a deep sense of foreboding while approaching an ancient structure, sensing great danger within.");

        return () => scenario.SlowDown();
    }
}