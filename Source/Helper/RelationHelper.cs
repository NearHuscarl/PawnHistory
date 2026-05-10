using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.Helper;

public static class RelationHelper
{
    public static bool TryGetBondedHumans(Pawn animal, out List<Pawn> bondedHumans)
    {
        bondedHumans = [];

        if (animal?.RaceProps is not { Animal: true })
            return false;

        foreach (var rel in animal.relations?.DirectRelations ?? [])
        {
            if (rel.def == PawnRelationDefOf.Bond)
                bondedHumans.Add(rel.otherPawn);
        }

        return bondedHumans.Count > 0;
    }
    
    public static List<Pawn> GetPawnsWithRelation(this Pawn pawn, PawnRelationDef relation)
    {
        return pawn.relations.DirectRelations
            .Where(r =>  r.def == relation &&  r.otherPawn is { Dead: false })
            .Select(r => r.otherPawn)
            .ToList();
    }
}