using RimWorld;

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
}
