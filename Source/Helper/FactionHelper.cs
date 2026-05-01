using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.Helper;

public static class FactionHelper
{
    extension(Faction faction)
    {
        public static Faction OfNonHostile => Find.FactionManager.AllFactions
            .FirstOrDefault(f =>
                !f.IsPlayer &&
                !f.def.hidden &&
                (f.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Neutral || f.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally)
            );
    }
}