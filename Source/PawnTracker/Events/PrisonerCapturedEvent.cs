using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PrisonerCapturedEvent(Pawn Prisoner, Pawn Captor, Room Room) : GameEventBase;

[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.CapturedBy))]
internal class Pawn_GuestTracker_CapturedBy_Patch
{
    private static void Postfix(Pawn_GuestTracker __instance, Pawn byPawn = null)
    {
        if (__instance.GuestStatus != GuestStatus.Prisoner)
            return;

        // Auto captured by moving a non-prisoner to a caravan of different faction.
        if (byPawn == null)
            return;

        var prisoner = Accessor.Pawn_GuestTracker.Pawn(__instance);
        if (prisoner?.ownership?.OwnedBed?.GetRoom() is not { } room)
            return;

        GameEventBus.Publish(new PrisonerCapturedEvent(prisoner, byPawn, room));
    }
}