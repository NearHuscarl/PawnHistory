using PawnHistory.Source.PawnTracker;
using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Test;
using Verse;

namespace PawnHistory.Source.Helper;

public static class GameUtility
{
    // copied from Root_Play.SetupForQuickTestPlay() but changed the map size and world size
    // reference: Search for "DevQuickTest"
    public static void CreateTestGame(Action runTest)
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
            Current.Game.World = WorldGenerator.GenerateWorld(0.03f, GenText.RandomSeedString(), OverallRainfall.AlmostNone, OverallTemperature.Normal, OverallPopulation.Normal, LandmarkDensity.Sparse);
            MakeWorldFlatAndBuildable(Current.Game.World);
            Find.GameInitData.startingTile = FindValidFlatTile(); // small map size requires a flat tile to avoid generation issues
            Find.GameInitData.mapSize = TestManager.ForcedDebugMapSize;
            Find.Scenario.PostIdeoChosen();

            PageUtility.InitGameStart();
            GameEventBus.SubscribeOnce<ScenarioPostGameStartEvent>((e) =>
            {
                ClearUpMap();
                LongEventHandler.ExecuteWhenFinished(runTest);
            });
        }, "GeneratingMap", true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
    }
    
    public static void MakeWorldFlatAndBuildable(World world)
    {
        world ??= Find.World;
        var grid = world.grid;

        for (var tileId = 0; tileId < grid.TilesCount; tileId++)
        {
            var tile = grid[tileId];

            tile.hilliness = Hilliness.Flat;
            tile.PrimaryBiome = BiomeDefOf.TemperateForest;

            // Keep the tile safely land, not beach/ocean/mountain-like.
            tile.elevation = 1000f;

            // Prevent marsh/swamp generation.
            tile.swampiness = 0f;

            // Avoid desert/sand-heavy generation.
            tile.rainfall = 0f;
            tile.temperature = 21f;

            tile.mutatorsNullable = [];
            // Rivers create water/mud during map generation.
            // tile.Rivers = [];
        }
    }

    /// <summary>
    /// Because we're testing on a tiny map, clear up all minable block, chunks & existing structures so nothing weird happens.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public static void ClearUpMap(Map map = null)
    {
        map ??= Find.CurrentMap;

        var chunks = map.listerThings.ThingsInGroup(ThingRequestGroup.Chunk);

        for (var i = chunks.Count - 1; i >= 0; i--)
        {
            chunks[i].Destroy();
        }

        var allThings = map.listerThings.AllThings;

        for (var i = allThings.Count - 1; i >= 0; i--)
        {
            var t = allThings[i];

            if (t.def.mineable)
                t.Destroy();
            if (t.def.IsWall)
                t.Destroy();
            if (t.def.IsDoor)
                t.Destroy();
            if (t.def.category == ThingCategory.Plant && t.def.plant.IsTree)
                t.Destroy();
        }
    }
    
    private static PlanetTile FindValidFlatTile()
    {
        var world = Find.World;

        for (var i = 0; i < 50000; i++)
        {
            var tile = Rand.Range(0, Find.WorldGrid.TilesCount);
            var tileInfo = world.grid[tile];

            if (tileInfo.WaterCovered)
                continue;

            if (tileInfo.Rivers is { Count: > 0 })
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
