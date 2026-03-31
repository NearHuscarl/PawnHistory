using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class GameEndedWanderersJoinedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;
            if (e.IncidentDef?.defName != "GameEndedWanderersJoin")
                return;

            HandleEvent(e);
        });
    }

    private void HandleEvent(WandererJoinedEvent e)
    {
        var recordDef = HistoryRecordDefOf.GameEndedWanderersJoined;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .WithOthers(e.Group.ToList())
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, e.Group);
    }

    public override void Test(TestScenario scenario)
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
