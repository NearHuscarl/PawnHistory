using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum PawnLostMapType
{
    Home,
    Caravan
}

public record PawnLostEvent(Pawn Pawn, PawnLostMapType MapType, string MapName, bool IsKidnapped) : GameEventBase;

internal record PawnLostState(List<(Pawn, bool)> PawnDataList, PawnLostMapType MapType, string WorldObject);

[HarmonyPatch(typeof(MapDeiniter), "PassPawnsToWorld")]
internal static class MapDeiniter_PassPawnsToWorld_Patch
{
    private static void Prefix(Map map, out PawnLostState __state)
    {
        var mapType = map.IsPlayerHome || map.IsPocketMap ? PawnLostMapType.Home : PawnLostMapType.Caravan;
        var allPawns = map.mapPawns.AllPawns.ToList();
        var pawnDataList = allPawns
            .Where(p => p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
            .Select(p => (p, map.ParentFaction.HostileTo(p.Faction)))
            .ToList();
        
        __state = new PawnLostState(pawnDataList, mapType, map.Parent.ColoredLabel);
    }

    private static void Postfix(PawnLostState __state)
    {
        foreach (var (pawn, isKidnapped) in __state.PawnDataList)
        {
            if (!pawn.IsWorldPawn())
                continue; // PassPawnsToWorld() throws
            GameEventBus.Publish(new PawnLostEvent(pawn, __state.MapType, __state.WorldObject, isKidnapped));
        }
    }
}
