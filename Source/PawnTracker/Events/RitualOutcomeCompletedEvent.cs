using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PawnHistory.Source.PawnTracker.Recorders;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RitualOutcomeCompletedEvent(Pawn Host, PreceptDef RitualDef, string RitualLabel, string OutcomeLabel, List<Pawn> Participants, List<Pawn> Spectators, Dictionary<string, List<Pawn>> AssignedRoles) : GameEventBase;

file record RitualOutcomeContextFrame(LordJob_Ritual RitualJob, List<Pawn> Participants, RitualOutcomePossibility Outcome);

file static class RitualOutcomeContext
{
    public static int CallDepth; // TODO: Necessary (?)
    public static RitualOutcomeContextFrame Frame; // RitualOutcomeEffectWorker_RemoveConsumableBuilding

    public static void Begin(LordJob_Ritual jobRitual, Dictionary<Pawn, int> totalPresence)
    {
        CallDepth++;

        if (CallDepth > 1)
            return;

        Frame = new RitualOutcomeContextFrame(jobRitual, totalPresence.Keys.ToList(), null);
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
        var assignedRoles = Accessor.RitualRoleAssignments.AssignedRoles(ritualJob.assignments).ToDictionary(e => e.Key, e => e.Value);
        var spectators = Accessor.RitualRoleAssignments.Spectators(ritualJob.assignments).ToList();

        GameEventBus.Publish(new RitualOutcomeCompletedEvent(host, ritualJob.Ritual.def, ritualJob.Ritual.Label, outcome, frame.Participants, spectators, assignedRoles));
    }

    private static Pawn GetOrganizer(LordJob_Ritual ritualJob)
    {
        if (ritualJob.Ritual.def == Extra.PreceptDefOf.Conversion)
            return ritualJob.PawnWithRole(RitualRoleId.Moralist);
        if (ritualJob.Ritual.def == Extra.PreceptDefOf.Execution)
            return ritualJob.PawnWithRole(RitualRoleId.Executioner);
        return Accessor.RitualRoleAssignments.AssignedRoles(ritualJob.assignments).Values.First().First();
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
        // yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_ChildBirth), methodName); ----> handled by GaveBirthRecorder as it's triggered outside ritual context as well.
    }

    private static void Prefix(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual) => RitualOutcomeContext.Begin(jobRitual, totalPresence);
    private static void Finalizer() => RitualOutcomeContext.End();
}

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_FromQuality), nameof(RitualOutcomeEffectWorker_FromQuality.GetOutcome))]
internal static class RitualOutcomeEffectWorker_FromQuality_GetOutcome_Patch
{
    private static void Postfix(LordJob_Ritual ritual, RitualOutcomePossibility __result)
    {
        if (RitualOutcomeContext.Frame.RitualJob != ritual)
            return;

        RitualOutcomeContext.Frame = RitualOutcomeContext.Frame with { Outcome = __result };
    }
}
