using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class TestManager
{
    public static int Timeout = 5000;
    public static TestContext Current;

    public static void Reset(string label)
    {
        TestScenario.ClearAll();
        Current = new TestContext(label);
    }

    public static void WaitForTestCompletion(TestContext ctx)
    {
        var start = Find.TickManager.TicksGame;

        TickDelayManager.Interval(1, (a) =>
        {
            if (ctx.PendingEventually == 0)
            {
                if (!ctx.Failed && ctx.AssertionsPassed > 0)
                    ctx.Pass();
                a.Cancelled = true;

                return;
            }

            if (Find.TickManager.TicksGame - start > Timeout)
            {
                ctx.Fail("Timeout waiting for test assertions.");
                a.Cancelled = true;
            }
        });
    }
}