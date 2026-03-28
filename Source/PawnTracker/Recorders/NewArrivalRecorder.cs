using PawnHistory.Source.PawnTracker.Events;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class NewArrivalRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<ScenarioStartEvent>(e =>
        {
            HandleNewArrivalEvent(e);
        });
    }

    private void HandleNewArrivalEvent(ScenarioStartEvent e)
    {
        var recordDef = HistoryRecordDefOf.NewArrival;

        foreach (var pawn in e.StartingPawns)
        {
            if (!ShouldRecord(pawn)) continue;

            // TODO: handle animal if whitelisted
            var desc = recordDef.Description(pawn)
                .WithOthers(e.StartingPawns)
                .AddConstant("method", e.ArriveMethod)
                .Resolve();
            AddRecord(recordDef, pawn, desc, e.StartingPawns);
        }
    }
}