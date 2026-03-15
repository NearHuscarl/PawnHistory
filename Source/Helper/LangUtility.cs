using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source.Helper;

internal static class LangUtility
{
    public static TaggedString FormatPawnList(List<Pawn> pawns)
    {
        int count = pawns.Count;
        if (count == 0) return string.Empty;

        string n1 = pawns[0].NameShortColored.Resolve();
        if (count == 1) return n1;

        string n2 = pawns[1].NameShortColored.Resolve();
        if (count == 2) return "NH_PH_PawnList_Two".Translate(n1, n2);
        if (count == 3) return "NH_PH_PawnList_Three".Translate(n1, n2);

        return "NH_PH_PawnList_Many".Translate(n1, n2, count - 2);
    }
}
