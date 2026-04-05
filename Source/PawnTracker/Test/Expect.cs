using PawnHistory.Source.PawnTracker.Recorders;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class Expect
{
    public static PawnHistoryAssertions That(Pawn pawn)
    {
        return new PawnHistoryAssertions([pawn]);
    }

    public static PawnHistoryAssertions AnyPawnOnMap()
    {
        return new PawnHistoryAssertions(Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(RecorderManager.ShouldRecord).ToList());
    }
}
