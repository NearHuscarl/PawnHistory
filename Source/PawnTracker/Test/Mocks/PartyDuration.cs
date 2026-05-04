using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(LordJob_Joinable_Party), MethodType.Constructor, typeof(IntVec3), typeof(Pawn), typeof(GatheringDef))]
internal static class PartyDuration
{
    private static void Postfix(LordJob_Joinable_Party __instance)
    {
        if (TestManager.Scenario.PartyDuration == 0)
            return;
        DurationTicks(__instance) = TestManager.Scenario.PartyDuration;
    }

    private static readonly AccessTools.FieldRef<LordJob_Joinable_Gathering, int> DurationTicks = AccessTools.FieldRefAccess<LordJob_Joinable_Gathering, int>("durationTicks");
}