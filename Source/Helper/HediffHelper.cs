using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source.Helper
{
    // Shamelessly copied from Number
    class HediffHelper
    {
        public static IEnumerable<Hediff> VisibleHediffs(Pawn pawn)
        {
            List<Hediff_MissingPart> mpca = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
            foreach (Hediff_MissingPart t in mpca)
            {
                yield return t;
            }

            IEnumerable<Hediff> visibleDiffs = pawn.health.hediffSet.hediffs.Where(d => !(d is Hediff_MissingPart) && d.Visible);
            foreach (Hediff diff in visibleDiffs)
            {
                yield return diff;
            }
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
            var icontipBuilder = new StringBuilder();

            foreach (IGrouping<BodyPartRecord, Hediff> diffs in VisibleHediffGroupsInOrder(pawn))
            {
                foreach (IGrouping<int, Hediff> current in diffs.GroupBy(x => x.UIGroupKey))
                {
                    int count = current.Count();
                    string text = current.First().LabelCap;
                    if (count != 1)
                    {
                        text = text + " x" + count;
                    }
                    icontipBuilder.AppendWithComma(text);
                }
            }
            return icontipBuilder.ToString();
        }
    }
}
