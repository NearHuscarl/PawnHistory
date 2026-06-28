using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PawnHistory.Source.PawnTracker.Recorders;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RitualOutcomeCompletedEvent(
    Pawn Host,
    PreceptDef RitualDef,
    RitualOutcomeEffectDef OutcomeEffectDef,
    string RitualLabel,
    string OutcomeLabel,
    List<Pawn> Spectators,
    List<Pawn> Participants,
    Dictionary<string, List<Pawn>> AssignedRoles,
    List<Thing> Targets) : GameEventBase;

file record RitualOutcomeContextFrame(LordJob_Ritual RitualJob, RitualOutcomePossibility Outcome);

file static class RitualOutcomeContext
{
    public static int CallDepth; // TODO: Necessary (?)
    public static RitualOutcomeContextFrame Frame; // RitualOutcomeEffectWorker_RemoveConsumableBuilding

    public static void Begin(LordJob_Ritual jobRitual)
    {
        CallDepth++;

        if (CallDepth > 1)
            return;

        Frame = new RitualOutcomeContextFrame(jobRitual, null);
    }

    public static void End()
    {
        CallDepth--;

        if (CallDepth > 0)
            return;

        var frame = Frame;
        Frame = null;
        CallDepth = 0;

        var ritualJob = frame.RitualJob;
        var host = GetOrganizer(ritualJob);
        var outcome = frame.Outcome?.label; // Ritual_Outcomes.xml
        var forcedRoles = (ritualJob.assignments.ForcedRolesForReading ?? []).ToDictionary(e => e.Key, e => new List<Pawn> { e.Value });
        var assignedRoles = Accessor.RitualRoleAssignments.AssignedRoles(ritualJob.assignments).Concat(forcedRoles).ToDictionary(e => e.Key, e => e.Value);
        var spectators = Accessor.RitualRoleAssignments.Spectators(ritualJob.assignments).ToList();
        var participants = Accessor.RitualRoleAssignments.Participants(ritualJob.assignments).ToList();
        var outcomeDef = ritualJob.Ritual.outcomeEffect.def;
        List<Thing> targets = [ritualJob.selectedTarget.Thing, ritualJob.obligation?.targetA.Thing, ritualJob.obligation?.targetB.Thing, ritualJob.obligation?.targetC.Thing];

        targets = targets.Where(t => t != null).Distinct().ToList();

        GameEventBus.Publish(new RitualOutcomeCompletedEvent(host, ritualJob.Ritual.def, outcomeDef, ritualJob.Ritual.Label, outcome, spectators, participants, assignedRoles, targets));
    }

    private static Pawn GetOrganizer(LordJob_Ritual ritualJob)
    {
        if (ritualJob.Ritual.def == Extra.PreceptDefOf.Conversion)
            return ritualJob.PawnWithRole(RitualRoleId.Moralist);
        if (ritualJob.Ritual.def == Extra.PreceptDefOf.Execution)
            return ritualJob.PawnWithRole(RitualRoleId.Executioner);
        if (ModsConfig.IdeologyActive && ritualJob.Ritual.outcomeEffect.def == Extra.RitualOutcomeEffectDefOf.Sacrifice)
            return ritualJob.PawnWithRole(RitualRoleId.Moralist);
        if (ritualJob.Ritual.def == Extra.PreceptDefOf.Trial
            || ritualJob.Ritual.def == Extra.PreceptDefOf.TrialPrisoner
            || ritualJob.Ritual.def == Extra.PreceptDefOf.TrialMentalState)
            return ritualJob.PawnWithRole(RitualRoleId.Leader);
        if (ritualJob.Ritual.def == Extra.PreceptDefOf.GladiatorDuel)
            return ritualJob.PawnWithRole(RitualRoleId.Leader);
        if (ritualJob.Ritual.def == Extra.PreceptDefOf.BlindingCeremony)
            return ritualJob.PawnWithRole(RitualRoleId.Doer);
        if (ritualJob.Ritual.def == Extra.PreceptDefOf.ScarificationCeremony)
            return ritualJob.PawnWithRole(RitualRoleId.Doer);
        if (ritualJob.Ritual.def == PreceptDefOf.ChildBirth)
            return ritualJob.PawnWithRole(RitualRoleId.Doctor);
        return Accessor.RitualRoleAssignments.AssignedRoles(ritualJob.assignments).Values.FirstOrDefault()?.First();
    }
}

// Call order:
// - RitualOutcomeEffectWorker_FromQuality.Apply() prefix
//  - RitualOutcomeEffectWorker_FromQuality.GetOutcome()
// - RitualOutcomeEffectWorker_FromQuality.Apply() postfix

[HarmonyPatch]
internal static class RitualOutcomeEffectWorker_Apply_Patch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        const string methodName = nameof(RitualOutcomeEffectWorker_FromQuality.Apply);

        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_FromQuality), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_Speech), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_Conversion), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_Execution), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_AnimaTreeLinking), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_ConnectToTree), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_ChildBirth), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_Trial), methodName);
    }

    private static void Prefix(LordJob_Ritual jobRitual) => RitualOutcomeContext.Begin(jobRitual);
    private static void Finalizer() => RitualOutcomeContext.End();
}

[HarmonyPatch]
internal static class RitualOutcomeEffectWorker_GetOutcome_Patch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        const string methodName = nameof(RitualOutcomeEffectWorker_FromQuality.GetOutcome);

        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_FromQuality), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_Trial), methodName);
    }

    private static void Postfix(LordJob_Ritual ritual, RitualOutcomePossibility __result)
    {
        if (RitualOutcomeContext.Frame.RitualJob != ritual)
            return;

        RitualOutcomeContext.Frame = RitualOutcomeContext.Frame with { Outcome = __result };
    }
}
