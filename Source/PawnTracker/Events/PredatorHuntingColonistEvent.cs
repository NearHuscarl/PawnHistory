using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PredatorHuntingColonistEvent(Pawn Predator, Pawn Prey) : GameEventBase;

internal static class PredatorHuntingColonistContext
{
    public static bool WasNotified;
}

[HarmonyPatch(typeof(JobDriver_PredatorHunt), "CheckWarnPlayerInterval")]
internal static class JobDriver_PredatorHunt_CheckWarnPlayerInterval_Patch
{
    public static void Prefix(JobDriver_PredatorHunt __instance)
    {
        PredatorHuntingColonistContext.WasNotified = Accessor.JobDriver_PredatorHunt.NotifiedPlayerAttacking(__instance);
    }

    public static void Postfix(JobDriver_PredatorHunt __instance)
    {
        if (PredatorHuntingColonistContext.WasNotified || !Accessor.JobDriver_PredatorHunt.NotifiedPlayerAttacking(__instance))
            return;

        var prey = __instance.Prey;
        var predator = Accessor.JobDriver.Pawn(__instance);
        if (prey == null || predator == null)
            return;

        GameEventBus.Publish(new PredatorHuntingColonistEvent(predator, prey));
    }
}
