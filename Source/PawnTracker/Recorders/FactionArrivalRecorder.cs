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
        GameEventListener.Subscribe<LordToilChangeEvent>(e =>
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

    private static void HandleVisitStartedEvents(Lord lord, List<Pawn> pawns)
    {
        var eventDef = PawnEventDefOf.VisitorArrived;

        foreach (var pawn in pawns)
        {
            var desc = eventDef.ResolveDescription(new DescriptionParams("visitorArrived", pawn, lord.faction)
            {
                RelatedPawns = pawns,
            });
            CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(eventDef, pawn, desc));
        }
    }

    private static void HandleTravelerGroupStartedEvents(Lord lord, List<Pawn> pawns)
    {
        var eventDef = PawnEventDefOf.TravelGroupArrived;

        foreach (var pawn in pawns)
        {
            var desc = eventDef.ResolveDescription(new DescriptionParams("travelGroupArrived", pawn, lord.faction)
            {
                RelatedPawns = pawns,
            });
            CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(eventDef, pawn, desc));
        }
    }
}
