using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class WildManWanderedInRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;
            if (e.IncidentDef?.defName != "WildManWandersIn")
                return;

            HandleWildManWanderedInEvent(e);
        });
    }

    private void HandleWildManWanderedInEvent(WandererJoinedEvent e)
    {
        var recordDef = HistoryRecordDefOf.WildManWanderedIn;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, e.Group);
    }

    [SkipTest]
    public override void Test(TestScenario scenario)
    {
        scenario.Incident("WildManWandersIn").Execute();
    }
}
