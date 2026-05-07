using System.Linq;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test;

internal static class PawnBuilderIdeologyExtension
{
    public static PawnBuilder RemoveIdeo(this PawnBuilder builder)
    {
        return builder.Do(p => p.ideo.SetIdeo(null));
    }
    
    public static PawnBuilder SetIdeo(this PawnBuilder builder, Ideo ideo = null, PreceptDef role = null, float? certainty = null)
    {
        builder.Do(p => p.ideo.SetIdeo(ideo ?? Faction.OfPlayer.ideos.PrimaryIdeo));

        if (role != null)
            builder.SetRole(role);

        if (certainty.HasValue)
            builder.SetIdeoCertainty(certainty.Value);

        return builder;
    }
    
    public static PawnBuilder SetIdeoCertainty(this PawnBuilder builder, float certainty)
    {
        return builder.Do(p =>
        {
            p.ideo.OffsetCertainty(certainty - p.ideo.Certainty);
        });
    }
    
    public static PawnBuilder SetRole(this PawnBuilder builder, PreceptDef roleDef)
    {
        return builder.Do(p =>
        {
            var role = (Precept_RoleSingle)p.Ideo.RolesListForReading.First(r => r.def == roleDef);
            role.Assign(p, addThoughts: false);
        });
    }
}
