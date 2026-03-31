using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class RefugeePodCrashedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;
            if (e.QuestScript?.defName != "RefugeePodCrash")
                return;

            HandleRefugeePodCrashedEvent(e);
        });
    }

    private void HandleRefugeePodCrashedEvent(WandererJoinedEvent e)
    {
        var recordDef = HistoryRecordDefOf.RefugeePodCrashed;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddConstant("dead", e.Pawn.Dead)
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, e.Group);
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Incident("RefugeePodCrash").Execute();
    }
}
