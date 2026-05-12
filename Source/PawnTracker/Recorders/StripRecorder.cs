using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class StripRecorder : HistoryTaleRecorder<StripEvent>
{
    protected override float DaysToRecordAgain => 5f;

    public override void Register()
    {
        GameEventBus.Subscribe<StripEvent>(CreateRecord);
    }

    public override void CreateRecord(StripEvent e)
    {
        var (pawn, strippedPawn) = e;
        var recordDef = HistoryRecordDefOf.Stripped;
        var desc = recordDef.Description(pawn)
            .AddRule("STRIPPED", strippedPawn, addSubsymbols: true)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc, [strippedPawn]);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Execute();

        for (var i = 0; i < pawns.Count; i++)
        {
            TaleRecorder.RecordTale(Extra.TaleDefOf.Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Extra.TaleDefOf.Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Extra.TaleDefOf.Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
        }
    }
}
