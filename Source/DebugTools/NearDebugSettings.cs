using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.DebugTools;

[HarmonyPatch(typeof(HediffComp_GetsPermanent), nameof(HediffComp_GetsPermanent.PreFinalizeInjury))]
internal class HediffComp_GetsPermanent_PreFinalizeInjury_Patch
{
    private static void Postfix(HediffComp_GetsPermanent __instance)
    {
        if (NearDebugSettings.ForceInjuryScar)
            __instance.IsPermanent = true;
    }
}

[HarmonyPatch(typeof(HediffComp_GetsPermanent), nameof(HediffComp_GetsPermanent.CompPostInjuryHeal))]
internal class HediffComp_GetsPermanent_CompPostInjuryHeal_Patch
{
    private static void Prefix(HediffComp_GetsPermanent __instance, float amount)
    {
        if (!NearDebugSettings.ForcePostHealScar)
            return;

        var injury = __instance.parent;
        __instance.permanentDamageThreshold = injury.Severity + amount;
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

internal class NearDebugSettings
{
    public static bool ForceInjuryScar = false;
    public static bool ForcePostHealScar = false;
    public static bool ForceManhunterChance = false;
    public static bool ForceSpringTrap = false;
    public static bool ForceRomanceSuccess = false;
    public static bool ForceMarriageProposalAccepted = false;
    public static bool ForceMarriageProposalRejected = false;
}
