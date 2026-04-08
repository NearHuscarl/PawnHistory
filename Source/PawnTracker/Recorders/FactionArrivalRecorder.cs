using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public enum FactionArrivalType
{
    None,
    VisitorGroup,
    TravelerGroup
}

public class FactionArrivalRecorder : RecorderBase<FactionArrivalRecorder.Input>
{
    public record Input(Faction Faction, List<Pawn> Pawns, FactionArrivalType FactionArrivalType);

    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            var lord = e.Lord;
            var currentToil = e.CurrentToil;
            var pawns = lord.ownedPawns;
            var isStartingLord = currentToil == null;
            var factionArrivalType = FactionArrivalType.None;

            if (isStartingLord && lord.LordJob is LordJob_TravelAndExit)
                factionArrivalType = FactionArrivalType.TravelerGroup;
            if (isStartingLord && lord.LordJob is LordJob_VisitColony)
                factionArrivalType = FactionArrivalType.VisitorGroup;

            CreateRecord(new Input(lord.faction, pawns, factionArrivalType));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (faction, pawns, factionArrivalType) = input;
        
        pawns = pawns.Where(ShouldRecord).ToList();

        if (factionArrivalType == FactionArrivalType.VisitorGroup)
        {
            var recordDef = HistoryRecordDefOf.VisitorArrived;

            foreach (var pawn in pawns)
            {
                var desc = recordDef.Description(pawn)
                    .WithFaction(faction)
                    .WithOthers(pawns)
                    .Resolve();
                AddRecord(recordDef, pawn, desc);
            }
        }
        else if (factionArrivalType == FactionArrivalType.TravelerGroup)
        {
            var recordDef = HistoryRecordDefOf.TravelGroupArrived;

            foreach (var pawn in pawns)
            {
                var desc = recordDef.Description(pawn)
                    .WithFaction(faction)
                    .WithOthers(pawns)
                    .Resolve();
                AddRecord(recordDef, pawn, desc);
            }
        }
    }

    [DebugValues(70, 100, 140, 200)]
    public void TestVisitorGroup(TestScenario scenario, int point)
    {
        scenario.Incident(IncidentDefOf.VisitorGroup).Point(point).Execute();
    }

    [DebugValues(70, 100, 140, 200)]
    public void TestTravelerGroup(TestScenario scenario, int point)
    {
        scenario.Incident(IncidentDefOf.TravelerGroup).Point(point).Execute();
    }
}
