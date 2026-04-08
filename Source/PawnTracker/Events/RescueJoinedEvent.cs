using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RescueJoinedEvent(Pawn Pawn) : GameEventBase;

class RescueJoinedContext
{
    public static Faction PawnFaction;
}

[HarmonyPatch(typeof(Pawn_GuestTracker), "Notify_PawnUndowned")]
public static class Pawn_GuestTracker_Notify_PawnUndowned_Patch
{
    public static void Prefix(Pawn_GuestTracker __instance)
    {
        RescueJoinedContext.PawnFaction = Accessor.Pawn_GuestTracker.Pawn(__instance).Faction;
    }

    public static void Postfix(Pawn_GuestTracker __instance)
    {
        var pawn = Accessor.Pawn_GuestTracker.Pawn(__instance);

        if (RescueJoinedContext.PawnFaction != pawn.Faction && pawn.Faction == Faction.OfPlayer)
            GameEventBus.Publish(new RescueJoinedEvent(pawn));
    }
}
