using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum QuestPawnArrivedMode
{
    WalkIn,
    DropPod,
    AddedToCaravan,
    Shuttle,
}

public record QuestPawnArrivedEvent(List<Pawn> Pawns, Quest Quest, QuestPawnArrivedMode ArrivalMode) : GameEventBase;

[HarmonyPatch(typeof(QuestPart_PawnsArrive), nameof(QuestPart_PawnsArrive.Notify_QuestSignalReceived))]
internal static class QuestPart_PawnsArrive_Notify_QuestSignalReceived_Patch
{
    private static void Postfix(QuestPart_PawnsArrive __instance, Signal signal)
    {
        if (signal.tag != __instance.inSignal)
            return;
        if (__instance.mapParent is not { HasMap: true })
            return;
        var pawns = __instance.pawns
            .Where(pawn => pawn.MapHeld == __instance.mapParent.Map)
            .ToList();
        
        var arriveMode = __instance.arrivalMode.defName.Contains("Drop") ? QuestPawnArrivedMode.DropPod : QuestPawnArrivedMode.WalkIn;
        GameEventBus.Publish(new QuestPawnArrivedEvent(pawns, __instance.quest, arriveMode));
    }
}

[HarmonyPatch(typeof(QuestPart_DropPods), nameof(QuestPart_DropPods.Notify_QuestSignalReceived))]
internal static class QuestPart_DropPods_Notify_QuestSignalReceived_Patch
{
    private static void Postfix(QuestPart_DropPods __instance, Signal signal)
    {
        if (signal.tag != __instance.inSignal)
            return;
        if (__instance.mapParent is not { HasMap: true })
            return;

        var tmpThings = Accessor.QuestPart_DropPods.TmpThingsToDrop(__instance);
        if (!tmpThings.Any())
            return;

        var pawns = tmpThings.OfType<Pawn>().ToList();
        GameEventBus.Publish(new QuestPawnArrivedEvent(pawns, __instance.quest, QuestPawnArrivedMode.DropPod));
    }
}

[HarmonyPatch(typeof(QuestPart_GiveToCaravan), nameof(QuestPart_GiveToCaravan.Notify_QuestSignalReceived))]
internal static class QuestPart_GiveToCaravan_Notify_QuestSignalReceived_Patch
{
    private static void Postfix(QuestPart_GiveToCaravan __instance, Signal signal)
    {
        if (signal.tag != __instance.inSignal)
            return;
        
        var pawns = __instance.Things.OfType<Pawn>().ToList();
        GameEventBus.Publish(new QuestPawnArrivedEvent(pawns, __instance.quest, QuestPawnArrivedMode.AddedToCaravan));
    }
}

// Do not patch QuestPart_AddShipJob.Notify_QuestSignalReceived, it only schedules the job and might not work as expected.
[HarmonyPatch(typeof(ShipJob_Arrive), nameof(ShipJob_Arrive.TryStart))]
internal static class ShipJob_Arrive_Start_Patch
{
    private static void Postfix(ShipJob_Arrive __instance, bool __result)
    {
        if (!__result)
            return;

        var ship = __instance.transportShip;
        if (!QuestHelper.TryGetRelatedQuestFrom(ship, out var quest))
            return;

        var pawns = ship.TransporterComp.innerContainer.OfType<Pawn>().ToList();
        GameEventBus.Publish(new QuestPawnArrivedEvent(pawns, quest, QuestPawnArrivedMode.Shuttle));
    }
}