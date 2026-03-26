using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class MentalBreakRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<MentalBreakStartEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn)) return;
            if (e.MentalBreakWorker is not MentalBreakWorker_RunWild) return;

            HandleMentalBreaksEvent(e.Pawn, e.MentalBreakWorker.def, e.Reason);
        });
        GameEventBus.Subscribe<MentalBreakStartedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn)) return;
            if (e.MentalBreakWorker is MentalBreakWorker_RunWild) return;

            if (e.Pawn.MentalState is MentalState_Slaughterer || e.Pawn.MentalState is MentalState_Jailbreaker)
                OnGoingMentalStates[e.Pawn] = (e.MentalBreakWorker.def, e.Reason, false);
            else
                HandleMentalBreaksEvent(e.Pawn, e.MentalBreakWorker.def, e.Reason);
        });
        GameEventBus.Subscribe<JobStartedEvent>(e =>
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
        GameEventBus.Subscribe<MentalStateEndedEvent>(e => OnGoingMentalStates.Remove(e.Pawn));
    }

    private static readonly Dictionary<Pawn, (MentalBreakDef mentalBreak, string reason, bool hasRecord)> OnGoingMentalStates = [];

    private void HandleMentalBreaksEvent(Pawn pawn, MentalBreakDef mentalBreak, string reason, Pawn target = null)
    {
        target ??= TryFindTarget(pawn.MentalState);

        var mentalState = pawn.MentalState; // mentalState could be null in some MentalBreak
        var recordDef = mentalState?.def.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;
        var hasCustomDescription = HasCustomDescription(mentalBreak, recordDef);
        var rootKeyword = hasCustomDescription ? "mentalBreak" : "mentalBreakDefault";
        var concerns = new List<Thing>() { mentalState?.causedByPawn, target };
        var descBuilder = recordDef.Description(rootKeyword, pawn)
            .WithFaction(pawn.Faction)
            .IncludePawnGrammar()
            .AddRule("REASON", ParseReason(reason))
            .AddRule("TARGET", target, addSubsymbols: true);

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
                descBuilder.AddRule("PRISONERS", LangUtility.FormatList(allPrisonersInRoom));
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

        AddRecord(recordDef, pawn, descBuilder.Resolve(), concerns);
    }

    private static bool HasCustomDescription(MentalBreakDef mentalBreak, HistoryRecordDef recordDef)
    {
        return recordDef.descriptionMaker.RulesPlusIncludes.Any(rule =>
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
            return "";

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

    public void TestNaturalBreak(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();

        Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;

        var interval = TickDelayManager.Interval(200, () =>
        {
            pawn.needs.mood.CurLevel = 0;
            pawn.needs.food.CurLevel = 0;
            pawn.needs.joy.CurLevel = 0;
            pawn.needs.beauty.CurLevel = 0;
            pawn.needs.comfort.CurLevel = 0;
            pawn.needs.rest.CurLevel = 0.05f;
        });

        // MentalBreaker.CurrentDesiredMoodBreakIntensity -> only allows mental break after 2000 ticks
        TickDelayManager.Delay(2500, () =>
        {
            pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced); // wake the fuck up
            pawn.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
            TickDelayManager.Cancel(interval);
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
        });
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Prison")
            .AsPrison(prisonerCount: 2) // Jailbreaker
            .Execute();

        scenario.Thing()
            .BuildRoom(MapBuilder.Beside("Prison", Rot4.North, 5, 5), "Grave")
            .WithGrave() // CorpseObsession
            .Execute();

        scenario.Thing()
            .BuildRoom(MapBuilder.Beside("Prison", Rot4.East, 7, 7), "Bedroom")
            .AsBarrack(bedCount: 2) // BedroomTantrum
            .WithThing(ThingDefOf.GoJuice) // Binging_DrugExtreme
            .WithThing(ThingDefOf.Beer) // Binging_DrugMajor
            .Execute();

        scenario.Thing()
            .BuildRoom(MapBuilder.Beside("Prison", Rot4.South, 18, 7), "Freezer")
            .WithThing(ThingDefOf.MealFine, 300) // Binging_Food
            .Execute();

        scenario.Thing()
            .BuildRoom(MapBuilder.Beside("Prison", Rot4.West, 5, 5), "Common")
            .Execute();

        scenario.Pawn()
            .Animal() // Slaughterer
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();

        var mentalBreaks = DefDatabase<MentalBreakDef>.AllDefs.ToList();

        var pawns = scenario.Pawn(mentalBreaks.Count)
            .ThatMatches(ShouldRecord)
            .WithPosition(TestScenario.TaggedRooms["Common"].CenterCell, 4)
            .Do(p => p.story?.traits?.allTraits.Clear())
            .Do(p => p.story?.traits?.GainTrait(new Trait(TraitDefOf.Pyromaniac))) // FireStartingSpree
            .Do((p, i) =>
            {
                if (mentalBreaks[i].defName == "BedroomTantrum" || mentalBreaks[i].defName.Contains("Wander_OwnRoom"))
                {
                    var bed = p.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>()
                        .FirstOrDefault(b => TestScenario.TaggedRooms["Bedroom"].Contains(b.Position) && b.AnyUnownedSleepingSlot);
                    if (bed != null)
                        p.ownership.ClaimBedIfNonMedical(bed);
                }
            })
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(mentalBreaks[i]))
            .Execute();
    }
}
