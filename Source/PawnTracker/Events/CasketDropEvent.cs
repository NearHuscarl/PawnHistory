using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;

namespace PawnHistory.Source.PawnTracker.Events;

public enum DropReason
{
    Ejected,
    CasketDestroyed,
}

public record CasketDropEvent(Pawn Pawn, Building_Casket Casket, DropReason Reason, Pawn Opener) : GameEventBase;

internal static class CasketDropContext
{
    public static void PrefixDrop(Building_Casket __instance, DropReason reason)
    {
        var innerContainer = Accessor.Building_Casket.InnerContainer(__instance);
        var ejector = __instance.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(p => p.CurJob?.targetA.Thing == __instance || p.CurJob?.targetB.Thing == __instance);

        foreach (var thing in innerContainer)
        {
            if (thing is Pawn pawn)
                GameEventBus.Publish(new CasketDropEvent(pawn, __instance, reason, ejector));
            if (thing is Corpse corpse)
                GameEventBus.Publish(new CasketDropEvent(corpse.InnerPawn, __instance, reason, ejector));
        }
    }
}

[HarmonyPatch(typeof(Building_Casket), nameof(Building_Casket.EjectContents))]
internal class Building_Casket_EjectContents_Patch
{
    private static void Prefix(Building_Casket __instance)
    {
        CasketDropContext.PrefixDrop(__instance, DropReason.Ejected);
    }
}

[HarmonyPatch(typeof(Building_Casket), nameof(Building_Casket.Destroy))]
internal class Building_Casket_Destroy_Patch
{
    private static void Prefix(Building_Casket __instance, DestroyMode mode)
    {
        var reason = mode == DestroyMode.Deconstruct ? DropReason.Ejected : DropReason.CasketDestroyed;
        CasketDropContext.PrefixDrop(__instance, reason);
    }
}