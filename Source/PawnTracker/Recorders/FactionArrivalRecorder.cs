using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class FactionArrivalRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            var lord = e.Lord;
            var currentToil = e.CurrentToil;
            var pawns = lord.ownedPawns.Where(ShouldRecord).ToList();
            var isStartingLord = currentToil == null;

            if (isStartingLord && lord.LordJob is LordJob_TravelAndExit)
                HandleTravelerGroupStartedEvents(lord, pawns);
            if (isStartingLord && lord.LordJob is LordJob_VisitColony)
                HandleVisitStartedEvents(lord, pawns);
        });
    }

    private void HandleVisitStartedEvents(Lord lord, List<Pawn> pawns)
    {
        var recordDef = HistoryRecordDefOf.VisitorArrived;

        foreach (var pawn in pawns)
        {
            var desc = recordDef.ResolveDescription("visitorArrived", pawn)
                .WithFaction(lord.faction)
                .WithOthers(pawns)
                .Resolve();
            AddRecord(recordDef, pawn, desc);
        }
    }

    private void HandleTravelerGroupStartedEvents(Lord lord, List<Pawn> pawns)
    {
        var recordDef = HistoryRecordDefOf.TravelGroupArrived;

        foreach (var pawn in pawns)
        {
            var desc = recordDef.ResolveDescription("travelGroupArrived", pawn)
                .WithFaction(lord.faction)
                .WithOthers(pawns)
                .Resolve();
            AddRecord(recordDef, pawn, desc);
        }
    }

    public override void Test(TestScenario scenario)
    {
        scenario.CreateIncident(IncidentDefOf.VisitorGroup).PawnCount(1).Execute();
        scenario.CreateIncident(IncidentDefOf.VisitorGroup).PawnCount(2).Execute();
        scenario.CreateIncident(IncidentDefOf.VisitorGroup).PawnCount(3).Execute();

        scenario.CreateIncident(IncidentDefOf.TravelerGroup).PawnCount(1).Execute();
        scenario.CreateIncident(IncidentDefOf.TravelerGroup).PawnCount(2).Execute();
        scenario.CreateIncident(IncidentDefOf.TravelerGroup).PawnCount(3).Execute();
    }
}
