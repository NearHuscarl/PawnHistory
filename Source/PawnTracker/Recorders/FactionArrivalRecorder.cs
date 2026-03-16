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
        var eventDef = HistoryRecordDefOf.VisitorArrived;

        foreach (var pawn in pawns)
        {
            var desc = eventDef.ResolveDescription("visitorArrived", pawn)
                .WithFaction(lord.faction)
                .WithOthers(pawns)
                .Resolve();
            AddRecord(new HistoryRecord(eventDef, pawn, desc));
        }
    }

    private void HandleTravelerGroupStartedEvents(Lord lord, List<Pawn> pawns)
    {
        var eventDef = HistoryRecordDefOf.TravelGroupArrived;

        foreach (var pawn in pawns)
        {
            var desc = eventDef.ResolveDescription("travelGroupArrived", pawn)
                .WithFaction(lord.faction)
                .WithOthers(pawns)
                .Resolve();
            AddRecord(new HistoryRecord(eventDef, pawn, desc));
        }
    }
}
