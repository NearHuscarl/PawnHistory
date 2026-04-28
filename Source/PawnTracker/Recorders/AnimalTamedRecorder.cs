using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class AnimalTamedRecorder : RecorderBase<AnimalTamedEvent>
{
    private const float WildnessThreshold = 0.75f;

    public override void Register()
    {
        GameEventBus.Subscribe<AnimalTamedEvent>(CreateRecord);
    }

    public override void CreateRecord(AnimalTamedEvent e)
    {
        if (!ShouldRecord(e.Tamer))
            return;

        var wasWildMan = e.TamedPawn.RaceProps.Humanlike;
        if (!wasWildMan && e.TamedPawn.GetStatValue(StatDefOf.Wildness) < WildnessThreshold)
            return;
        
        // remove Sentence_RecruitAttemptAccepted
        var tameAttemptText = e.LogEntryText.Split('.').Select(p => p.Trim()).FirstOrDefault(p => !p.NullOrEmpty());
        var recordDef = HistoryRecordDefOf.AnimalTamed;
        var desc = recordDef.Description(e.Tamer)
            .WithPlayerFaction()
            .AddRule("WildAnimal", e.TamedPawn, addSubsymbols: true)
            .AddRule("InteractionLog", tameAttemptText)
            .Resolve();

        AddRecord(recordDef, e.Tamer, desc, [e.TamedPawn]);
        if (wasWildMan && ShouldRecord(e.TamedPawn))
            AddRecord(recordDef, e.TamedPawn, desc, [e.Tamer]);
    }

    private static (Pawn, Pawn) SetupTest(TestScenario scenario, PawnKindDef pawnKindDef)
    {
        scenario.SpeedUp();
        scenario.Map()
            .BuildRoom(8, 8, "Freezer")
            .WithThing(ThingDefOf.Meat_Human, 500)
            .Execute();
        
        var target = scenario.Pawn().WithKind(pawnKindDef).CreateSingle();
        target.Map.designationManager.AddDesignation(new Designation(target, DesignationDefOf.Tame));
        var tamer = scenario.Pawn().Colonist().FullHeal().ResetSkillLevel(SkillDefOf.Animals, 20).StartJob(JobDefOf.Tame, target).CreateSingle();
        tamer.mindState.inspirationHandler.TryStartInspiration(InspirationDefOf.Inspired_Taming);

        return (tamer, target);
    }

    public Action Test(TestScenario scenario)
    {
        Expect.Assertions(1);
        var (tamer, target) = SetupTest(scenario, Extra.PawnKindDefOf.Bear_Grizzly);
        
        GameEventBus.SubscribeOnce<AnimalTamedEvent>(e =>
        {
            Expect.That(tamer).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.AnimalTamed,
                Description = "[WildAnimal] was tamed and joined [PlayerFaction].",
                Concerns = [target],
            });
        });

        return () => scenario.SlowDown();
    }
    
    public Action TestWildMan(TestScenario scenario)
    {
        Expect.Assertions(2);
        var (tamer, target) = SetupTest(scenario, PawnKindDefOf.WildMan);
        
        GameEventBus.SubscribeOnce<AnimalTamedEvent>(e =>
        {
            Expect.That(target).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.AnimalTamed,
                Description = "[WildAnimal] was tamed and joined [PlayerFaction].",
                Concerns = [tamer],
            });
            Expect.That(tamer).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.AnimalTamed,
                Description = "[WildAnimal] was tamed and joined [PlayerFaction].",
                Concerns = [target],
            });
        });

        return () => scenario.SlowDown();
    }
}
