using System;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryInstallImplantData(HediffDef HediffToAdd) : SurgeryEventData;

internal class SurgeryEventDataSource_InstallImplant : SurgeryEventDataSource
{
    protected override Type GetWorkClass()
    {
        return typeof(Recipe_InstallImplant);
    }

    protected override SurgeryEventData Create(RecipeDef recipe, Pawn patient, Pawn doctor, BodyPartRecord part)
    {
        return new SurgeryInstallImplantData(recipe.addsHediff);
    }
}
