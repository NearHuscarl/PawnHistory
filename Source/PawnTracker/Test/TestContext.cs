using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

internal sealed class TestContext(string testId)
{
    public readonly string TestId = testId;
    public readonly List<TestFailure> TestFailures = [];
    public int AssertionsPassed { get; private set; }
    public int PendingEventually;
    private int expectedAssertions = -1;
    private int registeredAssertions;
    private readonly TestReportEntry testReportEntry = new();

    public bool HasExpectedAssertions => expectedAssertions >= 0;
    public bool IsExpectedAssertionCountSatisfied => !HasExpectedAssertions || registeredAssertions == expectedAssertions;

    public void DeclareExpectedAssertions(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        expectedAssertions = count;

        if (registeredAssertions > count)
            Fail(new TestException(new TestExecutionFailure(TestId, $"Too many assertions already registered. Expected exactly {count} assertion(s), but {registeredAssertions} were already registered.")));
    }

    public bool TryRegisterAssertion()
    {
        registeredAssertions++;

        if (!HasExpectedAssertions)
            return true;

        if (registeredAssertions > expectedAssertions)
        {
            Fail(new TestException(new TestExecutionFailure(TestId, $"Too many assertions registered. Expected exactly {expectedAssertions} assertion(s).")));
            return false;
        }
        return true;
    }

    public string GetTimeoutMessage()
    {
        var parts = new List<string>();

        if (!IsExpectedAssertionCountSatisfied)
            parts.Add($"expected {expectedAssertions} assertion(s), but {registeredAssertions} were registered");

        if (PendingEventually > 0)
            parts.Add($"{PendingEventually} assertion execution(s) still pending");

        if (parts.Count == 0)
            parts.Add($"{TestId} failed after waiting for {TestManager.Timeout} ticks.");

        return $"Timeout waiting for test assertions. {parts.JoinToString("; ")}.";
    }

    public void Pass()
    {
        AssertionsPassed++;
        if (PendingEventually > 0)
            PendingEventually--;
    }

    public void Fail(Exception ex, AssertionSource source = null)
    {
        if (ex is not TestException te)
        {
            te = new TestException(new TestFailure(TestId, "Unknown error happened during a test run"), ex, source);
        }

        te.AssertionSource = source;
        switch (te.Failure)
        {
            case TimeoutFailure:
            {
                if (PendingEventually <= 0)
                {
                    TestFailures.Add(te.Failure);
                    break;
                }

                while (PendingEventually > 0)
                {
                    TestFailures.Add(te.Failure);
                    PendingEventually--;
                }
                break;
            }
            case TestExecutionFailure:
                TestFailures.Add(te.Failure);
                PendingEventually = 0;
                break;
            default:
                TestFailures.Add(te.Failure);
                PendingEventually--;
                break;
        }
        
        Log.Error(te.ToString());
    }

    public void ReportPass()
    {
        Log.Message(Palette.Green($"[PawnHistory] [Passed] {TestId}: {AssertionsPassed} passed"));
        Messages.Message("[Passed] " + TestId, MessageTypeDefOf.PositiveEvent);
    }

    public TestReportEntry CreateReportEntry()
    {
        testReportEntry.TestId = TestId;
        testReportEntry.AssertionsPassed = AssertionsPassed;
        testReportEntry.TestFailures = TestFailures;
        testReportEntry.timestampEnded = Stopwatch.GetTimestamp();
        return testReportEntry;
    }

    private readonly List<Action> cleanupCallbacks = [];

    public void OnCleanup(Action callback)
    {
        cleanupCallbacks.Add(callback);
    }

    public void Cleanup()
    {
        cleanupCallbacks.ForEach(c => c());
        cleanupCallbacks.Clear();
    }
}
