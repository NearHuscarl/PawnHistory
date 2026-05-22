using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

internal static class PawnBuilderBiotechExtension
{
    public static PawnBuilder SetGrowthTier(this PawnBuilder builder, int tier)
    {
        return builder.Do(p =>
        {
            p.ageTracker.growthPoints = GrowthUtility.GrowthTiers[tier].pointsRequirement;
            p.ageTracker.canGainGrowthPoints = true;
        });
    }

    public static PawnBuilder AddGenes(this PawnBuilder builder, List<GeneDef> genes)
    {
        return builder.Do(p =>
        {
            foreach (var gene in genes)
                p.genes?.AddGene(gene, xenogene: true);
        });
    }

    public static PawnBuilder SetXenotype(this PawnBuilder builder, XenotypeDef xenotype)
    {
        return builder.Do(p => p.genes?.SetXenotype(xenotype));
    }

    public static PawnBuilder SetXenotype(this PawnBuilder builder, string xenotypeName, XenotypeIconDef iconDef, List<GeneDef> genes)
    {
        return builder.Do(p =>
        {
            if (p.genes == null)
                return;

            p.genes.SetXenotype(XenotypeDefOf.Baseliner);
            p.genes.xenotypeName = xenotypeName;
            p.genes.iconDef = iconDef;

            foreach (var gene in genes)
                p.genes.AddGene(gene, xenogene: true);
        });
    }
}
