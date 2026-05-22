using HarmonyLib;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.InbredChanceFromParents))]
internal static class ForceInbred
{
    private static void Postfix(ref float __result)
    {
        if (TestManager.Scenario.ForceInbred)
            __result = 1f;
    }
}
