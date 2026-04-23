using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum QuestPawnArrivedMode
{
    WalkIn,
    DropPod,
    AddedToCaravan,
}

public record QuestPawnArrivedEvent(Pawn Pawn, List<Pawn> Group, Quest Quest, QuestPawnArrivedMode ArrivalMode) : GameEventBase;

[HarmonyPatch(typeof(QuestPart_PawnsArrive), nameof(QuestPart_PawnsArrive.Notify_QuestSignalReceived))]
internal static class QuestPart_PawnsArrive_Notify_QuestSignalReceived_Patch
{
    private static void Postfix(QuestPart_PawnsArrive __instance, Signal signal)
    {
        if (signal.tag != __instance.inSignal)
            return;
        if (__instance.mapParent is not { HasMap: true })
            return;

        var arrivedGroup = __instance.pawns
            .Where(pawn => pawn.MapHeld == __instance.mapParent.Map)
            .ToList();
        
        var arriveMode = __instance.arrivalMode.defName.Contains("Drop") ? QuestPawnArrivedMode.DropPod : QuestPawnArrivedMode.WalkIn;
        foreach (var pawn in arrivedGroup)
        {
            GameEventBus.Publish(new QuestPawnArrivedEvent(pawn, arrivedGroup, __instance.quest, arriveMode));
        }
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

        var arrivedGroup = tmpThings.OfType<Pawn>().ToList();

        foreach (var pawn in arrivedGroup)
        {
            GameEventBus.Publish(new QuestPawnArrivedEvent(pawn, arrivedGroup, __instance.quest, QuestPawnArrivedMode.DropPod));
        }
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

        foreach (var pawn in pawns)
        {
            GameEventBus.Publish(new QuestPawnArrivedEvent(pawn, pawns, __instance.quest, QuestPawnArrivedMode.AddedToCaravan));
        }
    }
}
