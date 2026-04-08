using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryRemoveBodyPartEvent(Pawn Patient, Pawn Doctor, BodyPartRecord Part, BodyPartRemovalIntent Intent, Hediff BadHediff) : SurgeryEvent(Patient, Doctor, Part);

class RemoveContext : SurgeryContext<SurgeryRemoveBodyPartEvent> { }

[HarmonyPatch(typeof(Recipe_RemoveBodyPart), nameof(Recipe_RemoveBodyPart.ApplyOnPawn))]
internal class Recipe_RemoveBodyPart_ApplyOnPawn_Patch
{
    static void Prefix(Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        if (billDoer == null) return; // not surgery related

        var intent = HealthUtility.PartRemovalIntent(pawn, part);
        var badHediff = pawn.GetMostDangerousHediff(part);

        RemoveContext.SurgeryRecipe_PreApplyOnPawn(pawn, () => new SurgeryRemoveBodyPartEvent(pawn, billDoer, part, intent, badHediff));
    }

    static void Postfix(Pawn pawn)
    {
        RemoveContext.SurgeryRecipe_PostApplyOnPawn(pawn);
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
internal class SurgeryOutcomeEffectDef_GetOutcome_Patch
{
    static void Postfix(Pawn patient, SurgeryOutcome __result)
    {
        RemoveContext.SurgeryOutcomeEffectDef_PostGetOutcome(patient, __result);
    }
}
