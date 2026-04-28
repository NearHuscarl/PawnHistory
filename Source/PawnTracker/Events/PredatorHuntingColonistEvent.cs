using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

// TODO: generalize it, applied to all pawns, not just colonist
public record PredatorHuntingColonistEvent(Pawn Predator, Pawn Prey) : GameEventBase;

internal record PredatorHuntingColonistState(bool WasNotified);

[HarmonyPatch(typeof(JobDriver_PredatorHunt), "CheckWarnPlayerInterval")]
internal static class JobDriver_PredatorHunt_CheckWarnPlayerInterval_Patch
{
    public static void Prefix(JobDriver_PredatorHunt __instance, out PredatorHuntingColonistState __state)
    {
        var wasNotified = Accessor.JobDriver_PredatorHunt.NotifiedPlayerAttacking(__instance);
        __state = new PredatorHuntingColonistState(wasNotified);
    }

    public static void Postfix(JobDriver_PredatorHunt __instance, PredatorHuntingColonistState __state)
    {
        if (__state.WasNotified || !Accessor.JobDriver_PredatorHunt.NotifiedPlayerAttacking(__instance))
            return;

        var prey = __instance.Prey;
        var predator = Accessor.JobDriver.Pawn(__instance);
        if (prey == null || predator == null)
            return;

        GameEventBus.Publish(new PredatorHuntingColonistEvent(predator, prey));
    }
}
