using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class CaravanAmbushRecorder : RecorderBase<CaravanAmbushedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<CaravanAmbushedEvent>(CreateRecord);
    }

    public override void CreateRecord(CaravanAmbushedEvent e)
    {
        var pawns = e.Enemies.Where(ShouldRecord).ToList();
        var recordDef = HistoryRecordDefOf.CaravanAmbush;

        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .WithOthers(pawns)
                .AddRule("Caravan", e.Caravan)
                .AddRule("Faction", e.Faction)
                .Resolve();

            AddRecord(recordDef, pawn, desc);
        }
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var spot = Find.WorldGrid.GetNearbyTile(Find.AnyPlayerHomeMap.Tile);
        Expect.Assertions(1);

        var caravan = scenario.Caravan(pawns)
            .Position(spot)
            .OnMapGenerated(e =>
            {
                var enemies = e.Map.mapPawns.AllPawnsSpawned.Where(p => p.Faction.HostileTo(Faction.OfPlayer));

                Expect.ThatAll(enemies)
                    .Eventually()
                    .ToHaveHistoryRecord("[PAWN] and [n] others from [Faction] ambushed [Caravan].", HistoryRecordDefOf.CaravanAmbush);
            })
            .Execute();

        scenario.Incident(DefLookup.Incident.Ambush, caravan).Point(200).Execute();
    }
}
