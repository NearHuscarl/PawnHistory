using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class GameEndedWanderersJoinedRecorder : RecorderBase<WandererJoinedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (e.IncidentDef?.defName != "GameEndedWanderersJoin")
                return;

            CreateRecord(e);
        });
    }

    public override void CreateRecord(WandererJoinedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.GameEndedWanderersJoined;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .WithOthers(e.Group.ToList())
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, e.Group);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();
        Find.CurrentMap.mapPawns.AllHumanlikeSpawned.ForEach(p => p.Kill(null));
        // kill man in black
        TickDelayManager.Delay(300, () => Find.CurrentMap.mapPawns.AllHumanlikeSpawned.ForEach(p => p.Kill(null)));
        // wait for the 'game over' letter appears
        TickDelayManager.Delay(1200, () =>
        {
            scenario.ForwardTime(1);
            scenario.SlowDown();
        });
    }
}
