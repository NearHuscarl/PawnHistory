using System.Diagnostics;
using System.Text;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestFailure : IExposable
{
    public string testId;
    public string message;
    public string stackTrace;
    public long ticksAbs = Stopwatch.GetTimestamp();

    public TestFailure() { }
    
    public TestFailure(string testId, string message)
    {
        this.testId = testId;
        this.message = message;
    }

    public override string ToString() => $"{testId} failed: {message}";

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref testId, "testId");
        Scribe_Values.Look(ref message, "message");
        Scribe_Values.Look(ref stackTrace, "stackTrace");
        Scribe_Values.Look(ref ticksAbs, "ticksAbs");
    }
}

public class TestAssertionFailure : TestFailure
{
    public string expected;
    public string actual;
    public bool isNegated;

    public TestAssertionFailure(string testId, string message, string expected, string actual, bool isNegated) : base(testId, message)
    {
        this.expected = expected;
        this.actual = actual;
        this.isNegated = isNegated;
    }
    
    public TestAssertionFailure() { }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine(message);
        sb.AppendLine("Expected:");
        sb.AppendLine(expected);
        sb.AppendLine("Actual:");
        sb.AppendLine(actual);
        
        return sb.ToString();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref expected, "expected");
        Scribe_Values.Look(ref actual, "actual");
        Scribe_Values.Look(ref isNegated, "isNegated");
    }
}

public class TimeoutFailure : TestFailure
{
    public TimeoutFailure(string testId, string message) : base(testId, message) { }
    public TimeoutFailure() { }
}

public class TestExecutionFailure : TestFailure
{
    public TestExecutionFailure(string testId, string message) : base(testId, message) { }
    public TestExecutionFailure() { }
}
