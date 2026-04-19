using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record VisitorLeftGiftEvent(Pawn Giver, Faction Faction, List<Thing> GiftedItems) : GameEventBase;

[HarmonyPatch(typeof(VisitorGiftForPlayerUtility), nameof(VisitorGiftForPlayerUtility.GiveGift))]
internal static class VisitorGiftForPlayerUtility_GiveGift_Patch
{
    public static void Postfix(List<Pawn> possibleGivers, Faction faction, List<Thing> gifts)
    {
        var giftGiver = Accessor.VisitorGiftForPlayerUtility.GetGiftGiver(possibleGivers, faction);
        if (giftGiver == null)
            return;

        var gifts2 = gifts.Where(i => !i.Destroyed).ToList();
        GameEventBus.Publish(new VisitorLeftGiftEvent(giftGiver, faction, gifts2));
    }
}
