using HarmonyLib;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record EatenEvent(Pawn Eaten, Pawn Eater) : GameEventBase;

[HarmonyPatch(typeof(Corpse), "IngestedCalculateAmounts")]
internal static class Corpse_IngestedCalculateAmounts_Patch
{
    private static void Postfix(Corpse __instance, Pawn ingester, ref int numTaken)
    {
        if (ingester == null || numTaken != 1)
            return;

        GameEventBus.Publish(new EatenEvent(__instance.InnerPawn, ingester));
    }
}
