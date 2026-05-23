using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

public static class SurgeryOutcomes
{
    private enum BaseOutcomeIndex
    {
        Success = 0,
        Death = 1,
        CatastrophicFailure = 2,
        RidiculousFailure = 3,
        IudSterilizedFailure = 4,
        MinorFailure = 6
    }

    private enum MinorFailureOutcomeIndex
    {
        Success = 0,
        Failure = 1
    }

    private static SurgeryOutcomeEffectDef Base => DefDatabase<SurgeryOutcomeEffectDef>.GetNamed("SurgeryOutcomeBase");
    private static SurgeryOutcomeEffectDef MinorFailureDef => DefDatabase<SurgeryOutcomeEffectDef>.GetNamed("SurgeryOutcomeMinorFailure");

    public static SurgeryOutcome Success => BaseOutcome(BaseOutcomeIndex.Success);
    public static SurgeryOutcome Death => BaseOutcome(BaseOutcomeIndex.Death);
    public static SurgeryOutcome CatastrophicFailure => BaseOutcome(BaseOutcomeIndex.CatastrophicFailure);
    public static SurgeryOutcome RidiculousFailure => BaseOutcome(BaseOutcomeIndex.RidiculousFailure);
    public static SurgeryOutcome SterilizedFailure => BaseOutcome(BaseOutcomeIndex.IudSterilizedFailure);
    public static SurgeryOutcome MinorFailure => BaseOutcome(BaseOutcomeIndex.MinorFailure);
    public static SurgeryOutcome MinorOnlyFailure => MinorFailureOutcome(MinorFailureOutcomeIndex.Failure);

    private static SurgeryOutcome BaseOutcome(BaseOutcomeIndex index) => Base.outcomes[(int)index];

    private static SurgeryOutcome MinorFailureOutcome(MinorFailureOutcomeIndex index)
    {
        return MinorFailureDef.outcomes[(int)index];
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
public static class ForcedSurgeryOutcome
{
    private static bool Prefix(
        RecipeDef recipe,
        Pawn surgeon,
        Pawn patient,
        BodyPartRecord part,
        ref SurgeryOutcome __result)
    {
        var outcome = TestManager.Scenario.SurgeryForcedOutcome;
        if (outcome == null)
            return true;

        var oldChance = recipe.deathOnFailedSurgeryChance;
        if (outcome.GetType() == typeof(SurgeryOutcome_Death))
            recipe.deathOnFailedSurgeryChance = 1f;

        try
        {
            outcome.Apply(1f, recipe, surgeon, patient, part);
        }
        finally
        {
            recipe.deathOnFailedSurgeryChance = oldChance;
        }

        __result = outcome;
        return false;
    }
}

[HarmonyPatch(typeof(SurgeryOutcome_Failure), "CanApply")]
internal static class ForcedSurgeryOutcome_CanApply_Patch
{
    private static bool Prefix(SurgeryOutcome_Failure __instance, ref bool __result)
    {
        var outcome = TestManager.Scenario.SurgeryForcedOutcome;
        if (outcome == null || !ReferenceEquals(__instance, outcome))
            return true;

        __result = true;
        return false;
    }
}
