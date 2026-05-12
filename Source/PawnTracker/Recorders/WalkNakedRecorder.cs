using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class WalkNakedRecorder : HistoryTaleRecorder<WalkNakedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<WalkNakedEvent>(CreateRecord);
    }

    public override void CreateRecord(WalkNakedEvent e)
    {
        var pawn = e.Pawn;
        var recordDef = HistoryRecordDefOf.WalkNaked;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Do(p => p.Strip()).Execute();

        foreach (var pawn in pawns)
        {
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
        }
    }
}
