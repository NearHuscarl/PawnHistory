using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.WorldPawn
{
    public class PawnTable_WorldPawns(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight) : PawnTable(def, pawnsGetter, uiWidth, uiHeight)
    {
        protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
        {
            return PlayerPawnsDisplayOrderUtility.InOrder(input);
        }
    }
}
