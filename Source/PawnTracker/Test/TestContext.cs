using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

internal sealed class TestContext(string name)
{
    public string Name = name;
    public int AssertionsPassed;
    public readonly List<TestFailureBase> AssertionsFailed = [];
    public int PendingEventually;
    public static string Red(object obj) => obj.ToString().ApplyTag(TagType.Red).Resolve();
    public static string Green(object obj) => obj.ToString().Colorize(ColoredText.FactionColor_Ally);

    public void Pass()
    {
        AssertionsPassed++;
        PendingEventually--;
    }

    public void Fail(Exception ex)
    {
        if (ex is not TestAssertionException tae)
        {
            tae = new TestAssertionException(new TestFailureBase(Name, "Unknown error happened during a test run"), ex);
        }
        
        if (tae.Failure is TestFailureTimeout)
        {
            while (PendingEventually > 0)
            {
                AssertionsFailed.Add(tae.Failure);
                PendingEventually--;
            }
        }
        else
        { 
            AssertionsFailed.Add(tae.Failure);
            PendingEventually--;
        }
        
        Log.Error(tae.ToString());
    }

    public void ReportPass()
    {
        Log.Message(Green($"[PawnHistory] [Passed] {Name}: {AssertionsPassed} passed"));
        Messages.Message("[Passed] " + Name, MessageTypeDefOf.PositiveEvent);
    }

    public TestReportEntry CreateReportEntry()
    {
        return new TestReportEntry(Name, AssertionsPassed, AssertionsFailed);
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
