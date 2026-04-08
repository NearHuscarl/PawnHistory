using HarmonyLib;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public record MentalStateEndedEvent(Pawn Pawn, MentalState MentalState) : GameEventBase;

[HarmonyPatch(typeof(MentalState), nameof(MentalState.RecoverFromState))]
static class RecoverFromState_RecoverFromState_Patch
{
    static void Postfix(MentalState __instance)
    {
        GameEventBus.Publish(new MentalStateEndedEvent(__instance.pawn, __instance));
    }
}