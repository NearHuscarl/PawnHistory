using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RitualOutcomeCompletedEvent(Pawn Host, string RitualLabel, string OutcomeLabel, List<Pawn> Participants, Dictionary<string, List<Pawn>> AssignedRoles) : GameEventBase;

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

        var host = GetOrganizer(frame);
        if (host == null)
            return;

        // Ritual_Outcomes.xml
        var outcome = frame.Outcome.label;
        var assignedRoles = Accessor.RitualRoleAssignments.AssignedRoles(frame.RitualJob.assignments).ToDictionary(e => e.Key, e => e.Value); 

        GameEventBus.Publish(new RitualOutcomeCompletedEvent(host, frame.RitualJob.Ritual.Label, outcome, frame.Participants, assignedRoles));
    }

    private static Pawn GetOrganizer(RitualOutcomeContextFrame frame)
    {
        if (frame.RitualJob.Ritual.def == Extra.PreceptDefOf.Conversion)
            return frame.RitualJob.PawnWithRole("moralist");
        if (frame.RitualJob.Ritual.def == Extra.PreceptDefOf.Execution)
            return frame.RitualJob.PawnWithRole("executioner");
        return frame.RitualJob.Organizer;
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
        // yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_ChildBirth), methodName); ----> handled by GaveBirthRecorder as it's triggered outside of ritual as well.
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
