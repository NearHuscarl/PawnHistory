using HarmonyLib;
using RimWorld.QuestGen;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(QuestNode_Chance), "RunInt")]
internal static class AlwaysHaveHelpersInQuest
{
    private static void Prefix(QuestNode_Chance __instance)
    {
        if (!TestManager.Scenario.AlwaysHaveHelpersInQuest)
            return;
        
        if (Accessor.SlateRef<float>.slateRef(ref __instance.chance) != "$helpersChance")
            return;

        __instance.chance = 1f;
    }
}