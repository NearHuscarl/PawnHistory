using PawnHistory.Source.Helper;
using System;
using System.Collections.Generic;
using PawnHistory.Source.DebugTools;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class TestManager
{
    public static readonly int DefaultTimeout = 5000;
    public static int Timeout = DefaultTimeout;
    internal static TestContext Ctx;
    internal static TestScenario Scenario = new();
    public static bool IsRunningTest => Scenario != TestScenario.Empty;
    private static readonly Queue<Action> Queue = new();
    private static bool isRunningTestCollection;

    public static bool EnableDebugMap = false;
    public static int ForcedDebugMapSize = Scenario.DefaultDebugMapSize;
    
    public static void EnqueueTest(string testId, Func<object> testAction, bool reuseMap = false, int? debugMapSize = null)
    {
        Queue.Enqueue(() =>
        {
            ForcedDebugMapSize = debugMapSize ?? Scenario.DefaultDebugMapSize;

            if (reuseMap)
            {
                ExecuteTestMethod(testId, testAction, _ => RunNext());
                return;
            }

            GameUtility.CreateTestGame(() => ExecuteTestMethod(testId, testAction, _ => RunNext()));
        });
    }

    public static void Run()
    {
        SetupBeforeAll();
        RunNext();
    }

    private static void RunNext()
    {
        if (Queue.Count == 0)
        {
            CleanupAfterAll();
            return;
        }

        var next = Queue.Dequeue();
        next();
    }

    private static void ExecuteTestMethod(string testId, Func<object> testAction, Action<bool> onCompleted = null)
    {
        SetupBeforeTest(testId);

        var ctx = Ctx;
        Action testCleanup = null;
        Log.Message("[PawnHistory] Starting test: " + testId);

        try
        {
            var res = testAction();
            if (res is Action cleanup)
                testCleanup = cleanup;
        }
        catch (Exception ex)
        {
            var failure = new TestExecutionFailure(testId, "Failed during test setup.");
            ctx.Fail(new TestException(failure, ex));
            CleanupAfterTest();
            onCompleted?.Invoke(false);
            return;
        }

        var start = Find.TickManager.TicksGame;
        var scheduled = TickDelayManager.Interval(1, a =>
        {
            if (ctx.PendingEventually == 0 && ctx.IsExpectedAssertionCountSatisfied)
            {
                if (ctx.TestFailures.Count == 0 && ctx.AssertionsPassed > 0)
                    ctx.ReportPass();
                a.Cancelled = true;
                try
                {
                    testCleanup?.Invoke(); /* user code, safeguard */
                }
                catch (Exception ex)
                {
                    Log.Error($"[PawnHistory] Failed during test cleanup for {testId}\n\n{ex}");
                }
                finally
                {
                    CleanupAfterTest();
                    onCompleted?.Invoke(ctx.TestFailures.Count == 0);
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
                    var failure = new TimeoutFailure(testId, ctx.GetTimeoutMessage());
                    ctx.Fail(new TestException(failure));
                    CleanupAfterTest();
                    onCompleted?.Invoke(false);
                }
            }
        });

        ctx.OnCleanup(() => scheduled.Data.Cancelled = true);
    }

    private static void SetupBeforeAll()
    {
        if (isRunningTestCollection)
            return;
        isRunningTestCollection = true; 
        TestReportManager.Reset();
        
        EnableDebugMap = true;
    }
    
    private static void CleanupAfterAll()
    {
        TestReportManager.PrintReport();
        TestReportManager.SaveReport();
        isRunningTestCollection = false;
        
        EnableDebugMap = false;
    }

    private static void SetupBeforeTest(string testId)
    {
        Timeout = DefaultTimeout;
        Ctx = new TestContext(testId);
        Scenario = new TestScenario();

        NearDebugSettings.NeverEverEverPause = true;
        Prefs.AutomaticPauseMode = AutomaticPauseMode.Never;
    }

    private static void CleanupAfterTest()
    {
        Scenario = TestScenario.Empty;
        NearDebugSettings.NeverEverEverPause = false;
        ForcedDebugMapSize = Scenario.DefaultDebugMapSize;
        TestReportManager.AddReportEntry(Ctx.CreateReportEntry());
        Ctx?.Cleanup();
    }

    public static void StopTestRun()
    {
        Queue.Clear();
        isRunningTestCollection = false;
    }
}
