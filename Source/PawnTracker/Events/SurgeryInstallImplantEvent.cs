using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

internal class SurgeryInstallImplantEvent(Pawn patient, Pawn doctor, BodyPartRecord part, HediffDef hediffToAdd) : SurgeryEvent(patient, doctor, part)
{
    public HediffDef HediffToAdd { get; } = hediffToAdd;
}

class InstallImplantContext : SurgeryContext<SurgeryInstallImplantEvent> { }

[HarmonyPatch(typeof(Recipe_InstallImplant), nameof(Recipe_InstallImplant.ApplyOnPawn))]
internal class Recipe_InstallImplant_ApplyOnPawn_Patch
{
    static void Prefix(Recipe_InstallImplant __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        var hediffToAdd = __instance.recipe.addsHediff;

        InstallImplantContext.SurgeryRecipe_PreApplyOnPawn(pawn, () => new SurgeryInstallImplantEvent(pawn, billDoer, part, hediffToAdd));
    }

    static void Postfix(Pawn pawn)
    {
        InstallImplantContext.SurgeryRecipe_PostApplyOnPawn(pawn);
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
internal class SurgeryOutcomeEffectDef_GetOutcome_Patch_1
{
    static void Postfix(Pawn patient, SurgeryOutcome __result)
    {
        InstallImplantContext.SurgeryOutcomeEffectDef_PostGetOutcome(patient, __result);
    }
}