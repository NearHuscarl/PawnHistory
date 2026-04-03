using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class FoodPoisoningRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<FoodPoisoningEvent>(e =>
        {
            HandleFoodPoisoningEvent(e);
        });
    }

    private void HandleFoodPoisoningEvent(FoodPoisoningEvent e)
    {
        if (!ShouldRecord(e.Victim))
            return;

        var recordDef = HistoryRecordDefOf.FoodPoisoning;
        var desc = recordDef.Description(e.Victim)
            .AddRule("Ingestible", e.Ingestible.LabelShort, addSubsymbols: true)
            .AddRule("Cook", e.Cook)
            .AddRule("Cause", e.Cause.ToStringHuman())
            .AddConstant("cause", e.Cause)
            .AddConstant("hasCook", e.Cook != null)
            .Resolve();

        AddRecord(recordDef, e.Victim, desc, [e.Cook]);
    }

    public override void Test(TestScenario scenario)
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
        scenario.OpenHistoryRecordTab(victim);
    }

    public void TestIncompetentCook(TestScenario scenario)
    {
        var pawns = scenario.Pawn(2)
            .ThatMatches(ShouldRecord)
            .FullHeal()
            .Execute();

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
        GameEventBus.SubscribeOnce<FoodPoisoningEvent>(e =>
        {
            scenario.SlowDown();
            scenario.OpenHistoryRecordTab(victim);
        });
    }
}
