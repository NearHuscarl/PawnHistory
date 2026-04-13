using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class Expect
{
    public static PawnHistoryAssertions That(Pawn pawn)
    {
        return new PawnHistoryAssertions([pawn]);
    }
    
    public static PawnHistoryAssertions ThatAll(IEnumerable<Pawn> pawns)
    {
        return new PawnHistoryAssertions(pawns, MatchCondition.All);
    }
    
    public static PawnHistoryAssertions ThatAny(IEnumerable<Pawn> pawns)
    {
        return new PawnHistoryAssertions(pawns, MatchCondition.Any);
    }

    public static PawnHistoryAssertions AnyPawnOnMap()
    {
        return new PawnHistoryAssertions(Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(RecorderManager.ShouldRecord).ToList());
    }
}
