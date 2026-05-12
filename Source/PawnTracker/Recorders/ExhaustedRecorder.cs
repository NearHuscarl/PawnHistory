using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ExhaustedRecorder : HistoryTaleRecorder<ExhaustedEvent>
{
    protected override float DaysToRecordAgain => 3f;

    public override void Register()
    {
        GameEventBus.Subscribe<ExhaustedEvent>(CreateRecord);
    }

    public override void CreateRecord(ExhaustedEvent e)
    {
        var pawn = e.Pawn;
        var recordDef = HistoryRecordDefOf.Exhausted;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(5).Colonist()
            .Do(p => TaleRecorder.RecordTale(TaleDefOf.Exhausted, p))
            .Execute();

        Expect.ThatAll(pawns).ToHaveHistoryRecordOf(HistoryRecordDefOf.Exhausted);
    }
}
