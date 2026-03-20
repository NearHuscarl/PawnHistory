using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source.Helper;

// Shamelessly copied from Number
public static class HediffHelper
{
    public static string LabelNounFull(this Hediff h)
    {
        var labelNoun = h.def.labelNoun ?? h.LabelBase;

        if (h.LabelInBrackets.NullOrEmpty())
            return labelNoun.ToLower().Colorize(h.LabelColor);

        return $"{labelNoun.ToLower()} ({h.LabelInBrackets})".Colorize(h.LabelColor);
    }

    public static bool IsImplant(this Hediff h)
    {
        return h.def.addedPartProps != null;
    }

    public static IEnumerable<Hediff> VisibleHediffs(Pawn pawn)
    {
        var mpca = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
        foreach (var t in mpca)
            yield return t;

        var visibleDiffs = pawn.health.hediffSet.hediffs.Where(d => d is not Hediff_MissingPart && d.Visible);

        foreach (var diff in visibleDiffs)
            yield return diff;
    }

    private static float GetListPriority(BodyPartRecord rec)
        => rec == null
            ? 9999999f
            : (int)rec.height * 10000 + rec.coverageAbsWithChildren;

    private static IEnumerable<IGrouping<BodyPartRecord, Hediff>> VisibleHediffGroupsInOrder(Pawn pawn)
        => VisibleHediffs(pawn)
            .GroupBy(x => x.Part)
            .OrderByDescending(x => GetListPriority(x.First().Part));

    public static string GetHediffText(Pawn pawn)
    {
        var res = new StringBuilder();

        foreach (var diffs in VisibleHediffGroupsInOrder(pawn))
        {
            foreach (var current in diffs.GroupBy(x => x.UIGroupKey))
            {
                var count = current.Count();
                var text = current.First().LabelCap;
                if (count != 1)
                {
                    text = text + " x" + count;
                }
                res.AppendWithComma(text);
            }
        }
        return res.ToString();
    }
}
