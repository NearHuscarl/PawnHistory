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
}

public record MentalBreakStartedEvent(Pawn Pawn, string Reason, MentalBreakCause Cause, MentalBreakDef MentalBreak, MentalStateDef MentalState = null, Pawn Target = null, Hediff Hediff = null) : GameEventBase;

internal class MentalBreakContext
{
    public static readonly Dictionary<Pawn, (MentalStateDef mentalState, string reason, bool causedByMood, bool hasRecord)> OnGoingMentalStates = [];
    public static Hediff CurrentTickingHediff;
    public static HashSet<string> IgnoredMentalBreaks = [
        "PanicFleeFire", // Happens too frequently
        "SocialFighting", // Handled by SocialFightStartedEvent
     ];

    public static MentalBreakCause GetCause(bool causedByMood)
    {
        if (CurrentTickingHediff != null)
            return MentalBreakCause.Hediff;
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
    public static void Postfix(bool __result, MentalStateHandler __instance, string reason, bool causedByMood)
    {
        if (!__result)
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

        var cause = MentalBreakContext.GetCause(causedByMood);
        var hediff = MentalBreakContext.CurrentTickingHediff;
        var target = MentalBreakContext.TryFindTarget(pawn.MentalState);
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, reason, cause, null, mentalState, target, hediff));
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

            var cause = MentalBreakContext.GetCause(ongoingState.causedByMood);
            var hediff = MentalBreakContext.CurrentTickingHediff;
            var target = newJob.targetA.Pawn;
            GameEventBus.Publish(new MentalBreakStartedEvent(pawn, ongoingState.reason, cause, null, ongoingState.mentalState, target, hediff));
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
        var cause = MentalBreakContext.GetCause(causedByMood);
        var hediff = MentalBreakContext.CurrentTickingHediff;
        // fire in prefix before they run wild and change their faction to null
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, reason, cause, __instance.def, MentalState: null, Target: null, hediff));
    }
}

[HarmonyPatch(typeof(MentalBreakWorker_Catatonic), nameof(MentalBreakWorker_Catatonic.TryStart))]
public static class MentalBreakWorker_Catatonic_TryStart_Patch
{
    public static void Postfix(MentalBreakWorker_Catatonic __instance, Pawn pawn, string reason, bool causedByMood)
    {
        var cause = MentalBreakContext.GetCause(causedByMood);
        var hediff = MentalBreakContext.CurrentTickingHediff;

        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, reason, cause, __instance.def, MentalState: null, Target: null, hediff));
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
