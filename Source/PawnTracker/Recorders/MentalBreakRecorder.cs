using PawnHistory.Source.DebugTools;
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

public class MentalBreakRecorder : RecorderBase<MentalBreakStartedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<MentalBreakStartedEvent>(CreateRecord);
    }

    public override void CreateRecord(MentalBreakStartedEvent input)
    {
        var (pawn, reason, cause, mentalBreak, mentalStateDef, target, hediff) = input;
        var mentalState = pawn.MentalState; // mentalState could be null in some MentalBreak

        if (!ShouldRecord(pawn))
            return;

        var defName = mentalBreak?.defName ?? mentalStateDef.defName;

        if (cause == MentalBreakCause.Other)
        {
            Log.Warning($"[PawnHistory] {defName} MentalBreak is not supported. {DebugUtility.Format(input)}");
            return;
        }

        var recordDef = mentalState?.def.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;
        var hasCustomDescription = HasCustomDescription(defName, recordDef);
        var concerns = new List<Thing>() { mentalState?.causedByPawn, target };
        var descBuilder = recordDef.Description(pawn)
            .WithFaction(pawn.Faction)
            .IncludePawnGrammar()
            .AddRule("Reason", GetReason(reason, cause, recordDef, hediff, pawn))
            .AddRule("Target", target, addSubsymbols: true)
            .AddConstant("name", defName);

        // Reasons to override mental state's description:
        // - Too long to fit in history record (RunWild, GiveUpExit)
        // - Change to past tense as this is a history mod.
        // - Some mental break messages are in strange places rather than from MentalBreakDef
        if (hasCustomDescription)
        {
            if (mentalState is MentalState_BingingDrug bd)
                descBuilder.AddRule("Drug", bd.chemical.label);
            else if (mentalState is MentalState_TargetedTantrum tt)
            {
                descBuilder.AddRule("Thing", tt.target.Label.Colorize(ColoredText.SubtleGrayColor));
                concerns.Add(tt.target);
            }
            else if (mentalState is MentalState_Jailbreaker)
            {
                var room = target.GetRoom();
                var allPrisonersInRoom = room.ContainedThings<Pawn>().Where(p => p.IsPrisoner).ToList();
                concerns.AddRange(allPrisonersInRoom);
                descBuilder.AddRule("Prisoners", LangUtility.FormatList(allPrisonersInRoom));
            }
        }
        else
        {
            // modded mental states or something I am missing in vanilla
            var inGameDesc = mentalState?.GetBeginLetterText().Resolve().Replace("\r", " ").Replace("\n", " ");

            if (inGameDesc.NullOrEmpty())
            {
                Log.Warning($"Cannot resolve description of {defName}: inGameDesc is null");
                return;
            }

            descBuilder.AddRule("InGameDesc", inGameDesc);
        }

        AddRecord(recordDef, pawn, descBuilder.Resolve(), concerns);
    }

    private static bool HasCustomDescription(string defName, HistoryRecordDef recordDef)
    {
        return recordDef.descriptionMaker.RulesPlusIncludes.Any(r => r.keyword == "entry"
            && r.constantConstraints != null
            && r.Priority == 1
            && r.constantConstraints.Any(c => c.key == "name" && c.value == defName)
        );
    }

    private static string GetReason(string inputReason, MentalBreakCause cause, HistoryRecordDef recordDef, Hediff hediff, Pawn pawn)
    {
        if (cause == MentalBreakCause.Mood)
            return ParsePoorMoodReason(inputReason);

        if (cause == MentalBreakCause.Hediff)
            return recordDef.Description(pawn)
                .AddRule("Hediff", hediff.LabelNounInBracket())
                .Resolve("ReasonHediff");

        return inputReason;
    }

    // This happened because of poor mood. The final straw was: {0} -> This happened because of poor mood: {0}
    private static string ParsePoorMoodReason(string fullReason)
    {
        var thisHappenedBecauseOfPoorMood = "MentalStateReason_Mood".Translate().ToString();

        if (fullReason.NullOrEmpty())
            return string.Empty;

        var theFinalStrawWas = "FinalStraw".Translate("{0}").ToString();
        var template = $"{thisHappenedBecauseOfPoorMood}\n\n{theFinalStrawWas}";
        var parts = template.Split(["{0}"], StringSplitOptions.None);
        if (parts.Length != 2)
            return string.Empty;

        var prefix = parts[0];
        var suffix = parts[1];

        if (fullReason.StartsWith(prefix) && fullReason.EndsWith(suffix))
        {
            var reason = fullReason.Substring(prefix.Length, fullReason.Length - prefix.Length - suffix.Length);
            return $"{thisHappenedBecauseOfPoorMood.Trim('.')}: {reason.Colorize(NeedsCardUtility.MoodColorNegative)}";
        }

        return fullReason;
    }

    [SkipTest]
    public Action TestNaturalBreak(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();

        scenario.SpeedUp();

        // MentalBreaker.CurrentDesiredMoodBreakIntensity -> only allows mental break after 2000 ticks
        var tickStart = Find.TickManager.TicksGame;
        scenario.RunUntil(() => Find.TickManager.TicksGame - tickStart > 2100, () =>
        {
            pawn.needs.mood.CurLevel = 0;
            pawn.needs.food.CurLevel = 0;
            pawn.needs.joy.CurLevel = 0;
            pawn.needs.beauty.CurLevel = 0;
            pawn.needs.comfort.CurLevel = 0;
            pawn.needs.rest.CurLevel = 0.05f;
        },
        onFinish: () =>
        {
            pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced); // wake the fuck up
            pawn.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
        }, 50);
        
        Expect.That(pawn).Eventually().ToHaveHistoryRecord(Reason);

        return () => scenario.SlowDown();
    }

    private static readonly List<MentalBreakDef> InvididuallyTestedMentalBreaks = [
        DefDatabase<MentalBreakDef>.GetNamed("Slaughterer"),
        DefDatabase<MentalBreakDef>.GetNamed("Jailbreaker"),
        DefDatabase<MentalBreakDef>.GetNamed("SadisticRage"),
        MentalBreakDefOf.CorpseObsession,
    ];

    private static readonly string Reason = "This happened because of poor mood: [Reason]";

    public void Test(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(7, 7, "Bedroom")
            .AsBarrack(bedCount: 2) // BedroomTantrum
            .WithThing(ThingDefOf.GoJuice) // Binging_DrugExtreme
            .WithThing(ThingDefOf.Beer) // Binging_DrugMajor
            .Execute();

        scenario.Map()
            .BuildRoom(MapBuilder.Beside("Bedroom", Rot4.South, 18, 7), "Freezer")
            .WithThing(ThingDefOf.MealFine, 300) // Binging_Food
            .Execute();

        scenario.Map()
            .BuildRoom(MapBuilder.Beside("Bedroom", Rot4.West, 5, 5), "Common")
            .Execute();

        var mentalBreaks = DefDatabase<MentalBreakDef>.AllDefs.Except(InvididuallyTestedMentalBreaks).ToList();

        var pawns = scenario.Pawn(mentalBreaks.Count)
            .ThatMatches(ShouldRecord)
            .StopMentalState()
            .WithPosition(TestScenario.TaggedRooms["Common"].CenterCell, 4)
            .Do(p => p.story?.traits?.GainTrait(new Trait(TraitDefOf.Pyromaniac))) // FireStartingSpree
            .Do((p, i) =>
            {
                if (mentalBreaks[i].defName == "BedroomTantrum" || mentalBreaks[i].defName.Contains("Wander_OwnRoom"))
                {
                    p.ownership.ClaimBedIfNonMedical(RestUtility.FindBedFor(p));
                }
            })
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(mentalBreaks[i]))
            .Execute();

        var mentalBreakTemplateLookup = new Dictionary<string, string>()
        {
            { "Berserk", $"[PAWN] went berserk. [PAWN_pronoun] was going to attack anyone [PAWN_pronoun] sees. {Reason}" },
            { "FireStartingSpree", $"[PAWN] was on a fire starting spree. [PAWN_pronoun] wandered around for a while, randomly starting fires. {Reason}" },
            { "Catatonic", $"[PAWN] suffered a total mental breakdown and entered a catatonic state for several days. {Reason}" },
            { "RunWild", $"[PAWN] was fed up with civilization. [PAWN_pronoun] decided to leave [FACTION] to live with the animals in the wild. {Reason}" },
            { "GiveUpExit", $"[PAWN] gave up on this community. [PAWN_pronoun] decided to leave and pursue a better life elsewhere. {Reason}" },
            { "Binging_DrugExtreme", $"[PAWN] binged on [Drug] during an extreme mental break. {Reason}" },
            { "Binging_DrugMajor", $"[PAWN] binged on [Drug] during a major mental break. {Reason}" },
            { "Binging_Food", $"[PAWN] pigged out on food. {Reason}" },
            { "Wander_Psychotic", $"[PAWN] wandered around in a psychotic state. {Reason}" },
            { "Wander_Sad", $"[PAWN] broke down and wandered around in sadness. {Reason}" },
            { "Wander_OwnRoom", $"[PAWN] hid in [PAWN_possessive] room. {Reason}" },
            { "Tantrum", $"[PAWN] had a tantrum. [PAWN_pronoun] was going to smash up random furniture, items and structures. {Reason}" },
            { "TargetedTantrum", $"[PAWN] had a tantrum. [PAWN_pronoun] was going to destroy [Thing]. {Reason}" },
            { "BedroomTantrum", $"[PAWN] had a tantrum. [PAWN_pronoun] was going to smash things in [PAWN_possessive] room. {Reason}" },
            { "InsultingSpree", $"[PAWN] was on an insulting spree. [PAWN_pronoun] was going to wander around, randomly insulting others. {Reason}" },
            { "TargetedInsultingSpree", $"[PAWN] fixated [PAWN_possessive] rage on [Target]. [PAWN_pronoun] was going to follow [Target_objective] around, hurling insults. {Reason}" },
            { "MurderousRage", $"[PAWN] flew into a murderous rage and decided to kill [Target]. {Reason}" },
        };

        for (var i = 0; i < mentalBreaks.Count; i++)
        {
            var recordDef = mentalBreaks[i].mentalState?.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;

            if (mentalBreakTemplateLookup.TryGetValue(mentalBreaks[i].defName, out var template))
                Expect.That(pawns[i]).ToHaveHistoryRecord(template, recordDef);
        }
    }

    public void TestAnimalSlaughterer(TestScenario scenario)
    {
        scenario.Pawn()
            .Animal()
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();

        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(DefDatabase<MentalBreakDef>.GetNamed("Slaughterer")))
            .CreateSingle();

        Expect.That(pawn).Eventually().ToHaveHistoryRecord($"[PAWN] had a mental breakdown and was going to vent [PAWN_possessive] anger by slaughtering [Target]. {Reason}");
    }

    public void TestSadisticRage(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Prison")
            .AsPrison(prisonerCount: 2)
            .Execute();

        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .WithPosition(scenario.OutsideOf("Prison"))
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(DefDatabase<MentalBreakDef>.GetNamed("SadisticRage")))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord($"[PAWN] flew into a sadistic rage. [PAWN_pronoun] was going to vent [PAWN_possessive] anger on the prisoners. {Reason}");
    }

    public void TestPrisonBreak(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Prison")
            .AsPrison(prisonerCount: 2) // Jailbreaker
            .Execute();

        var jailbreakerBreak = DefDatabase<MentalBreakDef>.GetNamed("Jailbreaker");
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .WithPosition(scenario.OutsideOf("Prison"))
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(jailbreakerBreak))
            .CreateSingle();

        Expect.That(pawn).Eventually().ToHaveHistoryRecord($"[PAWN] had a mental breakdown and was going to induce [Prisoners] to escape. {Reason}");
    }

    public void TestCorpseObsession(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, "Grave")
            .WithCasket(ThingDefOf.Sarcophagus, ThingDefOf.Plasteel)
            .Execute();

        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(MentalBreakDefOf.CorpseObsession))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord($"[PAWN] became obsessed with corpses. [PAWN_pronoun] was going to find and present [Target]'s corpse for all to see. {Reason}");
    }

    public void TestHediff(TestScenario scenario)
    {
        // <mentalBreakMtbDays>
        var hediff = (Hediff)null;
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Heal()
            .AddHediff(HediffDefOf.ResurrectionPsychosis, hediffCreated: h => hediff = h)
            .CreateSingle();

        hediff.Severity = 0.8f;

        for (var i = 0; i < 5000; i++)
            hediff.TickInterval(int.MaxValue);

        Expect.That(pawn).ToHaveHistoryRecord($"This happened because of: [Hediff]");
    }

    public void TestHediff2(TestScenario scenario)
    {
        // <mentalStateGivers>
        var hediff = (Hediff)null;
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Heal()
            .AddHediff("LuciferiumAddiction", hediffCreated: h => hediff = h)
            .CreateSingle();

        hediff.Severity = .01f;
        pawn.needs.TryGetNeed<Need_Chemical>().CurLevel = 0f;

        for (var i = 0; i < 500; i++)
            hediff.TickInterval(int.MaxValue);

        Expect.That(pawn).ToHaveHistoryRecord($"This happened because of: [Hediff]");
    }
}
