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

internal class CasketDropEvent(Pawn pawn, Building_Casket casket, DropReason reason, Pawn opener) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Building_Casket Casket { get; } = casket;
    public DropReason Reason { get; } = reason;
    public Pawn Opener { get; } = opener;
}

class CasketDropContext
{
    public static readonly AccessTools.FieldRef<Building_Casket, ThingOwner> InnerContainerRef = AccessTools.FieldRefAccess<Building_Casket, ThingOwner>("innerContainer");

    public static void PrefixDrop(Building_Casket __instance, DropReason reason)
    {
        var innerContainer = InnerContainerRef(__instance);
        var ejector = Find.CurrentMap.mapPawns.AllPawnsSpawned.FirstOrDefault(p => p.CurJob?.targetA.Thing == __instance || p.CurJob?.targetB.Thing == __instance);

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

    static void Prefix(Building_Casket __instance)
    {
        CasketDropContext.PrefixDrop(__instance, DropReason.Ejected);
    }
}

[HarmonyPatch(typeof(Building_Casket), nameof(Building_Casket.Destroy))]
internal class Building_Casket_Destroy_Patch
{
    static void Prefix(Building_Casket __instance, DestroyMode mode)
    {
        var reason = mode == DestroyMode.Deconstruct ? DropReason.Ejected : DropReason.CasketDestroyed;
        CasketDropContext.PrefixDrop(__instance, reason);
    }
}