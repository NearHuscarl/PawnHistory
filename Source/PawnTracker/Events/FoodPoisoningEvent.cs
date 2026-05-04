using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record FoodPoisoningEvent(Pawn Victim, Thing Ingestible, FoodPoisonCause Cause, Pawn Cook) : GameEventBase;

public class CompCookTracker : ThingComp
{
    public Pawn Cook;

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
        piece.TryGetComp<CompCookTracker>()?.Cook = Cook;
    }

    public override void PreAbsorbStack(Thing otherStack, int count)
    {
        base.PreAbsorbStack(otherStack, count);

        var otherTracker = otherStack.TryGetComp<CompCookTracker>();
        if (otherTracker?.Cook == null)
            return;

        var thisPoison = parent.TryGetComp<CompFoodPoisonable>();
        var otherPoison = otherStack.TryGetComp<CompFoodPoisonable>();

        var currentWeight = thisPoison.PoisonPercent * parent.stackCount;
        var incomingWeight = otherPoison.PoisonPercent * count;

        if (incomingWeight > currentWeight)
            Cook = otherTracker.Cook;
        else
            Cook ??= otherTracker.Cook;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref Cook, "PH_cook");
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
        __instance.parent.TryGetComp<CompCookTracker>()?.Cook = pawn;
    }
}

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.AddFoodPoisoningHediff))]
internal class FoodUtility_AddFoodPoisoningHediff_Patch
{
    private static void Postfix(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
    {
        var cook = ingestible.TryGetComp<CompCookTracker>()?.Cook;
        GameEventBus.Publish(new FoodPoisoningEvent(pawn, ingestible, cause, cook));
    }
}
