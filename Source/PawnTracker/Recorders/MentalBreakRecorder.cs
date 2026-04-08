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

public class MentalBreakRecorder : RecorderBase<MentalBreakRecorder.Input>
{
    public record Input(Pawn pawn, MentalBreakDef mentalBreak, string reason, Pawn target = null);

    public override void Register()
    {
        // TODO: move this to Events/
        GameEventBus.Subscribe<MentalBreakStartEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn)) return;
            if (e.MentalBreakWorker is not MentalBreakWorker_RunWild) return;

            CreateRecord(new Input(e.Pawn, e.MentalBreakWorker.def, e.Reason));
        });
        GameEventBus.Subscribe<MentalBreakStartedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn)) return;
            if (e.MentalBreakWorker is MentalBreakWorker_RunWild) return;

            if (e.Pawn.MentalState is MentalState_Slaughterer || e.Pawn.MentalState is MentalState_Jailbreaker)
                OnGoingMentalStates[e.Pawn] = (e.MentalBreakWorker.def, e.Reason, false);
            else
                CreateRecord(new Input(e.Pawn, e.MentalBreakWorker.def, e.Reason));
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
                CreateRecord(new Input(e.Pawn, ongoingState.mentalBreak, ongoingState.reason, e.NewJob.targetA.Pawn));
            }
        });
        GameEventBus.Subscribe<MentalStateEndedEvent>(e => OnGoingMentalStates.Remove(e.Pawn));
    }

    private static readonly Dictionary<Pawn, (MentalBreakDef mentalBreak, string reason, bool hasRecord)> OnGoingMentalStates = [];

    public override void CreateRecord(Input input)
    {
        var (pawn, mentalBreak, reason, target) = input;
        target ??= TryFindTarget(pawn.MentalState);

        var mentalState = pawn.MentalState; // mentalState could be null in some MentalBreak
        var recordDef = mentalState?.def.category == MentalStateCategory.Aggro ? HistoryRecordDefOf.MentalBreakViolent : HistoryRecordDefOf.MentalBreak;
        var hasCustomDescription = HasCustomDescription(mentalBreak, recordDef);
        var concerns = new List<Thing>() { mentalState?.causedByPawn, target };
        var descBuilder = recordDef.Description(pawn)
            .WithFaction(pawn.Faction)
            .IncludePawnGrammar()
            .AddRule("Reason", ParseReason(reason))
            .AddRule("Target", target, addSubsymbols: true)
            .AddConstant("name", mentalBreak.defName);

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
                Log.Warning($"Cannot resolve description of {mentalBreak}: inGameDesc is null");
                return;
            }

            descBuilder.AddRule("InGameDesc", inGameDesc);
        }

        AddRecord(recordDef, pawn, descBuilder.Resolve(), concerns);
    }

    private static bool HasCustomDescription(MentalBreakDef mentalBreak, HistoryRecordDef recordDef)
    {
        return recordDef.descriptionMaker.RulesPlusIncludes.Any(r => r.keyword == "entry"
            && r.constantConstraints != null
            && r.Priority == 1
            && r.constantConstraints.Any(c => c.key == "name" && c.value == mentalBreak.defName)
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

    [SkipTest]
    public void TestNaturalBreak(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();

        scenario.SpeedUp();

        // MentalBreaker.CurrentDesiredMoodBreakIntensity -> only allows mental break after 2000 ticks
        var tickStart = Find.TickManager.TicksGame;
        scenario.RunUntil(() => Find.TickManager.TicksGame - tickStart > 2500, () =>
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
            scenario.SlowDown();
        }, 200);
    }

    private static readonly List<MentalBreakDef> InvididuallyTestedMentalBreaks = [
        DefDatabase<MentalBreakDef>.GetNamed("Slaughterer"),
        DefDatabase<MentalBreakDef>.GetNamed("Jailbreaker"),
        DefDatabase<MentalBreakDef>.GetNamed("SadisticRage"),
        MentalBreakDefOf.CorpseObsession,
    ];

    [SkipTest]
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
    }

    public void TestAnimalSlaughterer(TestScenario scenario)
    {
        scenario.Pawn()
            .Animal()
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();

        var pawn = scenario.Pawn()
            .Colonist()
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(DefDatabase<MentalBreakDef>.GetNamed("Slaughterer")))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] had a mental breakdown and was going to vent [PAWN_possessive] anger by slaughtering [Target]. [Reason]");
    }

    public void TestSadisticRage(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Prison")
            .AsPrison(prisonerCount: 2)
            .Execute();

        var pawn = scenario.Pawn()
            .Colonist()
            .WithPosition(scenario.OutsideOf("Prison"))
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(DefDatabase<MentalBreakDef>.GetNamed("SadisticRage")))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] flew into a sadistic rage. [PAWN_pronoun] was going to vent [PAWN_possessive] anger on the prisoners. [Reason]");
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
            .WithPosition(scenario.OutsideOf("Prison"))
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(jailbreakerBreak))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] had a mental breakdown and was going to induce [Prisoners] to escape. [Reason]");
    }

    public void TestCorpseObsession(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, "Grave")
            .WithCasket(ThingDefOf.Sarcophagus, ThingDefOf.Plasteel)
            .Execute();

        var pawn = scenario.Pawn()
            .Colonist()
            .Do((p, i) => p.StartMentalBreakWithMadeupThought(MentalBreakDefOf.CorpseObsession))
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] became obsessed with corpses. [PAWN_pronoun] was going to find and present [Target]'s corpse for all to see. [Reason]");
    }
}
