using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DeepMineralFoundEvent(Pawn Pawn, ThingDef Material, IntVec3 Position) : GameEventBase;

internal sealed class DeepMineralScanContext(Map map, Pawn pawn)
{
    public Map Map { get; } = map;
    public Pawn Pawn { get; } = pawn;
    public List<IntVec3> Positions { get; } = [];
    public ThingDef Material { get; private set; }

    public void Capture(IntVec3 position, ThingDef material)
    {
        Positions.Add(position);
        Material = material;
    }
}

[HarmonyPatch(typeof(CompDeepScanner), "DoFind")]
internal static class CompDeepScanner_DoFind_Patch
{
    internal static DeepMineralScanContext ActiveScan;

    public static void Prefix(Pawn worker)
    {
        ActiveScan = new DeepMineralScanContext(worker.Map, worker);
    }

    public static void Postfix()
    {
        if (ActiveScan.Positions.Count == 0)
            return;
        
        GameEventBus.Publish(new DeepMineralFoundEvent(ActiveScan.Pawn, ActiveScan.Material, IntVec3Helper.GetCenter(ActiveScan.Positions)));
    }

    public static void Finalizer() => ActiveScan = null;
}

[HarmonyPatch(typeof(DeepResourceGrid), nameof(DeepResourceGrid.SetAt))]
internal static class DeepResourceGrid_SetAt_Patch
{
    public static void Prefix(DeepResourceGrid __instance, IntVec3 c, ThingDef def, int count)
    {
        var capture = CompDeepScanner_DoFind_Patch.ActiveScan;
        if (capture == null)
            return;

        if (Accessor.DeepResourceGrid.Map(__instance) != capture.Map || def == null || count <= 0)
            return;

        capture.Capture(c, def);
    }
}
