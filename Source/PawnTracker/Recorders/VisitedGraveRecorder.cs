using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class VisitedGraveRecorder : HistoryTaleRecorder<VisitedGraveEvent>
{
    protected override float DaysToRecordAgain => 3f;
    
    public override void Register()
    {
        GameEventBus.Subscribe<VisitedGraveEvent>(CreateRecord);
    }

    public override void CreateRecord(VisitedGraveEvent e)
    {
        var (pawn, deadPawn) = e;
        var recordDef = HistoryRecordDefOf.VisitedGrave;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Corpse", deadPawn)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc, [deadPawn]);
        AddRecord(recordDef, deadPawn, desc, [pawn]);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(5).Colonist().Execute();

        for (var i = 0; i < pawns.Count; i++)
        {
            TaleRecorder.RecordTale(Extra.TaleDefOf.VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Extra.TaleDefOf.VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Extra.TaleDefOf.VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
        }
    }
}
