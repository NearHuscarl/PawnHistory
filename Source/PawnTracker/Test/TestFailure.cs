using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestFailure : IExposable
{
    public string label;
    public string message;
    public long ticksAbs = Stopwatch.GetTimestamp();

    public TestFailure() { }
    
    public TestFailure(string label, string message)
    {
        this.label = label;
        this.message = message;
    }

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref label, "Label");
        Scribe_Values.Look(ref message, "message");
        Scribe_Values.Look(ref ticksAbs, "ticksAbs");
    }
}

public class TestAssertionFailure : TestFailure
{
    public string expected;
    public string actual;
    public bool isNegated;
    public Dictionary<string, string> testParams;

    public TestAssertionFailure(string label, string message, string expected, string actual, bool isNegated, Dictionary<string, string> testParams) : base(label, message)
    {
        this.expected = expected;
        this.actual = actual;
        this.isNegated = isNegated;
        this.testParams = testParams;
    }
    
    public TestAssertionFailure() { }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref expected, "expected");
        Scribe_Values.Look(ref actual, "actual");
        Scribe_Values.Look(ref isNegated, "isNegated");
        Scribe_Collections.Look(ref testParams, "testParams");
    }
}

public class TimeoutFailure : TestFailure
{
    public TimeoutFailure(string label, string message) : base(label, message) { }
    public TimeoutFailure() { }
}

public class TestExecutionFailure : TestFailure
{
    public TestExecutionFailure(string label, string message) : base(label, message) { }
    public TestExecutionFailure() { }
}
