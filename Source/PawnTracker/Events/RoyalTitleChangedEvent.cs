using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RoyalTitleChangedEvent(Pawn Pawn, Faction Faction, RoyalTitleDef PreviousTitle, RoyalTitleDef NewTitle) : GameEventBase;

[HarmonyPatch(typeof(Pawn_RoyaltyTracker), "OnPostTitleChanged")]
internal static class Pawn_RoyaltyTracker_OnPostTitleChanged_Patch
{
    private static void Postfix(Pawn_RoyaltyTracker __instance, Faction faction, RoyalTitleDef prevTitle, RoyalTitleDef newTitle)
    {
        GameEventBus.Publish(new RoyalTitleChangedEvent(__instance.pawn, faction, prevTitle, newTitle));
    }
}
