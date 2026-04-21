using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RitualOutcomeCompletedEvent(Pawn Host, string RitualLabel, string OutcomeLabel, List<Pawn> Participants) : GameEventBase;

internal static class RitualOutcomeContext
{
    public static int CallDepth;
    public static LordJob_Ritual ActiveJob;
    public static RitualOutcomePossibility Outcome;
    public static readonly HashSet<Pawn> Participants = [];
    public static readonly HashSet<Pawn> SpecialPawns = [];

    public static void Begin(LordJob_Ritual jobRitual, Dictionary<Pawn, int> totalPresence)
    {
        CallDepth++;

        if (CallDepth > 1)
            return;

        ActiveJob = jobRitual;

        foreach (var pawn in totalPresence.Keys)
            Participants.Add(pawn);

        if (jobRitual.selectedTarget.Thing is Corpse { InnerPawn: not null } corpse)
            SpecialPawns.Add(corpse.InnerPawn);
    }

    public static void End(LordJob_Ritual jobRitual)
    {
        if (CallDepth > 1)
            return;
        
        Publish(jobRitual);
    }

    public static void Finalizer()
    {
        CallDepth--;

        if (CallDepth <= 0)
            Reset();
    }

    private static void Publish(LordJob_Ritual jobRitual)
    {
        var host = jobRitual?.Organizer;
        if (host == null)
            return;

        // Ritual_Outcomes.xml
        var outcome = Outcome.label;

        GameEventBus.Publish(new RitualOutcomeCompletedEvent(host, jobRitual.Ritual.Label, outcome, Participants.ToList()));
    }

    private static void Reset()
    {
        CallDepth = 0;
        ActiveJob = null;
        Outcome = null;
        Participants.Clear();
        SpecialPawns.Clear();
    }
}

[HarmonyPatch]
internal static class RitualOutcomeEffectWorker_Apply_Patch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_FromQuality), nameof(RitualOutcomeEffectWorker_FromQuality.Apply));
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_Speech), nameof(RitualOutcomeEffectWorker_Speech.Apply));
    }

    private static void Prefix(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual) => RitualOutcomeContext.Begin(jobRitual, totalPresence);
    private static void Postfix(LordJob_Ritual jobRitual) => RitualOutcomeContext.End(jobRitual);
    private static void Finalizer() => RitualOutcomeContext.Finalizer();
}

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_FromQuality), nameof(RitualOutcomeEffectWorker_FromQuality.GetOutcome))]
internal static class RitualOutcomeEffectWorker_FromQuality_GetOutcome_Patch
{
    private static void Postfix(LordJob_Ritual ritual, RitualOutcomePossibility __result)
    {
        if (RitualOutcomeContext.ActiveJob != ritual)
            return;

        RitualOutcomeContext.Outcome = __result;
    }
}
