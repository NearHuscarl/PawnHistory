using PawnHistory.Source.Helper;
using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class TestManager
{
    public static int Timeout = 5000;
    public static TestContext Ctx;
    private static readonly Queue<Action> queue = new();
    private static bool isRunningTest;

    public static void QueueTest(Action testAction, string label)
    {
        queue.Enqueue(() =>
        {
            try
            {
                ResetBeforeTest(label);
                GameUtility.CreateTestGame(() =>
                {
                    Log.Message("[PawnHistory] Starting test: " + label);
                    // GameComponent initiated, can create scheduled action again here.
                    TickDelayManager.Delay(50, () =>
                    {
                        testAction();
                        WaitForTestCompletion(Ctx, result =>
                        {
                            isRunningTest = result;
                            if (isRunningTest)
                                RunNext();
                            else
                                queue.Clear();
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                isRunningTest = false;
                Log.Error($"[PawnHistory] [Failed] {label}\n\n{ex}");
            }
        });
    }

    public static void Run()
    {
        if (isRunningTest) return;
        isRunningTest = true;
        RunNext();
    }

    private static void RunNext()
    {
        if (queue.Count == 0)
        {
            isRunningTest = false;
            Log.Message("[PawnHistory] All tests finished.");
            return;
        }

        var next = queue.Dequeue();
        next();
    }

    public static void WaitForTestCompletion(TestContext ctx, Action<bool> onCompleted = null)
    {
        var start = Find.TickManager.TicksGame;
        var scheduled = TickDelayManager.Interval(1, (a) =>
        {
            if (ctx.PendingEventually == 0)
            {
                if (ctx.AssertionsFailed == 0 && ctx.AssertionsPassed > 0)
                    ctx.Pass();
                a.Cancelled = true;
                onCompleted?.Invoke(ctx.AssertionsFailed == 0);
                return;
            }

            if (Find.TickManager.TicksGame - start > Timeout)
            {
                a.Cancelled = true;
                onCompleted?.Invoke(false);
                ctx.Fail("Timeout waiting for test assertions.");
            }
        });

        ctx.OnCleanup(() => scheduled.Data.Cancelled = true);
    }

    public static void ResetBeforeTest(string label)
    {
        TestScenario.ClearAll();
        Ctx?.Cleanup();
        Ctx = new TestContext(label);
    }

    public static void StopTestRun()
    {
        TestScenario.ClearAll();
        Ctx?.Cleanup();
        queue.Clear();
        isRunningTest = false;
    }
}
