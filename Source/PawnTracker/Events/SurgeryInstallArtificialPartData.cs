using System;
using PawnHistory.Source.Helper;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryInstallArtificialPartData(HediffDef HediffToAdd, Hediff HediffToRemove, Hediff BadHediff, bool IsViolation) : SurgeryEventData;

internal class SurgeryEventDataSource_InstallArtificialPart : SurgeryEventDataSource
{
    protected override Type GetWorkClass()
    {
        return typeof(Recipe_InstallArtificialBodyPart);
    }

    protected override SurgeryEventData Create(RecipeDef recipe, Pawn patient, Pawn doctor, BodyPartRecord part)
    {
        var hediffToAdd = recipe.addsHediff;
        var hediffs = patient.health.hediffSet.hediffs.Where(h => h.Part == part);
        var hediffToRemove = hediffs.FirstOrDefault(h => h is Hediff_MissingPart || h.IsInstalledBodyPart());
        var badHediff = patient.GetMostDangerousHediff(part);
        var isViolation = recipe.addsHediff.addedPartProps is not { betterThanNatural: true } && HealthUtility.PartRemovalIntent(patient, part) == BodyPartRemovalIntent.Harvest;

        return new SurgeryInstallArtificialPartData(hediffToAdd, hediffToRemove, badHediff, isViolation);
    }
}
