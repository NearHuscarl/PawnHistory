using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public enum MentalBreakCause
{
    Other,
    Mood,
    Hediff,
    Trait,
}

public record MentalBreakReason(MentalBreakCause Cause, string InGameReason, Hediff Hediff = null, string Trait = null);

public record MentalBreakStartedEvent(Pawn Pawn, MentalBreakReason Reason, MentalBreakDef MentalBreak, MentalStateDef MentalState = null, Pawn Target = null) : GameEventBase;

internal class MentalBreakContext
{
    public static readonly Dictionary<Pawn, (MentalStateDef mentalState, string reason, bool causedByMood, bool hasRecord)> OnGoingMentalStates = [];
    public static Hediff CurrentTickingHediff;
    public static TraitDegreeData CurrentTickingTraitData;
    public static HashSet<string> IgnoredMentalBreaks = [
        "PanicFleeFire", // Happens too frequently
        "SocialFighting", // Handled by SocialFightStartedEvent
    ];

    public static MentalBreakReason CreateReason(bool causedByMood, string inGameReason)
    {
        var cause = GetCause(causedByMood);
        var hediff = CurrentTickingHediff;
        var trait = CurrentTickingTraitData;
        return new MentalBreakReason(cause, inGameReason, hediff, trait?.label);
    }

    public static MentalBreakCause GetCause(bool causedByMood)
    {
        if (CurrentTickingHediff != null)
            return MentalBreakCause.Hediff;
        if (CurrentTickingTraitData != null)
            return MentalBreakCause.Trait;
        if (causedByMood)
            return MentalBreakCause.Mood;
        return MentalBreakCause.Other;
    }

    public static Pawn TryFindTarget(MentalState mentalState)
    {
        if (mentalState == null)
            return null;

        if (mentalState is MentalState_MurderousRage mr)
            return mr.target;
        else if (mentalState is MentalState_InsultingSpree isp)
            return isp.target;
        else if (mentalState is MentalState_CorpseObsession co)
            return co.corpse.InnerPawn;
        return null;
    }
}

[HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
public static class MentalStateHandler_TryStartMentalState_Patch_2
{
    public static void Postfix(bool __result, MentalStateHandler __instance, string reason, bool causedByMood, bool transitionSilently)
    {
        if (!__result)
            return;

        if (transitionSilently) // transition from another mental state, not a new mental break, ignore
            return;

        var pawn = Accessor.MentalStateHandler.Pawn(__instance);
        var mentalState = __instance.CurStateDef;

        if (MentalBreakContext.IgnoredMentalBreaks.Contains(mentalState.defName))
            return;

        if (pawn.MentalState is MentalState_Slaughterer || pawn.MentalState is MentalState_Jailbreaker)
        {
            MentalBreakContext.OnGoingMentalStates[pawn] = (mentalState, reason, causedByMood, hasRecord: false);
            return;
        }

        var target = MentalBreakContext.TryFindTarget(pawn.MentalState);
        var mentalBreakReason = MentalBreakContext.CreateReason(causedByMood, reason);
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, mentalBreakReason, null, mentalState, target));
    }
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class Pawn_JobTracker_StartJob_Patch_2
{
    public static void Postfix(Pawn_JobTracker __instance, Job newJob)
    {
        var pawn = Accessor.Pawn_JobTracker.Pawn(__instance);
        var mentalState = pawn.MentalState;

        if (mentalState == null || MentalBreakContext.OnGoingMentalStates.Count == 0)
            return;
        if (!MentalBreakContext.OnGoingMentalStates.TryGetValue(pawn, out var ongoingState) || ongoingState.hasRecord)
            return;

        if (mentalState is MentalState_Slaughterer && newJob.def == JobDefOf.Slaughter || mentalState is MentalState_Jailbreaker && newJob.def == JobDefOf.InducePrisonerToEscape)
        {
            MentalBreakContext.OnGoingMentalStates[pawn] = ongoingState with { hasRecord = true };

            var target = newJob.targetA.Pawn;
            var mentalBreakReason = MentalBreakContext.CreateReason(ongoingState.causedByMood, ongoingState.reason);
            GameEventBus.Publish(new MentalBreakStartedEvent(pawn, mentalBreakReason, null, ongoingState.mentalState, target));
        }
    }
}

[HarmonyPatch(typeof(MentalState), nameof(MentalState.RecoverFromState))]
static class RecoverFromState_RecoverFromState_Patch
{
    static void Postfix(MentalState __instance) => MentalBreakContext.OnGoingMentalStates.Remove(__instance.pawn);
}

// -- Mental breaks without Mental State --

[HarmonyPatch(typeof(MentalBreakWorker_RunWild), nameof(MentalBreakWorker_RunWild.TryStart))]
public static class MentalBreakWorker_RunWild_TryStart_Patch
{
    public static void Prefix(MentalBreakWorker_RunWild __instance, Pawn pawn, string reason, bool causedByMood)
    {
        var mentalBreakReason = MentalBreakContext.CreateReason(causedByMood, reason);
        // fire in prefix before they run wild and change their faction to null
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, mentalBreakReason, __instance.def, MentalState: null, Target: null));
    }
}

[HarmonyPatch(typeof(MentalBreakWorker_Catatonic), nameof(MentalBreakWorker_Catatonic.TryStart))]
public static class MentalBreakWorker_Catatonic_TryStart_Patch
{
    public static void Postfix(MentalBreakWorker_Catatonic __instance, Pawn pawn, string reason, bool causedByMood)
    {
        var mentalBreakReason = MentalBreakContext.CreateReason(causedByMood, reason);
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, mentalBreakReason, __instance.def, MentalState: null, Target: null));
    }
}

// Call order:
// - Hediff.TickInterval() prefix
//  - pawn.mindState.mentalStateHandler.TryStartMentalState()
//  - Hediff.TryDoRandomMentalBreak()
// - Hediff.TickInterval() postfix

[HarmonyPatch(typeof(Hediff), nameof(Hediff.TickInterval))]
internal static class Hediff_TickInterval_Patch
{
    static void Prefix(Hediff __instance) => MentalBreakContext.CurrentTickingHediff = __instance;
    static void Finalizer() => MentalBreakContext.CurrentTickingHediff = null;
}

// Call order:
// - TraitMentalStateGiver.CheckGive() prefix
//  - pawn.mindState.mentalStateHandler.TryStartMentalState()
// - TraitMentalStateGiver.CheckGive() postfix

[HarmonyPatch(typeof(TraitMentalStateGiver), nameof(TraitMentalStateGiver.CheckGive))]
internal static class TraitMentalStateGiver_CheckGive_Patch
{
    static void Prefix(TraitMentalStateGiver __instance) => MentalBreakContext.CurrentTickingTraitData = __instance.traitDegreeData;
    static void Finalizer() => MentalBreakContext.CurrentTickingTraitData = null;
}
