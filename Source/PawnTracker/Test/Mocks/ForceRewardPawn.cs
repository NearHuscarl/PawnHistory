using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(RewardsGenerator), "DoGenerate")]
[HarmonyPriority(Priority.First)]
internal static class ForceRewardPawn
{
    private static void Postfix(ref List<Reward> __result)
    {
        var forcedPawn = TestManager.Scenario.ForceRewardPawnInQuest;
        if (forcedPawn == null)
            return;

        __result = [
            new Reward_Pawn
            {
                pawn = forcedPawn,
                arrivalMode = Reward_Pawn.ArrivalMode.WalkIn,
            }
        ];
    }
}
