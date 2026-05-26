using System;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryAddHediffData(HediffDef HediffToAdd) : SurgeryEventData;

internal class SurgeryEventDataSource_AddHediff : SurgeryEventDataSource
{
    protected override Type GetWorkClass()
    {
        return typeof(Recipe_AddHediff);
    }

    protected override SurgeryEventData Create(RecipeDef recipe, Pawn patient, Pawn doctor, BodyPartRecord part)
    {
        return new SurgeryAddHediffData(recipe.addsHediff);
    }
}
