using System;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryInstallNaturalPartData(Hediff HediffToRemove, Hediff BadHediff) : SurgeryEventData;

internal class SurgeryEventDataSource_InstallNaturalPart : SurgeryEventDataSource
{
    protected override Type GetWorkClass()
    {
        return typeof(Recipe_InstallNaturalBodyPart);
    }

    protected override SurgeryEventData Create(RecipeDef recipe, Pawn patient, Pawn doctor, BodyPartRecord part)
    {
        var hediffs = patient.health.hediffSet.hediffs.Where(h => h.Part == part);
        var hediffToRemove = hediffs.FirstOrDefault(h => h is Hediff_MissingPart || h.IsInstalledBodyPart());
        var badHediff = patient.GetMostDangerousHediff(part);

        return new SurgeryInstallNaturalPartData(hediffToRemove, badHediff);
    }
}
