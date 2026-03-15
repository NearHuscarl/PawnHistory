using LudeonTK;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class MentalBreakRecorder : RecorderBase
{
    private static bool debugShowReason = false;

    [NearDebugAction]
    public static void ForceDisplayMentalBreakReason()
    {
        debugShowReason = !debugShowReason;
        Log.Message($"[MentalBreakRecorder] Force display reason is now: {debugShowReason}");
    }

    public override void Register()
    {
        GameEventListener.Subscribe<MentalBreakStartEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn)) return;
            if (e.MentalBreakWorker is not MentalBreakWorker_RunWild) return;

            HandleMentalBreaksEvent(e.Pawn, e.MentalBreakWorker.def, e.Reason);
        });
        GameEventListener.Subscribe<MentalBreakStartedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn)) return;
            if (e.MentalBreakWorker is MentalBreakWorker_RunWild) return;

            if (e.Pawn.MentalState is MentalState_Slaughterer || e.Pawn.MentalState is MentalState_Jailbreaker)
                OnGoingMentalStates[e.Pawn] = (e.MentalBreakWorker.def, e.Reason, false);
            else
                HandleMentalBreaksEvent(e.Pawn, e.MentalBreakWorker.def, e.Reason);
        });
        GameEventListener.Subscribe<JobStartedEvent>(e =>
        {
            var mentalState = e.Pawn.MentalState;

            if (mentalState == null || OnGoingMentalStates.Count == 0)
                return;
            if (!OnGoingMentalStates.TryGetValue(e.Pawn, out var ongoingState) || ongoingState.hasRecord)
                return;

            if (mentalState is MentalState_Slaughterer && e.NewJob.def == JobDefOf.Slaughter
            || mentalState is MentalState_Jailbreaker && e.NewJob.def == JobDefOf.InducePrisonerToEscape)
            {
                OnGoingMentalStates[e.Pawn] = ongoingState with { hasRecord = true };
                HandleMentalBreaksEvent(e.Pawn, ongoingState.mentalBreak, ongoingState.reason, e.NewJob.targetA.Pawn);
            }
        });
        GameEventListener.Subscribe<MentalStateEndedEvent>(e => OnGoingMentalStates.Remove(e.Pawn));
    }

    private static readonly Dictionary<Pawn, (MentalBreakDef mentalBreak, string reason, bool hasRecord)> OnGoingMentalStates = [];

    private void HandleMentalBreaksEvent(Pawn pawn, MentalBreakDef mentalBreak, string reason, Pawn target = null)
    {
        target ??= TryFindTarget(pawn.MentalState);

        var mentalState = pawn.MentalState; // mentalState could be null in some MentalBreak
        var eventDef = mentalState?.def.category == MentalStateCategory.Aggro ? PawnEventDefOf.MentalBreakViolent : PawnEventDefOf.MentalBreak;
        var hasCustomDescription = HasCustomDescription(mentalBreak, eventDef);
        var rootKeyword = hasCustomDescription ? "mentalBreak" : "mentalBreakDefault";
        var concerns = new List<Thing>() { mentalState?.causedByPawn, target };
        var descBuilder = eventDef.ResolveDescription(rootKeyword, pawn)
            .WithFaction(pawn.Faction)
            .IncludePawnGrammar()
            .AddRule("REASON", ParseReason(reason))
            .RulesForPawn("TARGET", target)
            .AddRule("TARGET", target);

        if (hasCustomDescription)
        {
            // Reasons to override mental state's description:
            // - Too long to fit in history record (RunWild, GiveUpExit)
            // - Change to past tense as this is a history mod.
            // - Some mental break messages are in strange places rather than from MentalBreakDef
            descBuilder.AddConstant("name", mentalBreak.defName);

            if (mentalState is MentalState_BingingDrug bd)
                descBuilder.AddRule("DRUG", bd.chemical.label);
            else if (mentalState is MentalState_TargetedTantrum tt)
            {
                descBuilder.AddRule("THING", tt.target.Label.Colorize(ColoredText.SubtleGrayColor));
                concerns.Add(tt.target);
            }
            else if (mentalState is MentalState_Jailbreaker)
            {
                var room = target.GetRoom();
                var allPrisonersInRoom = room.ContainedThings<Pawn>().Where(p => p.IsPrisoner).ToList();
                concerns.AddRange(allPrisonersInRoom);
                descBuilder.AddRule("PRISONERS", LangUtility.FormatPawnList(allPrisonersInRoom));
            }
        }
        else
        {
            // modded mental states or something I am missing in vanilla
            var inGameDesc = mentalState?.GetBeginLetterText().Resolve().Replace("\r", " ").Replace("\n", " ");

            if (inGameDesc.NullOrEmpty())
            {
                Log.Warning($"Cannot resolve description of {mentalBreak}: inGameDesc is null");
                return;
            }

            descBuilder.AddRule("INGAMEDESC", inGameDesc);
        }

        AddRecord(new HistoryRecord(eventDef, pawn, descBuilder.Resolve(), concerns));
    }

    private static bool HasCustomDescription(MentalBreakDef mentalBreak, PawnEventDef eventDef)
    {
        return eventDef.rulePackDef.RulesPlusIncludes.Any(rule =>
            rule.keyword == "mentalBreak" &&
            rule.constantConstraints != null &&
            rule.constantConstraints.Any(c => c.key == "name" && c.value == mentalBreak.defName)
            );
    }

    private static Pawn TryFindTarget(MentalState mentalState)
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

    // This happened because of poor mood. The final straw was: {0} -> This happened because of poor mood: {0}
    private static string ParseReason(string fullReason)
    {
        var thisHappenedBecauseOfPoorMood = "MentalStateReason_Mood".Translate().ToString();

        if (fullReason.NullOrEmpty())
            return debugShowReason ? $"{thisHappenedBecauseOfPoorMood.Trim('.')}: {GetMockedReason().Colorize(NeedsCardUtility.MoodColorNegative)}" : "";

        var theFinalStrawWas = "FinalStraw".Translate("{0}").ToString();
        var template = $"{thisHappenedBecauseOfPoorMood}\n\n{theFinalStrawWas}";
        var parts = template.Split(["{0}"], StringSplitOptions.None);
        if (parts.Length != 2)
            return fullReason;

        var prefix = parts[0];
        var suffix = parts[1];

        if (fullReason.StartsWith(prefix) && fullReason.EndsWith(suffix))
        {
            var reason = fullReason.Substring(prefix.Length, fullReason.Length - prefix.Length - suffix.Length);
            return $"{thisHappenedBecauseOfPoorMood.Trim('.')}: {reason.Colorize(NeedsCardUtility.MoodColorNegative)}";
        }

        return fullReason;
    }

    private static string GetMockedReason()
    {
        var randomNegativeThought = DefDatabase<ThoughtDef>.AllDefs
            .Where(t => t.stages != null && t.stages.Any(s => s != null && s.baseMoodEffect < 0))
            .RandomElementWithFallback();

        var fakeReason = randomNegativeThought != null
            ? randomNegativeThought.stages[0].label
            : "[Unknown Debug Reason]";

        Log.Message($"[PawnHistory] Mental Break had no reason. Injected: {fakeReason}");
        return fakeReason;
    }
}
