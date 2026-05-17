using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record FriendlyTrapHitEvent(Pawn Pawn) : GameEventBase;

[HarmonyPatch(typeof(Building_Trap), "CheckSpring")]
internal class Building_Trap_CheckSpring_Patch
{
    private static void Postfix(Building_Trap __instance, Pawn p)
    {
        if (p.Faction != Faction.OfPlayer && p.HostFaction != Faction.OfPlayer)
            return;
        if (!__instance.Destroyed)
            return;

        GameEventBus.Publish(new FriendlyTrapHitEvent(p));
    }
}
