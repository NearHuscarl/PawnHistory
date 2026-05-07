using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RitualOutcomeCompletedEvent(Pawn Host, string RitualLabel, string OutcomeLabel, List<Pawn> Participants) : GameEventBase;

file record RitualOutcomeContextFrame(LordJob_Ritual RitualJob, List<Pawn> Participants, RitualOutcomePossibility Outcome);

file static class RitualOutcomeContext
{
    public static int CallDepth;
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
        
        var host = GetOrganizer();
        if (host == null)
            return;

        // Ritual_Outcomes.xml
        var outcome = Frame.Outcome.label;

        GameEventBus.Publish(new RitualOutcomeCompletedEvent(host, Frame.RitualJob.Ritual.Label, outcome, Frame.Participants));
        Frame = null;
        CallDepth = 0;
    }

    private static Pawn GetOrganizer()
    {
        if (Frame.RitualJob.Ritual.def == Extra.PreceptDefOf.Conversion)
            return Frame.RitualJob.PawnWithRole("moralist");
        return Frame.RitualJob.Organizer;
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
