using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class WandererJoinedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;
            if (e.QuestScript?.defName != "WandererJoins")
                return;

            HandleWandererJoinedEvent(e);
        });
    }

    private void HandleWandererJoinedEvent(WandererJoinedEvent e)
    {
        var recordDef = HistoryRecordDefOf.WandererJoined;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, e.Group);
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Incident("WandererJoin").Execute();
    }
}
