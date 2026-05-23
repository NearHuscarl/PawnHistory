using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryInstallImplantEvent(Pawn Patient, Pawn Doctor, BodyPartRecord Part, HediffDef HediffToAdd) : SurgeryEvent(Patient, Doctor, Part);

file class InstallImplantContext : SurgeryContext<SurgeryInstallImplantEvent>;

[HarmonyPatch(typeof(Recipe_InstallImplant), nameof(Recipe_InstallImplant.ApplyOnPawn))]
internal class Recipe_InstallImplant_ApplyOnPawn_Patch
{
    private static void Prefix(Recipe_InstallImplant __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        if (billDoer == null) return; // not surgery related
        var hediffToAdd = __instance.recipe.addsHediff;

        InstallImplantContext.SurgeryRecipe_PreApplyOnPawn(pawn, () => new SurgeryInstallImplantEvent(pawn, billDoer, part, hediffToAdd));
    }

    private static void Postfix(Pawn pawn)
    {
        InstallImplantContext.SurgeryRecipe_PostApplyOnPawn(pawn);
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
internal class SurgeryOutcomeEffectDef_GetOutcome_Patch_1
{
    private static void Postfix(Pawn patient, SurgeryOutcome __result)
    {
        InstallImplantContext.SurgeryOutcomeEffectDef_PostGetOutcome(patient, __result);
    }
}