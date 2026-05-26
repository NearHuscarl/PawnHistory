using System;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryRemoveHediffData(HediffDef HediffToRemove) : SurgeryEventData;

internal class SurgeryEventDataSource_RemoveHediff : SurgeryEventDataSource
{
    protected override Type GetWorkClass()
    {
        return typeof(Recipe_RemoveHediff);
    }

    protected override SurgeryEventData Create(RecipeDef recipe, Pawn patient, Pawn doctor, BodyPartRecord part)
    {
        return new SurgeryRemoveHediffData(recipe.removesHediff);
    }
}
