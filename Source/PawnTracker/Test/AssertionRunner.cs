using System;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

internal readonly record struct AssertionRunOptions(bool Eventually = false, int TimeoutTicks = 1, int PollIntervalTicks = 0);

internal static class AssertionRunner
{
    public static void RunAssertion(Action assertion, AssertionSource source, AssertionRunOptions options = default)
    {
        var ctx = TestManager.Ctx;
        if (!ctx.TryRegisterAssertion())
            return;

        ctx.PendingEventually++;
        // delayed, so test methods can return cleanup action first even if synchronous test call failed.
        TickDelayManager.Delay(0, () => DoRunAssertion(assertion, options, source));
    }

    private static void DoRunAssertion(Action assertion, AssertionRunOptions options, AssertionSource source)
    {
        var ctx = TestManager.Ctx;
        var tickStart = Find.TickManager.TicksGame;
        Exception lastException = null;

        var action = TickDelayManager.Interval(options.PollIntervalTicks, a =>
        {
            if (!options.Eventually)
            {
                RunOnce(assertion, source);
                a.Cancelled = true;
                return;
            }

            if (Find.TickManager.TicksGame - tickStart > options.TimeoutTicks)
            { 
                var failure = new TimeoutFailure(ctx.TestId, $"Test assertion failed after waiting for {options.TimeoutTicks} ticks.");
                ctx.Fail(new TestException(failure, lastException, source));
                a.Cancelled = true;
                return;
            }

            try
            {
                assertion();
                ctx.Pass();
                a.Cancelled = true;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        });

        ctx.OnCleanup(() => action.Data.Cancelled = true);
    }

    private static void RunOnce(Action assertion, AssertionSource source)
    {
        var ctx = TestManager.Ctx;
        
        try
        {
            assertion();
            ctx.Pass();
        }
        catch (Exception ex)
        {
            ctx.Fail(ex, source);
        }
    }
}