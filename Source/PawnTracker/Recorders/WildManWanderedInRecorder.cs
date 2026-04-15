using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class WildManWanderedInRecorder : RecorderBase<WandererJoinedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (e.IncidentDef?.defName != "WildManWandersIn")
                return;

            CreateRecord(e);
        });
    }

    public override void CreateRecord(WandererJoinedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.WildManWanderedIn;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, e.Group);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        scenario.Incident(DefLookup.Incident.WildManWandersIn).Execute();
    }
}
