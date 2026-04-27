using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record KidnappedEvent(Pawn Victim, Faction KidnapFaction, Pawn Kidnapper) : GameEventBase;

internal record KidnappedState(bool ShouldPublish);

[HarmonyPatch(typeof(KidnappedPawnsTracker), nameof(KidnappedPawnsTracker.Kidnap))]
internal static class KidnappedPawnsTracker_Kidnap_Patch
{
    private static void Prefix(KidnappedPawnsTracker __instance, out KidnappedState __state, Pawn pawn)
    {
        var kidnapFaction = Accessor.KidnappedPawnsTracker.Faction(__instance);
        var shouldPublish = !__instance.KidnappedPawnsListForReading.Contains(pawn) && pawn.Faction != kidnapFaction;
        __state =  new KidnappedState(shouldPublish);
    }

    private static void Postfix(KidnappedPawnsTracker __instance, KidnappedState __state, Pawn pawn, Pawn kidnapper)
    {
        if (!__state.ShouldPublish)
            return;

        var kidnapFaction = Accessor.KidnappedPawnsTracker.Faction(__instance);
        GameEventBus.Publish(new KidnappedEvent(pawn, kidnapFaction, kidnapper));
    }
}
