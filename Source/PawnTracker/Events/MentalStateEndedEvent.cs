using HarmonyLib;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public class MentalStateEndedEvent(Pawn pawn, MentalState mentalState) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public MentalState MentalState { get; } = mentalState;
}

[HarmonyPatch(typeof(MentalState), nameof(MentalState.RecoverFromState))]
static class RecoverFromState_RecoverFromState_Patch
{
    static void Postfix(MentalState __instance)
    {
        GameEventBus.Publish(new MentalStateEndedEvent(__instance.pawn, __instance));
    }
}