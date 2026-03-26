using HarmonyLib;
using LudeonTK;
using System;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.DebugTools;

[HarmonyPatch(typeof(HediffComp_GetsPermanent), nameof(HediffComp_GetsPermanent.PreFinalizeInjury))]
internal class HediffComp_GetsPermanent_PreFinalizeInjury_Patch
{
    static void Postfix(HediffComp_GetsPermanent __instance)
    {
        if (NearDebugSettings.ForceInjuryScar)
            __instance.IsPermanent = true;
    }
}

[HarmonyPatch(typeof(HediffComp_GetsPermanent), nameof(HediffComp_GetsPermanent.CompPostInjuryHeal))]
internal class HediffComp_GetsPermanent_CompPostInjuryHeal_Patch
{
    static void Prefix(HediffComp_GetsPermanent __instance, ref float amount)
    {
        if (!NearDebugSettings.ForcePostHealScar)
            return;

        var injury = __instance.parent;
        __instance.permanentDamageThreshold = injury.Severity + amount;
    }
}

[HarmonyPatch(typeof(DebugTabMenu_Settings), nameof(DebugTabMenu_Settings.InitActions))]
public static class Patch_DebugTabMenu_Settings_InitActions
{
    private static readonly Action<DebugTabMenu_Settings, FieldInfo, string> AddNode =
       AccessTools.MethodDelegate<Action<DebugTabMenu_Settings, FieldInfo, string>>(AccessTools.Method(typeof(DebugTabMenu_Settings), "AddNode"));

    static void Postfix(DebugTabMenu_Settings __instance, DebugActionNode __result)
    {
        var fields = typeof(NearDebugSettings).GetFields();

        foreach (var field in fields)
        {
            AddNode(__instance, field, "NearSettings");
        }
    }
}

internal class NearDebugSettings
{
    public static bool ForceInjuryScar = false;
    public static bool ForcePostHealScar = false;
}
