using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
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
        var caravan = scenario.Caravan(pawns).Position(spot).Execute();

        scenario.Incident(DefLookup.Incident.Ambush, caravan).Point(200).Execute();

        // TODO: async test that only make assertion after map generation.
        // var enemies = pawns[0].Map.mapPawns.AllPawnsSpawned.Where(p => p.Faction.HostileTo(Faction.OfPlayer)).ToList();
        // Expect.ThatAll(enemies).ToHaveHistoryRecord("[PAWN] and [n] others from [Faction] ambushed [Caravan].", HistoryRecordDefOf.CaravanAmbush);
    }
}
