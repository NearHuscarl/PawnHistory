using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class PrisonerCapturedEvent(Pawn prisoner, Pawn captor, Room room) : GameEventBase
{
    public Pawn Prisoner { get; } = prisoner;
    public Pawn Captor { get; } = captor;
    public Room Room { get; } = room;
}

[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.CapturedBy))]
internal class Pawn_GuestTracker_CapturedBy_Patch
{
    static readonly AccessTools.FieldRef<Pawn_GuestTracker, Pawn> PawnRef = AccessTools.FieldRefAccess<Pawn_GuestTracker, Pawn>("pawn");

    static void Postfix(Pawn_GuestTracker __instance, Pawn byPawn = null)
    {
        if (__instance.GuestStatus != GuestStatus.Prisoner)
            return;

        // Auto captured by moving a non-prisoner to a caravan of different faction.
        if (byPawn == null)
            return;

        var prisoner = PawnRef(__instance);
        if (prisoner?.ownership?.OwnedBed?.GetRoom() is not { } room)
            return;

        GameEventBus.Publish(new PrisonerCapturedEvent(prisoner, byPawn, room));
    }
}