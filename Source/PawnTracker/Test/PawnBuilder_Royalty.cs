using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test;

internal static class PawnBuilderRoyaltyExtension
{
    public static PawnBuilder SetRoyalTitle(this PawnBuilder builder, RoyalTitleDef royalTitle)
    {
        return builder.Do(p =>
        {
            if (p.royalty.GetCurrentTitle(Faction.OfEmpire) == royalTitle)
                return;
            p.royalty.SetTitle(Faction.OfEmpire, royalTitle, grantRewards: false);
        });
    }
}
