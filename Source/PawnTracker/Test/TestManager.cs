using PawnHistory.Source.Helper;
using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class TestManager
{
    public static int Timeout = 5000;
    internal static TestContext Ctx;
    private static readonly Queue<Action> Queue = new();
    private static bool isRunningTest;
    private static AutomaticPauseMode? curPauseMode = null;

    public static void EnqueueTest(Func<object> testAction, string label)
    {
        Queue.Enqueue(() =>
        {
            GameUtility.CreateTestGame(() =>
            {
                ExecuteTestMethod(label, testAction, _ => RunNext());
            });
        });
    }

    public static void Run()
    {
        if (isRunningTest) return;
        isRunningTest = true; 
        TestReportManager.Reset();
        RunNext();
    }

    private static void RunNext()
    {
        if (Queue.Count == 0)
        {
            isRunningTest = false;
            Log.Message("[PawnHistory] All tests finished.");
            TestReportManager.SaveReport();
            return;
        }

        var next = Queue.Dequeue();
        next();
    }

    public static void ExecuteTestMethod(string label, Func<object> testAction, Action<bool> onCompleted = null)
    {
        SetupBeforeTest(label);

        var ctx = Ctx;
        Action testCleanup = null;
        Log.Message("[PawnHistory] Starting test: " + label);

        try
        {
            var res = testAction();
            if (res is Action cleanup)
                testCleanup = cleanup;
        }
        catch (Exception ex)
        {
            Log.Error($"[PawnHistory] Failed during test setup for {label}\n\n{ex}");
            CleanupAfterTest();
            onCompleted?.Invoke(false);
            return;
        }

        var start = Find.TickManager.TicksGame;
        var scheduled = TickDelayManager.Interval(1, (a) =>
        {
            if (ctx.PendingEventually == 0)
            {
                if (ctx.AssertionsFailed.Count == 0 && ctx.AssertionsPassed > 0)
                    ctx.ReportPass();
                a.Cancelled = true;
                try
                {
                    testCleanup?.Invoke(); /* user code, safeguard */
                }
                catch (Exception ex)
                {
                    Log.Error($"[PawnHistory] Failed during test cleanup for {label}\n\n{ex}");
                }
                finally
                {
                    CleanupAfterTest();
                    onCompleted?.Invoke(ctx.AssertionsFailed.Count == 0);
                }
                return;
            }

            if (Find.TickManager.TicksGame - start > Timeout)
            {
                a.Cancelled = true;
                try
                {
                    testCleanup?.Invoke();
                }
                finally
                {
                    var timeoutFailure = new TestFailureTimeout(label, $"Timeout waiting for test assertions of {label}.");
                    ctx.Fail(new TestAssertionException(timeoutFailure));
                    CleanupAfterTest();
                    onCompleted?.Invoke(false);
                }
            }
        });

        ctx.OnCleanup(() => scheduled.Data.Cancelled = true);
    }

    private static void SetupBeforeTest(string label)
    {
        Ctx = new TestContext(label);

        curPauseMode = Prefs.AutomaticPauseMode;
        Prefs.AutomaticPauseMode = AutomaticPauseMode.Never;
    }

    private static void CleanupAfterTest()
    {
        if (curPauseMode.HasValue)
        {
            Prefs.AutomaticPauseMode = curPauseMode.Value;
            curPauseMode = null;
        }
        TestScenario.ClearAll(); 
        TestReportManager.AddReportEntry(Ctx.CreateReportEntry());
        Ctx?.Cleanup();
    }

    public static void StopTestRun()
    {
        Queue.Clear();
        isRunningTest = false;
    }
}
