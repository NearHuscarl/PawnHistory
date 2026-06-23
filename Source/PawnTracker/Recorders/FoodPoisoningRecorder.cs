using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
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
        var ingestibleCorpse = ingestible as Corpse;
        var desc = recordDef.Description(e.Victim)
            .AddRule("Ingestible", ingestible.LabelShort, addSubsymbols: true)
            .AddRule("Ingestible", ingestibleCorpse?.InnerPawn, addSubsymbols: true, replaceIfExist: true)
            .AddRule("Cook", cook)
            .AddRule("Cause", cause.ToStringHuman())
            .AddConstant("isCorpse", ingestibleCorpse != null)
            .AddConstant("humanLikeCorpse", ingestibleCorpse?.InnerPawn.RaceProps.Humanlike)
            .AddConstant("cause", cause)
            .AddConstant("hasCook", cook != null)
            .Resolve();

        AddRecord(recordDef, e.Victim, desc, [e.Cook, ingestibleCorpse?.InnerPawn]);
    }

    public void Test(TestScenario scenario)
    {
        var victim = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .FullHeal()
            .CreateSingle();
        
        var ingestible = scenario.Thing(ThingDefOf.MealSimple).CreateSingle();
        foreach (FoodPoisonCause cause in Enum.GetValues(typeof(FoodPoisonCause)))
        {
            FoodUtility.AddFoodPoisoningHediff(victim, ingestible, cause);
        }

        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.FoodPoisoning, "[PAWN] got food poisoning from a simple meal.", exactMatch: true, index: -5);
        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.FoodPoisoning, "[PAWN] got food poisoning from a simple meal because of incompetent cook.", index: -4);
        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.FoodPoisoning, "[PAWN] got food poisoning from a simple meal because of dirty cooking area.", index: -3);
        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.FoodPoisoning, "[PAWN] got food poisoning from a simple meal because of rotten food.", index: -2);
        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.FoodPoisoning, "[PAWN] got food poisoning from a simple meal because of dangerous food type.", index: -1);

        scenario.OpenHistoryRecordTab(victim);
    }

    public void TestCorpse(TestScenario scenario)
    {
        var victim = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .FullHeal()
            .CreateSingle();
        var deadHuman = scenario.Pawn().Do(p => p.Kill(null)).CreateSingle();
        var deadAnimal = scenario.Pawn().Animal(PawnKindDefOf.Muffalo).Do(p => p.Kill(null)).CreateSingle();
        
        FoodUtility.AddFoodPoisoningHediff(victim, deadHuman.Corpse, FoodPoisonCause.Unknown);
        FoodUtility.AddFoodPoisoningHediff(victim, deadAnimal.Corpse, FoodPoisonCause.Unknown);
        Expect.That(victim).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.FoodPoisoning,
            Description = "[PAWN] got food poisoning from [PAWN]'s corpse.",
            Concerns = [deadHuman],
        });
        Expect.That(victim).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.FoodPoisoning,
            Description = "[PAWN] got food poisoning from a muffalo's corpse.",
            Concerns = [deadAnimal],
        });
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
            .CreateSingle();

        scenario.Pawn(victim)
            .Do(p => p.needs.food.CurLevel = 0f)
            .StartJob(JobDefOf.Ingest, ingestible)
            .Execute();

        scenario.SpeedUp();
        Expect.That(victim).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.FoodPoisoning,
            Description = "[PAWN] got food poisoning from a simple meal because of incompetent cook [Cook].",
            Concerns = [cook],
        });

        GameEventBus.SubscribeOnce<FoodPoisoningEvent>(e =>
        {
            Find.Storyteller.difficulty.foodPoisonChanceFactor = oldPoisonChance;
            scenario.SlowDown();
            scenario.OpenHistoryRecordTab(victim);
        });
    }
}
