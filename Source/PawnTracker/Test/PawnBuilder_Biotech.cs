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
}
