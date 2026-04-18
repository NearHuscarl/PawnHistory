using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

internal sealed class TestContext(string testId)
{
    public readonly string TestId = testId;
    public int AssertionsPassed;
    public readonly List<TestFailure> TestFailures = [];
    public int PendingEventually;
    public static string Red(object obj) => obj.ToString().ApplyTag(TagType.Red).Resolve();
    public static string Green(object obj) => obj.ToString().Colorize(ColoredText.FactionColor_Ally);
    private readonly TestReportEntry testReportEntry = new();

    public void Pass()
    {
        AssertionsPassed++;
        PendingEventually--;
    }

    public void Fail(Exception ex)
    {
        if (ex is not TestException te)
        {
            te = new TestException(new TestFailure(TestId, "Unknown error happened during a test run"), ex);
        }
        
        switch (te.Failure)
        {
            case TimeoutFailure:
            {
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
        Log.Message(Green($"[PawnHistory] [Passed] {TestId}: {AssertionsPassed} passed"));
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
