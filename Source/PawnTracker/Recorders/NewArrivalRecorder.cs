using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;

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

    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(3)
            .Colonist()
            .Execute();

        foreach (var pawn in pawns)
        {
            Expect.That(pawn).ToHaveHistoryRecord("[PAWN] crash-landed in a drop pod with 2 others to start a new settlement.");
        }
    }
}