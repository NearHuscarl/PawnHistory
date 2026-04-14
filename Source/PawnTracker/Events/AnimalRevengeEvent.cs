using HarmonyLib;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public enum RevengeReason
{
    Hunt,
    Tame,
}

public record AnimalRevengeEvent(List<Pawn> Animals, Pawn Instigator, RevengeReason Reason) : GameEventBase;

internal static class AnimalRevengeContext
{
    public static bool PendingRevenge = false;
    public static readonly List<Pawn> Manhunters = [];
    public static void Clear()
    {
        PendingRevenge = false;
        Manhunters.Clear();
    }
}

// Call order:
// - Pawn_MindState.StartManhunterBecauseOfPawnAction() prefix
//  - MentalStateHandler.TryStartMentalState()
// - Pawn_MindState.StartManhunterBecauseOfPawnAction() postfix

[HarmonyPatch(typeof(Pawn_MindState), "StartManhunterBecauseOfPawnAction")]
internal static class Pawn_MindState_StartManhunterBecauseOfPawnAction_Patch
{
    public static void Prefix()
    {
        AnimalRevengeContext.PendingRevenge = true;
    }

    public static void Postfix(Pawn instigator, bool causedByDamage)
    {
        if (!AnimalRevengeContext.PendingRevenge)
            return;

        var reason = causedByDamage ? RevengeReason.Hunt : RevengeReason.Tame;
        GameEventBus.Publish(new AnimalRevengeEvent(AnimalRevengeContext.Manhunters, instigator, reason));
    }

    private static void Finalizer() => AnimalRevengeContext.Clear();
}

[HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
internal static class MentalStateHandler_TryStartMentalState_Patch
{
    public static void Postfix(bool __result, MentalStateHandler __instance)
    {
        if (!AnimalRevengeContext.PendingRevenge)
            return;
        if (!__result)
            return;

        var pawn = Accessor.MentalStateHandler.Pawn(__instance);
        AnimalRevengeContext.Manhunters.Add(pawn);
    }
}
