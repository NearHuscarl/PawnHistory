using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class RescueRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<JobEndEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;
            if (e.CurrentJob.def != JobDefOf.Rescue)
                return;
            if (e.Condition != JobCondition.Succeeded)
                return;
            if (e.CurrentJob.targetA.Thing is not Pawn takee)
                return;
            if (takee.IsPrisonerOfColony) // handled by CaptureRecorder
                return;

            HandleRescueEvent(e.Pawn, takee);
        });
    }

    private void HandleRescueEvent(Pawn rescuer, Pawn takee)
    {
        var recordDef = HistoryRecordDefOf.Rescued;
        var desc = recordDef.Description(takee)
            .IncludePawnGrammar()
            .AddRule("Rescuer", rescuer, addSubsymbols: true)
            .Resolve();
        AddRecord(recordDef, takee, desc, [rescuer]);
    }

    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(2).Colonist().Execute();
        var rescuer = pawns[0];
        var victim = pawns[1];

        HealthUtility.DamageUntilDowned(victim);

        scenario.SpeedUp();
        scenario.Thing()
            .BuildRoom(7, 7, "Bedroom")
            .AsBarrack(pawns)
            .Execute();

        scenario.Pawn([rescuer])
            .StartJob(JobDefOf.Rescue, victim, RestUtility.FindBedFor(victim))
            .Execute();
    }
}
