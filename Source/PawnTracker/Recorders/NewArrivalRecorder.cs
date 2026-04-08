using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class NewArrivalRecorder : RecorderBase<ScenarioStartEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<ScenarioStartEvent>(CreateRecord);
    }

    public override void CreateRecord(ScenarioStartEvent e)
    {
        var recordDef = HistoryRecordDefOf.NewArrival;
        var (startingPawns, arriveMethod) = e;

        foreach (var pawn in startingPawns)
        {
            if (!ShouldRecord(pawn))
                continue;

            // TODO: handle animal if whitelisted
            var desc = recordDef.Description(pawn)
                .WithOthers(startingPawns)
                .AddConstant("method", arriveMethod)
                .Resolve();
            AddRecord(recordDef, pawn, desc, startingPawns);
        }
    }

    public void Test(TestScenario scenario)
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