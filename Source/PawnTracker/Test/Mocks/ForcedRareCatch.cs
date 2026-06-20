using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(FishingUtility), nameof(FishingUtility.GetCatchesFor))]
internal static class ForcedRareCatch
{
    private static bool Prefix(ref List<Thing> __result, ref bool rare)
    {
        var forcedCatch = TestManager.Scenario.ForcedRareCatch;
        if (forcedCatch == null)
            return true;

        rare = true;
        __result = [forcedCatch];
        return false;
    }
}
