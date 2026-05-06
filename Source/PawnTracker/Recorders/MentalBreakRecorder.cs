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
        // mentalState could be null in some MentalBreak
        var (pawn, reason, mentalBreak, mentalState, target, quest) = input;

        if (!ShouldRecord(pawn))
            return;

        var defName = mentalBreak?.defName ?? mentalState.def.defName;

        if (reason.Cause == MentalBreakCause.Other)
        {
            Log.Warning($"[PawnHistory] {defName} MentalBreak is not supported. {DebugUtility.Format(input)}");
            return;
        }

        var recordDef = mentalState?.def.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;
        var concerns = new List<Thing> { mentalState?.causedByPawn, target };
        var builder = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Target", target, addSubsymbols: true)
            .AddRule("InGameDesc", mentalState?.GetBeginLetterText().Resolve().Replace("\r", " ").Replace("\n", " ").Trim('.'))
            .AddRule("ReasonHediff", reason.Hediff?.LabelNounInBracket())
            .AddRule("ReasonTrait", reason.Trait?.Colorize(NeedsCardUtility.MoodColorNegative))
            .AddRule("ReasonMood", ParsePoorMoodReason(reason.InGameReason)?.Colorize(NeedsCardUtility.MoodColorNegative))
            .AddConstant("cause", reason.Cause)
            .AddConstant("name", defName);
        var buildInput = new MentalBreakComp.BuildInput(pawn, reason, mentalBreak, mentalState, target, quest);

        foreach (var comp in Comps.OfType<MentalBreakComp>())
        {
            if (!comp.Match(buildInput))
                continue;

            builder = comp.BuildGrammarRequest(builder, buildInput);
            concerns.AddRange(comp.GetConcerns(buildInput));
        }

        AddRecord(recordDef, pawn, builder.Resolve(), concerns, quest: quest);
    }

    // This happened because of poor mood. The final straw was: {0} -> {0}
    private static string ParsePoorMoodReason(string fullReason)
    {
        if (fullReason.NullOrEmpty())
            return string.Empty;

        var thisHappenedBecauseOfPoorMood = "MentalStateReason_Mood".Translate().ToString();
        var theFinalStrawWas = "FinalStraw".Translate("{0}").ToString();
        var template = $"{thisHappenedBecauseOfPoorMood}\n\n{theFinalStrawWas}";
        var parts = template.Split(["{0}"], StringSplitOptions.None);

        if (parts.Length != 2)
            return string.Empty;

        var prefix = parts[0];
        var suffix = parts[1];

        if (fullReason.StartsWith(prefix) && fullReason.EndsWith(suffix))
        {
            return fullReason.Substring(prefix.Length, fullReason.Length - prefix.Length - suffix.Length);
        }
        
        return string.Empty;
    }
    
    [SkipTest]
    public Action TestNaturalBreak(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();

        scenario.SpeedUp();

        var tickStart = Find.TickManager.TicksGame;
        scenario.Loop(d =>
        {
            pawn.needs.mood.CurLevel = 0;
            pawn.needs.food.CurLevel = 0;
            pawn.needs.joy.CurLevel = 0;
            pawn.needs.beauty.CurLevel = 0;
            pawn.needs.comfort.CurLevel = 0;
            pawn.needs.rest.CurLevel = 0.05f;

            // MentalBreaker.CurrentDesiredMoodBreakIntensity -> only allows mental break after 2000 ticks
            if (Find.TickManager.TicksGame - tickStart <= 2100)
                return;
            d.Cancelled = true;
            
            if (!pawn.Awake())
                RestUtility.WakeUp(pawn);
            pawn.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
        },
        50);
        
        var recordDef = pawn.MentalState?.def.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;
        
        Expect.That(pawn).Eventually().ToHaveHistoryRecord(recordDef, MoodReasonTemplate);

        return () => scenario.SlowDown();
    }

    internal const string MoodReasonTemplate = "This happened because of poor mood: [Reason].";
    
    public void Test(TestScenario scenario)
    {
        var mentalBreakTemplateLookup = new Dictionary<string, string>
        {
            { "Berserk", $"[PAWN] went berserk. [He] was going to attack anyone [He] sees. {MoodReasonTemplate}" },
            { "FireStartingSpree", $"[PAWN] was on a fire starting spree. [He] wandered around for a while, randomly starting fires. {MoodReasonTemplate}" },
            { "Catatonic", $"[PAWN] suffered a total mental breakdown and entered a catatonic state for several days. {MoodReasonTemplate}" },
            { "GiveUpExit", $"[PAWN] gave up on this community. [He] decided to leave and pursue a better life elsewhere. {MoodReasonTemplate}" },
            { "Binging_Food", $"[PAWN] pigged out on food. {MoodReasonTemplate}" },
            { "Wander_Psychotic", $"[PAWN] wandered around in a psychotic state. {MoodReasonTemplate}" },
            { "Wander_Sad", $"[PAWN] broke down and wandered around in sadness. {MoodReasonTemplate}" },
            { "Wander_OwnRoom", $"[PAWN] hid in [His] room. {MoodReasonTemplate}" },
            { "Tantrum", $"[PAWN] had a tantrum. [He] was going to smash up random furniture, items and structures. {MoodReasonTemplate}" },
            { "BedroomTantrum", $"[PAWN] had a tantrum. [He] was going to smash things in [His] room. {MoodReasonTemplate}" },
            { "InsultingSpree", $"[PAWN] was on an insulting spree. [He] was going to wander around, randomly insulting others. {MoodReasonTemplate}" },
            { "TargetedInsultingSpree", $"[PAWN] fixated [His] rage on [Target]. [He] was going to follow [Target_objective] around, hurling insults. {MoodReasonTemplate}" },
            { "MurderousRage", $"[PAWN] flew into a murderous rage and decided to kill [Target]. {MoodReasonTemplate}" },
        };
        
        scenario.Map()
            .BuildRoom(7, 7, "Bedroom")
            .AsBarrack(bedCount: 2) // BedroomTantrum
            .Execute();

        scenario.Map()
            .BuildRoom(MapBuilder.Beside("Bedroom", Rot4.South, 18, 7), "Freezer")
            .WithThing(ThingDefOf.MealFine, 300) // Binging_Food
            .Execute();

        var mentalBreaks = DefDatabase<MentalBreakDef>.AllDefs
            .Where(d => mentalBreakTemplateLookup.ContainsKey(d.defName))
            .ToList();

        var pawns = scenario.Pawn(mentalBreaks.Count)
            .ThatMatches(ShouldRecord)
            .StopMentalState()
            .Position(scenario.TaggedRooms["Freezer"].CenterCell, 4)
            .GiveTrait(TraitDefOf.Pyromaniac) // FireStartingSpree
            .Do((p, i) =>
            {
                if (mentalBreaks[i].defName == "BedroomTantrum" || mentalBreaks[i].defName == "Wander_OwnRoom")
                {
                    p.ownership.ClaimBedIfNonMedical(RestUtility.FindBedFor(p));
                }
            })
            .Do((p, i) => p.StartMentalBreakWithMadeUpThought(mentalBreaks[i]))
            .Execute();

        for (var i = 0; i < mentalBreaks.Count; i++)
        {
            var recordDef = mentalBreaks[i].mentalState?.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;

            if (mentalBreakTemplateLookup.TryGetValue(mentalBreaks[i].defName, out var template))
                Expect.That(pawns[i]).ToHaveHistoryRecord(recordDef, template);
        }
    }

    public void TestAnimalSlaughterer(TestScenario scenario)
    {
        var animal = scenario.Pawn()
            .Animal()
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();

        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Do((p, i) => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.Slaughterer))
            .CreateSingle();

        Expect.That(pawn).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.MentalBreakViolent,
            Description = $"[PAWN] had a mental breakdown and was going to vent [PAWN_possessive] anger by slaughtering [Target]. {MoodReasonTemplate}",
            Concerns = [animal],
        });
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
            .Position(scenario.OutsideOf("Prison"))
            .Do(p => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.SadisticRage))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.MentalBreakViolent, $"[PAWN] flew into a sadistic rage. [PAWN_pronoun] was going to vent [PAWN_possessive] anger on the prisoners. {MoodReasonTemplate}");
    }

    public void TestCorpseObsession(TestScenario scenario)
    {
        var deadPawn = scenario.Pawn().Colonist().CreateSingle();
        scenario.Map()
            .BuildRoom(6, 6, "Grave")
            .WithCasket(ThingDefOf.Sarcophagus, ThingDefOf.Plasteel, true, deadPawn)
            .Execute();

        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Do(p => p.StartMentalBreakWithMadeUpThought(MentalBreakDefOf.CorpseObsession))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.MentalBreak,
            Description = $"[PAWN] became obsessed with corpses. [He] was going to find and present [Target]'s corpse for all to see. {MoodReasonTemplate}",
            Concerns = [deadPawn],
        });
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
        {
            hediff.TickInterval(int.MaxValue);
            if (pawn.MentalState != null) break;
        }

        var recordDef = pawn.MentalState?.def.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;

        Expect.That(pawn).ToHaveHistoryRecord(recordDef, "[MentalBreak]. This happened because of: Resurrection psychosis (total).");
    }

    public void TestHediff2(TestScenario scenario)
    {
        // <mentalStateGivers>
        var hediff = (Hediff)null;
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Heal()
            .AddHediff(Extra.HediffDefOf.LuciferiumAddiction, hediffCreated: h => hediff = h)
            .CreateSingle();

        hediff.Severity = .01f;
        pawn.needs.TryGetNeed<Need_Chemical>().CurLevel = 0f;

        for (var i = 0; i < 5000; i++)
        {
            hediff.TickInterval(int.MaxValue);
            if (pawn.MentalState != null) break;
        }

        var recordDef = pawn.MentalState?.def.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;
        
        Expect.That(pawn).ToHaveHistoryRecord(recordDef, $"[MentalBreak]. This happened because of: Luciferium need (unmet).");
    }

    public void TestTrait(TestScenario scenario)
    {
        // <randomMentalState>
        scenario.Map()
            .BuildRoom(5, 5, "Freezer")
            .WithThing(ThingDefOf.MealFine, 100) // Binging_Food
            .Execute();

        var trait = (Trait)null;
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Heal()
            .GiveTrait(Extra.TraitDefOf.Gourmand, traitCreated: t => trait = t)
            .CreateSingle();

        pawn.needs.mood.CurLevel = 0;
        pawn.needs.food.CurLevel = 0;

        for (var i = 0; i < 500; i++)
        {
            if (trait.CurrentData.MentalStateGiver.CheckGive(pawn, int.MaxValue))
                break;
        }
        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.MentalBreak, $"[PAWN] pigged out on food. This happened because of the trait: Gourmand.");
    }
}
