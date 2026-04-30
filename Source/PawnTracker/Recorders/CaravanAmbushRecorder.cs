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

    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn().Colonist().Execute();
        var spot = Find.WorldGrid.GetNearbyTile();
        Expect.Assertions(1);

        var caravan = scenario.Caravan(pawns)
            .Position(spot)
            .OnMapGenerated(e =>
            {
                var enemies = e.Map.mapPawns.AllPawnsSpawned.Where(p => p.HostileTo(Faction.OfPlayer));

                Expect.ThatAll(enemies)
                    .Eventually()
                    .ToHaveHistoryRecord(new ExpectedHistoryRecord
                    {
                        Def = HistoryRecordDefOf.CaravanAmbush,
                        Description = "[PAWN] and [n] others from [Faction] ambushed [Caravan].",
                        TileId = e.Map.Tile.tileId,
                    });
            })
            .Execute();

        scenario.Incident(Extra.IncidentDefOf.Ambush, caravan).Point(200).Execute();
    }
}
