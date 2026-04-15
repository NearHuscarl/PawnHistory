using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PlayerCaravanArriveEvent(List<Pawn> Pawns, string Caravan, CaravanArrivalAction ArrivalAction) : GameEventBase;

[HarmonyPatch]
internal static class CaravanArrivalAction_Arrived_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(CaravanArrivalAction_AttackSettlement), nameof(CaravanArrivalAction.Arrived)); 
        yield return AccessTools.Method(typeof(CaravanArrivalAction_Enter), nameof(CaravanArrivalAction.Arrived)); 
        yield return AccessTools.Method(typeof(CaravanArrivalAction_OfferGifts), nameof(CaravanArrivalAction.Arrived)); 
        yield return AccessTools.Method(typeof(CaravanArrivalAction_Trade), nameof(CaravanArrivalAction.Arrived)); 
        yield return AccessTools.Method(typeof(CaravanArrivalAction_VisitEscapeShip), nameof(CaravanArrivalAction.Arrived)); 
        yield return AccessTools.Method(typeof(CaravanArrivalAction_VisitPeaceTalks), nameof(CaravanArrivalAction.Arrived)); 
        yield return AccessTools.Method(typeof(CaravanArrivalAction_VisitSettlement), nameof(CaravanArrivalAction.Arrived)); 
        yield return AccessTools.Method(typeof(CaravanArrivalAction_VisitSite), nameof(CaravanArrivalAction.Arrived)); 
    }

    // caravan is destroyed if run in postfix in certain subclasses
    private static void Prefix(CaravanArrivalAction __instance, Caravan caravan)
    {
        GameEventBus.Publish(new PlayerCaravanArriveEvent(caravan.pawns.InnerListForReading, caravan.Label, __instance));
    }

    private static void Finalizer() => WandererJoinContext.Finalizer();
}
