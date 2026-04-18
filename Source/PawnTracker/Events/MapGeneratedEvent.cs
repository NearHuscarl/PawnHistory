using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record MapGeneratedEvent(Map Map, MapParent MapParent) : GameEventBase;

[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
internal static class MapGenerator_GenerateMap_Patch
{
    private static void Postfix(Map __result, MapParent parent)
    {
        GameEventBus.Publish(new MapGeneratedEvent(__result, parent));
    }
}