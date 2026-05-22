using HarmonyLib;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ChanceMomDiesDuringBirth))]
internal static class ForceMotherDeathDuringBirth
{
    private static void Postfix(ref float __result)
    {
        if (TestManager.Scenario.ForceMotherDeathDuringBirth)
            __result = 1f;
    }
}
