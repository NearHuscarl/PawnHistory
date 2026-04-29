using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class Expect
{
    private static TestContext Ctx(string caller) => TestManager.Ctx ?? throw new InvalidOperationException($"{nameof(Expect)}.{caller} can only be used during a test run.");
    
    public static void Assertions(int count)
    {
        Ctx(nameof(Assertions)).DeclareExpectedAssertions(count);
    }

    public static PawnHistoryAssertions That(Pawn pawn)
    {
        Ctx(nameof(That));
        return new PawnHistoryAssertions([pawn]);
    }
    
    public static PawnHistoryAssertions ThatAll(IEnumerable<Pawn> pawns)
    {
        Ctx(nameof(ThatAll));
        return new PawnHistoryAssertions(pawns, MatchCondition.All);
    }
    
    public static PawnHistoryAssertions ThatAny(IEnumerable<Pawn> pawns)
    {
        Ctx(nameof(ThatAny));
        return new PawnHistoryAssertions(pawns, MatchCondition.Any);
    }
    
    public static SimpleAssertions<T> That<T>(T value)
    {
        Ctx(nameof(That));
        return new SimpleAssertions<T>(value);
    }
    
    public static SimpleAssertions<T> That<T>(IEnumerable<T> value)
    {
        Ctx(nameof(That));
        return new SimpleAssertions<T>(value);
    }
    
    public static SimpleAssertions<T> That<T>(List<T> value)
    {
        Ctx(nameof(That));
        return new SimpleAssertions<T>(value);
    }
}
