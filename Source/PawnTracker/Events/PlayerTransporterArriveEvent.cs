using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PlayerTransporterArriveEvent(List<Pawn> Pawns, TransportersArrivalAction ArrivalAction, PlanetTile Tile) : GameEventBase;

internal static class PlayerTransporterArriveContext
{
    public static int CallDepth;
}

[HarmonyPatch]
internal static class TransportersArrivalAction_Arrived_Patch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // TransportersArrivalAction_Trade > TransportersArrivalAction_VisitSettlement > TransportersArrivalAction_FormCaravan > TransportersArrivalAction
        // TransportersArrivalAction_Trade: handled in caravan recorder
        yield return AccessTools.Method(typeof(TransportersArrivalAction_AttackSettlement), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_GiveGift), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_GiveToCaravan), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_FormCaravan), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_LandInSpecificCell), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_Trade), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_VisitSettlement), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_VisitSite), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_VisitSpace), nameof(TransportersArrivalAction.Arrived));
        yield return AccessTools.Method(typeof(TransportersArrivalAction_TransportShip), nameof(TransportersArrivalAction.Arrived));
    }

    private static void Prefix(TransportersArrivalAction __instance, List<ActiveTransporterInfo> transporters, PlanetTile tile)
    {
        PlayerTransporterArriveContext.CallDepth++;

        if (PlayerTransporterArriveContext.CallDepth > 1)
            return;
        
        var pawns = transporters
            .SelectMany(transporter => transporter.innerContainer.OfType<Pawn>())
            .ToList();

        if (pawns.Count == 0)
            return;

        GameEventBus.Publish(new PlayerTransporterArriveEvent(pawns, __instance, tile));
    }
    
    private static void Postfix() => PlayerTransporterArriveContext.CallDepth--;
}
