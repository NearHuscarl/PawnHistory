using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

internal class SurgeryInstallArtificialPartEvent(Pawn patient, Pawn doctor, BodyPartRecord part, HediffDef hediffToAdd, Hediff hediffToRemove, Hediff badHediff, bool isViolation) : SurgeryEvent(patient, doctor, part)
{
    public HediffDef HediffToAdd { get; } = hediffToAdd;
    public Hediff HediffToRemove { get; } = hediffToRemove;
    public Hediff BadHediff { get; } = badHediff;
    public bool IsViolation { get; } = isViolation;
}

class InstallArtificialPartContext : SurgeryContext<SurgeryInstallArtificialPartEvent> { }

[HarmonyPatch(typeof(Recipe_InstallArtificialBodyPart), nameof(Recipe_InstallArtificialBodyPart.ApplyOnPawn))]
internal class Recipe_InstallArtificialBodyPart_ApplyOnPawn_Patch
{
    static void Prefix(Recipe_InstallArtificialBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        var recipe = __instance.recipe;
        var hediffToAdd = recipe.addsHediff;
        var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.Part == part);
        var hediffToRemove = hediffs.FirstOrDefault(h => h is Hediff_MissingPart || h.IsInstalledBodyPart());
        var badHediff = pawn.GetMostDangerousHediff(part);
        var isViolation = (recipe.addsHediff.addedPartProps == null || !recipe.addsHediff.addedPartProps.betterThanNatural) && HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;

        InstallArtificialPartContext.SurgeryRecipe_PreApplyOnPawn(pawn, () => new SurgeryInstallArtificialPartEvent(pawn, billDoer, part, hediffToAdd, hediffToRemove, badHediff, isViolation));
    }

    static void Postfix(Pawn pawn)
    {
        InstallArtificialPartContext.SurgeryRecipe_PostApplyOnPawn(pawn);
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
internal class SurgeryOutcomeEffectDef_GetOutcome_Patch_3
{
    static void Postfix(Pawn patient, SurgeryOutcome __result)
    {
        InstallArtificialPartContext.SurgeryOutcomeEffectDef_PostGetOutcome(patient, __result);
    }
}