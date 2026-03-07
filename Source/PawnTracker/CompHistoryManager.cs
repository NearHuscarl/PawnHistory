using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
internal class Game_LoadGame_Patch
{
    static void Prefix() => CompHistoryManager.ClearAll();
}

internal class CompHistoryManager
{
    public static readonly Dictionary<int, CompHistory> CompCache = [];
    public static HashSet<int> TrackingDefHash = [];

    public static CompHistory GetComp(Pawn pawn)
    {
        if (pawn == null)
            return null;
        if (CompCache.TryGetValue(pawn.thingIDNumber, out CompHistory compCached))
            return compCached;
        if (!TrackingDefHash.Contains(pawn.def.shortHash))
            return null;
        var comp = pawn.GetComp<CompHistory>();
        if (comp != null)
            CompCache.Add(pawn.thingIDNumber, comp);
        return comp;
    }

    public static void ClearAll() => CompCache.Clear();
}
