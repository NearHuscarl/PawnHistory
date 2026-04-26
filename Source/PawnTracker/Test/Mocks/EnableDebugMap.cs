using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
internal static class EnableDebugMap
{
    private static void Prefix(ref IntVec3 mapSize, MapGeneratorDef mapGenerator)
    {
        if (!TestManager.Scenario.EnableDebugMap)
            return;
        
        var size = TestManager.Scenario.ForcedDebugMapSize;
        mapSize = new IntVec3(size, 1, size);
    }
}

[HarmonyPatch(typeof(GenStep_MutatorPostTerrain), nameof(GenStep_MutatorPostTerrain.Generate))]
internal static class GenStep_MutatorPostTerrain_Generate_Patch
{
    private static void Postfix(Map map)
    {
        if (!TestManager.Scenario.EnableDebugMap)
            return;
        
        // reset to heavy terrain so building can be placed on a tiny map
        foreach (var allCell in map.AllCells)
        {
            map.terrainGrid.SetTerrain(allCell, TerrainDefOf.MetalTile);
        }
    }
}

[HarmonyPatch]
internal static class GenStep_RocksFromGrid_Generate_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // remove cave, mountain
        yield return AccessTools.Method(typeof(GenStep_RocksFromGrid), nameof(GenStep_RocksFromGrid.Generate)); 
        yield return AccessTools.Method(typeof(GenStep_RockChunks), nameof(GenStep_RockChunks.Generate)); 
    }
    
    private static bool Prefix()
    {
        if (!TestManager.Scenario.EnableDebugMap)
            return true;

        return false;
    }
}