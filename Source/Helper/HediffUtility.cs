using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public static bool IsInstalledBodyPart(this Hediff h)
    {
        return h.def.addedPartProps != null;
    }

    private static string FormatDamageSource(ThingDef sourceDef, BodyPartGroupDef sourceBodyPartGroup, string sourceToolLabel, string sourceLabel)
    {
        // pawn (fist, bite, etc.), returns "right fist" rather than "human fist"
        if (sourceDef?.race != null)
            return sourceBodyPartGroup?.label ?? sourceToolLabel;

        // weapon
        if (!sourceToolLabel.NullOrEmpty())
            return "SourceToolLabel".Translate((NamedArgument)sourceLabel, (NamedArgument)sourceToolLabel).Resolve();

        if (sourceBodyPartGroup != null)
            return "SourceToolLabel".Translate((NamedArgument)sourceLabel, (NamedArgument)sourceBodyPartGroup.LabelShort).Resolve();

        return sourceLabel;
    }

    // Reference: Hediff_Injury.LabelInBrackets
    public static string GetDamageSource(this Hediff h) => FormatDamageSource(h.sourceDef, h.sourceBodyPartGroup, h.sourceToolLabel, h.sourceLabel);

    // Reference: DamageWorker_AddInjury.FinalizeAndAddInjury
    public static string GetDamageSource(this DamageInfo dinfo)
    {
        var sourceDef = dinfo.Weapon;
        var sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
        var sourceToolLabel = dinfo.Tool?.labelNoLocation ?? dinfo.Tool?.label;

        string sourceLabel;

        if (dinfo.Instigator is Pawn instigator && instigator.IsMutant && dinfo.Weapon == ThingDefOf.Human)
            sourceLabel = instigator.mutant.Def.label;
        else
            sourceLabel = dinfo.Weapon?.label ?? "";

        return FormatDamageSource(sourceDef, sourceBodyPartGroup, sourceToolLabel, sourceLabel);
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
