using HarmonyLib;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(HediffComp_GetsPermanent), nameof(HediffComp_GetsPermanent.PreFinalizeInjury))]
internal class HediffComp_GetsPermanent_PreFinalizeInjury_Patch
{
    private static void Postfix(HediffComp_GetsPermanent __instance)
    {
        if (TestManager.Scenario.ForceInjuryScar)
            __instance.IsPermanent = true;
    }
}

[HarmonyPatch(typeof(HediffComp_GetsPermanent), nameof(HediffComp_GetsPermanent.CompPostInjuryHeal))]
internal class HediffComp_GetsPermanent_CompPostInjuryHeal_Patch
{
    private static void Prefix(HediffComp_GetsPermanent __instance, float amount)
    {
        if (!TestManager.Scenario.ForcePostHealScar)
            return;

        var injury = __instance.parent;
        __instance.permanentDamageThreshold = injury.Severity + amount;
    }
}