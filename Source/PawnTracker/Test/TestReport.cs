using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestReport : IExposable
{
    public List<TestReportEntry> Entries = [];

    public void ExposeData()
    {
        Scribe_Collections.Look(ref Entries, "Entries", LookMode.Deep);
    }
}

public class TestReportEntry : IExposable
{
    public string Label;
    public int AssertionsPassed;
    public List<TestFailureBase> AssertionsFailures;

    public TestReportEntry() { }

    public TestReportEntry(string label, int assertionsPassed, List<TestFailureBase> assertionsFailures)
    {
        Label = label;
        AssertionsPassed = assertionsPassed;
        AssertionsFailures = assertionsFailures;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref Label, "Label");
        Scribe_Values.Look(ref AssertionsPassed, "AssertionsPassed");
        Scribe_Collections.Look(ref AssertionsFailures, "AssertionsFailed");
    }
}

public class TestFailureBase : IExposable
{
    public string label;
    public string message;
    public int ticksAbs = GenTicks.TicksAbs;

    public TestFailureBase() { }
    
    public TestFailureBase(string label, string message)
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

public class TestFailure : TestFailureBase
{
    public string expected;
    public string actual;
    public bool isNegated;
    public Dictionary<string, string> testParams;

    public TestFailure(string label, string message, string expected, string actual, bool isNegated, Dictionary<string, string> testParams) : base(label, message)
    {
        this.expected = expected;
        this.actual = actual;
        this.isNegated = isNegated;
        this.testParams = testParams;
    }
    
    public TestFailure() { }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref expected, "expected");
        Scribe_Values.Look(ref actual, "actual");
        Scribe_Values.Look(ref isNegated, "isNegated");
        Scribe_Collections.Look(ref testParams, "testParams");
    }
}

public class TestFailureTimeout : TestFailureBase
{
    public TestFailureTimeout(string label, string message) : base(label, message) { }
    public TestFailureTimeout() { }
}
