using System;
using PawnHistory.Source.Helper;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryRemoveBodyPartData(BodyPartRemovalIntent Intent, Hediff BadHediff) : SurgeryEventData;

internal class SurgeryEventDataSource_RemoveBodyPart : SurgeryEventDataSource
{
    protected override Type GetWorkClass()
    {
        return typeof(Recipe_RemoveBodyPart);
    }

    protected override SurgeryEventData Create(RecipeDef recipe, Pawn patient, Pawn doctor, BodyPartRecord part)
    {
        var intent = HealthUtility.PartRemovalIntent(patient, part);
        var badHediff = patient.GetMostDangerousHediff(part);

        return new SurgeryRemoveBodyPartData(intent, badHediff);
    }
}
