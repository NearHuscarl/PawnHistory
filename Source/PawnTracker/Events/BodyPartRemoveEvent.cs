using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

internal class BodyPartRemoveEvent(Pawn patient, Pawn doctor, Hediff hediff, BodyPartRecord part, BodyPartRemovalIntent intent, Hediff badHediff) : GameEventBase
{
    public Pawn Patient { get; } = patient;
    public Pawn Doctor { get; } = doctor;
    public Hediff Hediff { get; } = hediff;
    public BodyPartRecord Part { get; } = part;
    public BodyPartRemovalIntent Intent { get; } = intent;
    public Hediff BadHediff { get; } = badHediff;
    public List<Hediff> NewInjuries { get; set; }
    public SurgeryOutcome Outcome { get; set; }
}

class BodyPartRemoveContext()
{
    public BodyPartRemoveEvent e;
    public List<Hediff> injurySnapshot;

    public static readonly Dictionary<Pawn, BodyPartRemoveContext> PendingSurgeries = [];

    /// <summary>
    /// Copied from HealthUtility.PartRemovalIntent(), but return the target part's hediff
    /// </summary>
    public static Hediff GetBadHediff(Pawn pawn, BodyPartRecord part)
    {
        return pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.Visible && h.Part == part && h.def.isBad);
    }

    public static List<Hediff> GetInjurySnapshot(Pawn pawn) => pawn.health.hediffSet.hediffs.Where(h => h is Hediff_Injury).ToList();
}

// Call order:
// Recipe_RemoveBodyPart.ApplyOnPawn() prefix
// SurgeryOutcomeEffectDef.GetOutcome()
// Recipe_RemoveBodyPart.ApplyOnPawn() postfix

[HarmonyPatch(typeof(Recipe_RemoveBodyPart), nameof(Recipe_RemoveBodyPart.ApplyOnPawn))]
internal class Recipe_RemoveBodyPart_ApplyOnPawn_Patch
{
    static void Prefix(Recipe_RemoveBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        var hediffToRemove = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == __instance.recipe.removesHediff);
        var intent = HealthUtility.PartRemovalIntent(pawn, part);
        var badHediff = BodyPartRemoveContext.GetBadHediff(pawn, part);

        BodyPartRemoveContext.PendingSurgeries[pawn] = new BodyPartRemoveContext()
        {
            e = new BodyPartRemoveEvent(pawn, billDoer, hediffToRemove, part, intent, badHediff),
            injurySnapshot = BodyPartRemoveContext.GetInjurySnapshot(pawn),
        };
    }

    static void Postfix(Recipe_RemoveBodyPart __instance, Pawn pawn, BodyPartRecord part)
    {
        if (!BodyPartRemoveContext.PendingSurgeries.TryGetValue(pawn, out var ctx))
            return;
        BodyPartRemoveContext.PendingSurgeries.Remove(pawn);

        // Injury hediffs are those added to the part during the failed surgery
        // (e.g. surgical cut, etc.) - compare snapshot to current state
        ctx.e.NewInjuries = BodyPartRemoveContext.GetInjurySnapshot(pawn)
            .Except(ctx.injurySnapshot)
            .OrderByDescending(h => h.Severity)
            .ToList();

        GameEventBus.Publish(ctx.e);
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
internal class SurgeryOutcomeEffectDef_GetOutcome_Patch
{
    static void Postfix(Pawn patient, SurgeryOutcome __result)
    {
        if (!BodyPartRemoveContext.PendingSurgeries.TryGetValue(patient, out var ctx))
            return;
        ctx.e.Outcome = __result;
    }
}
