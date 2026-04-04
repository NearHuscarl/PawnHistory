using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class WalkNakedRecorder : HistoryTaleRecorder
{
    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != TaleDefOf.WalkedNaked)
                return;

            HandleWalkNakedEvent(e);
        });
    }

    private void HandleWalkNakedEvent(TaleRecordedEvent e)
    {
        var recordDef = HistoryRecordDefOf.WalkNaked;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .Resolve();

        if (!ShouldRecordTale(e.Pawn, recordDef, desc))
            return;

        AddRecord(recordDef, e.Pawn, desc);
    }

    [SkipTest]
    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().StripNaked().Execute();

        foreach (var pawn in pawns)
        {
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
        }
    }
}
