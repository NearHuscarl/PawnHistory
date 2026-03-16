using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker;

public static class PawnEventDefExtension
{
    public static HistoryDescriptionBuilder ResolveDescription(this HistoryRecordDef eventDef, string rootKeyword, Pawn pawn)
    {
        return new HistoryDescriptionBuilder(eventDef, rootKeyword, pawn);
    }

    public static HistoryDescriptionBuilder ResolveDescription(this HistoryRecordDef eventDef, Pawn pawn)
    {
        return new HistoryDescriptionBuilder(eventDef, null, pawn);
    }
}
