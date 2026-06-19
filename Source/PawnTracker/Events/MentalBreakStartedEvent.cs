using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public enum MentalBreakCause
{
    Other,
    Mood,
    Hediff,
    Trait,
    Arrested,
    Royalty,
}

public record MentalBreakReason(MentalBreakCause Cause, string InGameReason, Hediff Hediff = null, string Trait = null, Pawn Arrester = null);

public record MentalBreakStartedEvent(Pawn Pawn, MentalBreakReason Reason, MentalBreakDef MentalBreak, MentalState MentalState = null, Pawn Target = null, Quest Quest = null) : GameEventBase;

internal static class MentalBreakContext
{
    public static readonly Dictionary<Pawn, (MentalState mentalState, string reason, bool causedByMood, bool hasRecord)> OnGoingMentalStates = [];
    public static Hediff CurrentTickingHediff;
    public static TraitDegreeData CurrentTickingTraitData;
    public static Pawn CurrentArrester;
    public static readonly HashSet<string> IgnoredMentalBreaks = [
        "PanicFlee", // Handled in PanicFleeRecorder. MentalBreakRecorder does not support event of a group of pawns
        "PanicFleeFire", // Happens too frequently
        "SocialFighting", // Handled by SocialFightStartedEvent
        "IdeoChange", // Handled separately because MentalStateHandler.CurState is set to SadWander immediately in the nested call (MentalState_IdeoChange.PostStart)
    ];

    public static MentalBreakReason CreateReason(bool causedByMood, string inGameReason, bool issueDecree = false)
    {
        var cause = GetCause(causedByMood, issueDecree);
        var hediff = CurrentTickingHediff;
        var trait = CurrentTickingTraitData;
        return new MentalBreakReason(cause, inGameReason, hediff, trait?.label, CurrentArrester);
    }

    private static MentalBreakCause GetCause(bool causedByMood, bool issueDecree = false)
    {
        if (CurrentTickingHediff != null)
            return MentalBreakCause.Hediff;
        if (CurrentTickingTraitData != null)
            return MentalBreakCause.Trait;
        if (CurrentArrester != null)
            return MentalBreakCause.Arrested;
        if (causedByMood)
            return MentalBreakCause.Mood;
        if (issueDecree)
            return MentalBreakCause.Royalty;
        return MentalBreakCause.Other;
    }

    public static Pawn TryFindTarget(MentalState mentalState)
    {
        if (mentalState == null)
            return null;

        if (mentalState is MentalState_MurderousRage mr)
            return mr.target;
        if (mentalState is MentalState_InsultingSpree isp)
            return isp.target;
        if (mentalState is MentalState_CorpseObsession co)
            return co.corpse.InnerPawn;
        return null;
    }
}

[HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
internal static class MentalStateHandler_TryStartMentalState_Patch_2
{
    public static void Postfix(bool __result, MentalStateDef stateDef, MentalStateHandler __instance, string reason, bool causedByMood, bool transitionSilently)
    {
        if (!__result)
            return;

        if (transitionSilently) // transition from another mental state, not a new mental break, ignore
            return;

        var pawn = Accessor.MentalStateHandler.Pawn(__instance);
        var mentalState = __instance.CurState;

        if (MentalBreakContext.IgnoredMentalBreaks.Contains(stateDef.defName))
            return;

        if (pawn.MentalState is MentalState_Slaughterer or MentalState_Jailbreaker)
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
internal static class Pawn_JobTracker_StartJob_Patch_2
{
    private static void Postfix(Pawn_JobTracker __instance, Job newJob)
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
internal static class RecoverFromState_RecoverFromState_Patch
{
    private static void Postfix(MentalState __instance) => MentalBreakContext.OnGoingMentalStates.Remove(__instance.pawn);
}

// -- Mental breaks without Mental State --

[HarmonyPatch(typeof(MentalBreakWorker_RunWild), nameof(MentalBreakWorker_RunWild.TryStart))]
internal static class MentalBreakWorker_RunWild_TryStart_Patch
{
    public static void Prefix(MentalBreakWorker_RunWild __instance, Pawn pawn, string reason, bool causedByMood)
    {
        var mentalBreakReason = MentalBreakContext.CreateReason(causedByMood, reason);
        // fire in prefix before they run wild and change their faction to null
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, mentalBreakReason, __instance.def));
    }
}

[HarmonyPatch(typeof(MentalBreakWorker_Catatonic), nameof(MentalBreakWorker_Catatonic.TryStart))]
internal static class MentalBreakWorker_Catatonic_TryStart_Patch
{
    public static void Postfix(MentalBreakWorker_Catatonic __instance, Pawn pawn, string reason, bool causedByMood)
    {
        var mentalBreakReason = MentalBreakContext.CreateReason(causedByMood, reason);
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, mentalBreakReason, __instance.def));
    }
}

[HarmonyPatch(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.IssueDecree))]
internal static class Pawn_RoyaltyTracker_IssueDecree_Patch
{
    public static void Prefix(ref int __state, Pawn_RoyaltyTracker __instance)
    {
        __state = __instance.lastDecreeTicks;
    }
    public static void Postfix(int __state, Pawn_RoyaltyTracker __instance, bool causedByMentalBreak, string mentalBreakReason)
    {
        if (__state == __instance.lastDecreeTicks)
            return;
        var pawn = __instance.pawn;
        var reason = MentalBreakContext.CreateReason(causedByMentalBreak, mentalBreakReason, true);
        var quest = Find.QuestManager.QuestsListForReading.LastOrDefault(q => q.root.decreeTags.Any());
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, reason, Extra.MentalBreakDefOf.WildDecree, Quest: quest));
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
    private static void Prefix(Hediff __instance) => MentalBreakContext.CurrentTickingHediff = __instance;
    private static void Postfix() => MentalBreakContext.CurrentTickingHediff = null;
}

// Call order:
// - TraitMentalStateGiver.CheckGive() prefix
//  - pawn.mindState.mentalStateHandler.TryStartMentalState()
// - TraitMentalStateGiver.CheckGive() postfix

[HarmonyPatch(typeof(TraitMentalStateGiver), nameof(TraitMentalStateGiver.CheckGive))]
internal static class TraitMentalStateGiver_CheckGive_Patch
{
    private static void Prefix(TraitMentalStateGiver __instance) => MentalBreakContext.CurrentTickingTraitData = __instance.traitDegreeData;
    private static void Postfix() => MentalBreakContext.CurrentTickingTraitData = null;
}

// Call order:
// - pawn.CheckAcceptArrest() prefix
//  - pawn.mindState.mentalStateHandler.TryStartMentalState()
// - pawn.CheckAcceptArrest() postfix

[HarmonyPatch(typeof(Pawn), nameof(Pawn.CheckAcceptArrest))]
internal static class Pawn_CheckAcceptArrest_Patch
{
    private static void Prefix(Pawn arrester) => MentalBreakContext.CurrentArrester = arrester;
    private static void Postfix() => MentalBreakContext.CurrentArrester = null;
}

// Edge case: IdeoChange mental break

[HarmonyPatch(typeof(MentalState_IdeoChange), nameof(MentalState_IdeoChange.PostStart))]
internal static class MentalState_IdeoChange_PostStart_Patch
{
    private static void Postfix(MentalState_IdeoChange __instance, string reason)
    {
        var mentalBreakReason = MentalBreakContext.CreateReason(__instance.causedByMood, reason);
        GameEventBus.Publish(new MentalBreakStartedEvent(__instance.pawn, mentalBreakReason, Extra.MentalBreakDefOf.IdeoChange, __instance));
    }
}
