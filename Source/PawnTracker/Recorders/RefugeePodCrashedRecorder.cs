using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RefugeePodCrashedRecorder : RecorderBase<WandererJoinedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (e.QuestScript?.defName != "RefugeePodCrash")
                return;

            CreateRecord(e);
        });
    }

    public override void CreateRecord(WandererJoinedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.RefugeePodCrashed;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddConstant("dead", e.Pawn.Dead)
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, e.Group);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        scenario.Incident(DefLookup.Incident.RefugeePodCrash).Execute();
    }
}
