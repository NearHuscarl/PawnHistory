using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class Expect
{
    public static PawnHistoryAssertions That(Pawn pawn)
    {
        return new PawnHistoryAssertions(pawn);
    }
}
