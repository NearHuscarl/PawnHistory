using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace PawnHistory.Source.DebugTools;

[HarmonyPatch(typeof(TickManager), nameof(TickManager.Pause))]
internal class TickManager_Pause_Patch
{
    private static void Postfix(TickManager __instance)
    {
        if (NearDebugSettings.NeverEverEverPause)
        {
            if (__instance.CurTimeSpeed == TimeSpeed.Paused)
                __instance.CurTimeSpeed = __instance.prePauseTimeSpeed;
        }
    }
}

[HarmonyPatch(typeof(DebugTabMenu_Settings), nameof(DebugTabMenu_Settings.InitActions))]
internal static class Patch_DebugTabMenu_Settings_InitActions_Patch
{
    private static readonly Action<DebugTabMenu_Settings, FieldInfo, string> AddNode =
       AccessTools.MethodDelegate<Action<DebugTabMenu_Settings, FieldInfo, string>>(AccessTools.Method(typeof(DebugTabMenu_Settings), "AddNode"));

    private static void Postfix(DebugTabMenu_Settings __instance, DebugActionNode __result)
    {
        var fields = typeof(NearDebugSettings).GetFields();

        foreach (var field in fields)
        {
            AddNode(__instance, field, "NearSettings");
        }
    }
}

[HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.GetManhunterChanceFactorForInstigator))]
internal static class PawnUtility_GetManhunterChanceFactorForInstigator_Patch
{
    private static void Postfix(ref float __result)
    {
        if (NearDebugSettings.ForceManhunterChance)
            __result = 10f;
    }
}

[HarmonyPatch(typeof(Building_Trap), "SpringChance")]
internal static class Building_Trap_SpringChance_Patch
{
    private static void Postfix(ref float __result)
    {
        if (NearDebugSettings.ForceSpringTrap)
            __result = 10f;
    }
}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance))]
internal static class InteractionWorker_RomanceAttempt_SuccessChance_Patch
{
    private static void Postfix(ref float __result)
    {
        if (NearDebugSettings.ForceRomanceSuccess)
            __result = 10f;
        else if (NearDebugSettings.ForceRomanceRejection)
            __result = 0f;
    }
}

[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.AcceptanceChance))]
internal static class InteractionWorker_MarriageProposal_AcceptanceChance_Patch
{
    private static void Postfix(ref float __result)
    {
        if (NearDebugSettings.ForceMarriageProposalAccepted)
            __result = 10f;
        else if (NearDebugSettings.ForceMarriageProposalRejected)
            __result = 0f;
    }
}

[HarmonyPatch(typeof(QuestNode_EndGame_ShipEscape_FindShipTile), "TryFindDestinationTile", [typeof(PlanetTile), typeof(PlanetTile)], [ArgumentType.Normal, ArgumentType.Out])]
internal class QuestNode_EndGame_ShipEscape_FindShipTile_TryFindDestinationTile_Patch
{
    private static void Postfix(ref PlanetTile tile)
    {
        if (NearDebugSettings.ShipEscapeSpawnNearby)
        {
            var homeTile = Find.AnyPlayerHomeMap.Tile;
            var neighbors = new List<PlanetTile>();
            Find.WorldGrid.GetTileNeighbors(homeTile, neighbors);
            tile = neighbors.FirstOrDefault(t => Find.WorldGrid[t].HillinessLabel != Hilliness.Impassable);
        }
    }
}

internal class NearDebugSettings
{
    public static bool NeverEverEverPause = false;
    public static bool ForceManhunterChance = false;
    public static bool ForceSpringTrap = false;
    public static bool ForceRomanceSuccess = false;
    public static bool ForceRomanceRejection = false;
    public static bool ForceMarriageProposalAccepted = false;
    public static bool ForceMarriageProposalRejected = false;
    public static bool ShipEscapeSpawnNearby = false;
    public static bool LogDebug = false;
    public static bool DrawHistoryCardState = false;
}
