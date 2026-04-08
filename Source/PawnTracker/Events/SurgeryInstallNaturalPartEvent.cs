using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryInstallNaturalPartEvent(Pawn Patient, Pawn Doctor, BodyPartRecord Part, Hediff HediffToRemove, Hediff BadHediff) : SurgeryEvent(Patient, Doctor, Part);

class InstallNaturalPartContext : SurgeryContext<SurgeryInstallNaturalPartEvent> { }

[HarmonyPatch(typeof(Recipe_InstallNaturalBodyPart), nameof(Recipe_InstallNaturalBodyPart.ApplyOnPawn))]
internal class Recipe_InstallNaturalBodyPart_ApplyOnPawn_Patch
{
    static void Prefix(Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        if (billDoer == null) return; // not surgery related

        var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.Part == part);
        var hediffToRemove = hediffs.FirstOrDefault(h => h is Hediff_MissingPart || h.IsInstalledBodyPart());
        var badHediff = pawn.GetMostDangerousHediff(part);

        InstallNaturalPartContext.SurgeryRecipe_PreApplyOnPawn(pawn, () => new SurgeryInstallNaturalPartEvent(pawn, billDoer, part, hediffToRemove, badHediff));
    }

    static void Postfix(Pawn pawn)
    {
        InstallNaturalPartContext.SurgeryRecipe_PostApplyOnPawn(pawn);
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
internal class SurgeryOutcomeEffectDef_GetOutcome_Patch_2
{
    static void Postfix(Pawn patient, SurgeryOutcome __result)
    {
        InstallNaturalPartContext.SurgeryOutcomeEffectDef_PostGetOutcome(patient, __result);
    }
}