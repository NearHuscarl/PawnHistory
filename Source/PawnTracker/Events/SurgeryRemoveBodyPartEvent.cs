using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

internal class SurgeryRemoveBodyPartEvent(Pawn patient, Pawn doctor, BodyPartRecord part, BodyPartRemovalIntent intent, Hediff badHediff) : SurgeryEvent(patient, doctor, part)
{
    public BodyPartRemovalIntent Intent { get; } = intent;
    public Hediff BadHediff { get; } = badHediff;
}

class RemoveContext : SurgeryContext<SurgeryRemoveBodyPartEvent> { }

[HarmonyPatch(typeof(Recipe_RemoveBodyPart), nameof(Recipe_RemoveBodyPart.ApplyOnPawn))]
internal class Recipe_RemoveBodyPart_ApplyOnPawn_Patch
{
    static void Prefix(Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        var intent = HealthUtility.PartRemovalIntent(pawn, part);
        var badHediff = GetBadHediff(pawn, part);

        RemoveContext.SurgeryRecipe_PreApplyOnPawn(pawn, () => new SurgeryRemoveBodyPartEvent(pawn, billDoer, part, intent, badHediff));
    }

    /// <summary>
    /// Copied from HealthUtility.PartRemovalIntent(), but return the target part's hediff
    /// </summary>
    private static Hediff GetBadHediff(Pawn pawn, BodyPartRecord part)
    {
        return pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.Visible && h.Part == part && h.def.isBad);
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
