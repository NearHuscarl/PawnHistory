using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test;

internal static class PawnBuilderIdeologyExtension
{
    public static PawnBuilder SetIdeo(this PawnBuilder builder, Ideo ideo = null)
    {
        return builder.Do(p =>
        {
            p.ideo.SetIdeo(ideo ?? Faction.OfPlayer.ideos.PrimaryIdeo);
        });
    }
    public static PawnBuilder SetIdeoCertainty(this PawnBuilder builder, float certainty)
    {
        return builder.Do(p =>
        {
            p.ideo.OffsetCertainty(certainty - p.ideo.Certainty);
        });
    }
}
