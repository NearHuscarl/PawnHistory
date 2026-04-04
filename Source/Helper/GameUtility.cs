using PawnHistory.Source.PawnTracker;
using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace PawnHistory.Source.Helper;

public static class GameUtility
{
    // copied from Root_Play.SetupForQuickTestPlay() but changed the map size and world size
    // reference: Search for "DevQuickTest"
    public static void CreateTestGame(Action onCompleted = null)
    {
        LongEventHandler.QueueLongEvent(() =>
        {
            Current.ProgramState = ProgramState.Entry;
            Game.ClearCaches();
            Current.Game = new Game();
            Current.Game.InitData = new GameInitData();
            Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
            Find.Scenario.PreConfigure();
            Current.Game.storyteller = new Storyteller(StorytellerDefOf.Cassandra, DifficultyDefOf.Easy);
            Current.Game.World = WorldGenerator.GenerateWorld(0.03f, GenText.RandomSeedString(), OverallRainfall.AlmostNone, OverallTemperature.Hot, OverallPopulation.Normal, LandmarkDensity.Sparse);
            Find.GameInitData.startingTile = FindValidFlatTile(); // small map size requires a flat tile to avoid generation issues
            Find.GameInitData.mapSize = 25;
            Find.Scenario.PostIdeoChosen();

            PageUtility.InitGameStart();
            GameEventBus.SubscribeOnce<ScenarioPostGameStartEvent>((e) =>
            {
                ClearUpMap();
                onCompleted();
            });
        }, "GeneratingMap", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap));
    }

    /// <summary>
    /// Because we're testing on a tiny map, clear up all minable block, chunks & existing structures so nothing weird happens.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private static void ClearUpMap()
    {
        var chunks = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Chunk);

        for (var i = chunks.Count - 1; i >= 0; i--)
        {
            chunks[i].Destroy();
        }

        var buildings = Find.CurrentMap.listerBuildings.allBuildingsColonist
            .Concat(Find.CurrentMap.listerBuildings.allBuildingsNonColonist)
            .ToList();

        foreach (var b in buildings)
        {
            b.Destroy();
        }

        var allThings = Find.CurrentMap.listerThings.AllThings;

        for (var i = allThings.Count - 1; i >= 0; i--)
        {
            var t = allThings[i];

            if (t.def.mineable)
            {
                t.Destroy();
            }
        }

        var plants = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Plant);

        for (int i = plants.Count - 1; i >= 0; i--)
        {
            if (plants[i] is Plant plant && plant.def.plant.IsTree)
            {
                plant.Destroy();
            }
        }
    }

    private static PlanetTile FindValidFlatTile()
    {
        var world = Find.World;

        for (int i = 0; i < 50000; i++)
        {
            int tile = Rand.Range(0, Find.WorldGrid.TilesCount);
            var tileInfo = world.grid[tile];

            if (tileInfo.WaterCovered)
                continue;

            if (tileInfo.Rivers != null && tileInfo.Rivers.Count > 0)
                continue;

            if (tileInfo.hilliness != Hilliness.Flat)
                continue;

            if (IsAdjacentToWater(world, tile))
                continue;

            return tile;
        }

        throw new Exception("Failed to find suitable flat test tile.");
    }

    private static bool IsAdjacentToWater(World world, int tile)
    {
        var neighbors = new List<PlanetTile>();
        Find.WorldGrid.GetTileNeighbors(tile, neighbors);

        foreach (var n in neighbors)
        {
            var neighborTile = world.grid[n];

            if (neighborTile.WaterCovered)
                return true;
        }

        return false;
    }
}
