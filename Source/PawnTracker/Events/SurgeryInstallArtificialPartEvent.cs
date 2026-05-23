using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryInstallArtificialPartEvent(Pawn Patient, Pawn Doctor, BodyPartRecord Part, HediffDef HediffToAdd, Hediff HediffToRemove, Hediff BadHediff, bool IsViolation) : SurgeryEvent(Patient, Doctor, Part);

file class InstallArtificialPartContext : SurgeryContext<SurgeryInstallArtificialPartEvent>;

[HarmonyPatch(typeof(Recipe_InstallArtificialBodyPart), nameof(Recipe_InstallArtificialBodyPart.ApplyOnPawn))]
internal class Recipe_InstallArtificialBodyPart_ApplyOnPawn_Patch
{
    private static void Prefix(Recipe_InstallArtificialBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        if (billDoer == null) return; // not surgery related

        var recipe = __instance.recipe;
        var hediffToAdd = recipe.addsHediff;
        var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.Part == part);
        var hediffToRemove = hediffs.FirstOrDefault(h => h is Hediff_MissingPart || h.IsInstalledBodyPart());
        var badHediff = pawn.GetMostDangerousHediff(part);
        var isViolation = recipe.addsHediff.addedPartProps is not { betterThanNatural: true } && HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;

        InstallArtificialPartContext.SurgeryRecipe_PreApplyOnPawn(pawn, () => new SurgeryInstallArtificialPartEvent(pawn, billDoer, part, hediffToAdd, hediffToRemove, badHediff, isViolation));
    }

    private static void Postfix(Pawn pawn)
    {
        InstallArtificialPartContext.SurgeryRecipe_PostApplyOnPawn(pawn);
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
internal class SurgeryOutcomeEffectDef_GetOutcome_Patch_3
{
    private static void Postfix(Pawn patient, SurgeryOutcome __result)
    {
        InstallArtificialPartContext.SurgeryOutcomeEffectDef_PostGetOutcome(patient, __result);
    }
}