using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record FoodPoisoningEvent(Pawn Victim, Thing Ingestible, FoodPoisonCause Cause, Pawn Cook) : GameEventBase;

public class CompCookTracker : ThingComp
{
    public Pawn cook;

    public static void InjectComp()
    {
        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            if (def.HasComp(typeof(CompFoodPoisonable)))
            {
                def.comps.Add(new CompProperties() { compClass = typeof(CompCookTracker) });
            }
        }
    }

    public override void PostSplitOff(Thing piece)
    {
        base.PostSplitOff(piece);
        piece.TryGetComp<CompCookTracker>()?.cook = cook;
    }

    public override void PreAbsorbStack(Thing otherStack, int count)
    {
        base.PreAbsorbStack(otherStack, count);

        var otherTracker = otherStack.TryGetComp<CompCookTracker>();
        if (otherTracker?.cook == null)
            return;

        var thisPoison = parent.TryGetComp<CompFoodPoisonable>();
        var otherPoison = otherStack.TryGetComp<CompFoodPoisonable>();

        var currentWeight = thisPoison.PoisonPercent * parent.stackCount;
        var incomingWeight = otherPoison.PoisonPercent * count;

        if (incomingWeight > currentWeight)
            cook = otherTracker.cook;
        else
            cook ??= otherTracker.cook;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref cook, "PH_cook");
    }
}

// Call order:
// CompFoodPoisonable.Notify_RecipeProduced()
// ...
// FoodUtility.AddFoodPoisoningHediff()

[HarmonyPatch(typeof(CompFoodPoisonable), nameof(CompFoodPoisonable.Notify_RecipeProduced))]
internal class CompFoodPoisonable_Notify_RecipeProduced_Patch
{
    private static void Postfix(CompFoodPoisonable __instance, Pawn pawn)
    {
        __instance.parent.TryGetComp<CompCookTracker>()?.cook = pawn;
    }
}

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.AddFoodPoisoningHediff))]
internal class FoodUtility_AddFoodPoisoningHediff_Patch
{
    private static void Postfix(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
    {
        var cook = ingestible.TryGetComp<CompCookTracker>()?.cook;
        GameEventBus.Publish(new FoodPoisoningEvent(pawn, ingestible, cause, cook));
    }
}
