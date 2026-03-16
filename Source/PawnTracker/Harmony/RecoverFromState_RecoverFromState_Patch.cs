using HarmonyLib;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Harmony;

[HarmonyPatch(typeof(MentalState), nameof(MentalState.RecoverFromState))]
static class RecoverFromState_RecoverFromState_Patch
{
    static void Postfix(MentalState __instance)
    {
        GameEventBus.Publish(new MentalStateEndedEvent(__instance.pawn, __instance));
    }
}