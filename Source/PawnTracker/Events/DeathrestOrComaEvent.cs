using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DeathrestOrComaEvent(Pawn Pawn, DeathrestStartReason Reason, bool IsDeathRest) : GameEventBase;

[HarmonyPatch(typeof(SanguophageUtility), nameof(SanguophageUtility.TryStartDeathrest))]
internal static class SanguophageUtility_TryStartDeathrest_Patch
{
    public static void Postfix(Pawn pawn, DeathrestStartReason reason, bool __result)
    {
        if (!__result)
            return;
        
        GameEventBus.Publish(new DeathrestOrComaEvent(pawn, reason, true));
    }
}

[HarmonyPatch(typeof(SanguophageUtility), nameof(SanguophageUtility.TryStartRegenComa))]
internal static class SanguophageUtility_TryStartRegenComa_Patch
{
    public static void Postfix(Pawn pawn, DeathrestStartReason reason, bool __result)
    {
        if (!__result)
            return;
        
        GameEventBus.Publish(new DeathrestOrComaEvent(pawn, reason, false));
    }
}
