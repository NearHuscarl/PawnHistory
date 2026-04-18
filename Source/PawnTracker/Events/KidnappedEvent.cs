using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record KidnappedEvent(Pawn Victim, Pawn Kidnapper) : GameEventBase;

internal static class KidnappedContext
{
    public static bool ShouldPublish;
}

[HarmonyPatch(typeof(KidnappedPawnsTracker), nameof(KidnappedPawnsTracker.Kidnap))]
internal static class KidnappedPawnsTracker_Kidnap_Patch
{
    private static void Prefix(KidnappedPawnsTracker __instance, Pawn pawn, Pawn kidnapper)
    {
        var inGameCheckPass = !__instance.KidnappedPawnsListForReading.Contains(pawn) && pawn.Faction != kidnapper.Faction;
        KidnappedContext.ShouldPublish = pawn != null && kidnapper != null && inGameCheckPass;
    }

    private static void Postfix(Pawn pawn, Pawn kidnapper)
    {
        if (!KidnappedContext.ShouldPublish)
            return;

        GameEventBus.Publish(new KidnappedEvent(pawn, kidnapper));
    }

    private static void Finalizer() => KidnappedContext.ShouldPublish = false;
}
