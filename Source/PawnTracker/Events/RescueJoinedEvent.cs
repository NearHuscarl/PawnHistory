using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RescueJoinedEvent(Pawn Pawn) : GameEventBase;

internal record RescueJoinedState(Faction PawnFaction);

[HarmonyPatch(typeof(Pawn_GuestTracker), "Notify_PawnUndowned")]
internal static class Pawn_GuestTracker_Notify_PawnUndowned_Patch
{
    public static void Prefix(Pawn_GuestTracker __instance, out RescueJoinedState __state)
    {
        __state = new RescueJoinedState(Accessor.Pawn_GuestTracker.Pawn(__instance).Faction);
    }

    public static void Postfix(Pawn_GuestTracker __instance, RescueJoinedState __state)
    {
        var pawn = Accessor.Pawn_GuestTracker.Pawn(__instance);

        if (__state.PawnFaction != pawn.Faction && pawn.Faction == Faction.OfPlayer)
            GameEventBus.Publish(new RescueJoinedEvent(pawn));
    }
}
