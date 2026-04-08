using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using UnityEngine.Profiling;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class FoodPoisoningRecorder : RecorderBase<FoodPoisoningEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<FoodPoisoningEvent>(CreateRecord);
    }

    public override void CreateRecord(FoodPoisoningEvent e)
    {
        var (victim, ingestible, cause, cook) = e;

        if (!ShouldRecord(victim))
            return;

        var recordDef = HistoryRecordDefOf.FoodPoisoning;
        var desc = recordDef.Description(e.Victim)
            .AddRule("Ingestible", ingestible.LabelShort, addSubsymbols: true)
            .AddRule("Cook", cook)
            .AddRule("Cause", cause.ToStringHuman())
            .AddConstant("cause", cause)
            .AddConstant("hasCook", cook != null)
            .Resolve();

        AddRecord(recordDef, e.Victim, desc, [e.Cook]);
    }

    public void Test(TestScenario scenario)
    {
        var victim = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .FullHeal()
            .CreateSingle();
        
        var ingestible = scenario.Thing(ThingDefOf.MealSimple).Create();
        foreach (FoodPoisonCause cause in Enum.GetValues(typeof(FoodPoisonCause)))
        {
            FoodUtility.AddFoodPoisoningHediff(victim, ingestible, cause);
        }

        Expect.That(victim).ToHaveHistoryRecord("[PAWN] got food poisoning from a simple meal.", -5, exactMatch: true);
        Expect.That(victim).ToHaveHistoryRecord("[PAWN] got food poisoning from a simple meal because of incompetent cook.", -4);
        Expect.That(victim).ToHaveHistoryRecord("[PAWN] got food poisoning from a simple meal because of dirty cooking area.", -3);
        Expect.That(victim).ToHaveHistoryRecord("[PAWN] got food poisoning from a simple meal because of rotten food.", -2);
        Expect.That(victim).ToHaveHistoryRecord("[PAWN] got food poisoning from a simple meal because of dangerous food type.", -1);

        scenario.OpenHistoryRecordTab(victim);
    }

    public void TestIncompetentCook(TestScenario scenario)
    {
        var pawns = scenario.Pawn(2)
            .ThatMatches(ShouldRecord)
            .FullHeal()
            .Execute();
        var oldPoisonChance = Find.Storyteller.difficulty.foodPoisonChanceFactor;
        Find.Storyteller.difficulty.foodPoisonChanceFactor = 1f;

        var victim = pawns[0];
        var cook = pawns[1];
        var ingestible = scenario.Thing(ThingDefOf.MealSimple)
            .PoisonFood(cook)
            .Create();

        scenario.Pawn(victim)
            .Do(p => p.needs.food.CurLevel = 0f)
            .StartJob(JobDefOf.Ingest, ingestible)
            .Execute();

        scenario.SpeedUp();
        Expect.That(victim).Eventually().ToHaveHistoryRecord("[PAWN] got food poisoning from a simple meal because of incompetent cook [Cook].");

        GameEventBus.SubscribeOnce<FoodPoisoningEvent>(e =>
        {
            Find.Storyteller.difficulty.foodPoisonChanceFactor = oldPoisonChance;
            scenario.SlowDown();
            scenario.OpenHistoryRecordTab(victim);
        });
    }
}
