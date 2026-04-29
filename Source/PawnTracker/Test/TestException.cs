using System;
using System.Text;

namespace PawnHistory.Source.PawnTracker.Test;

internal class TestException : Exception
{
    public TestException(TestFailure failure, Exception exception = null, AssertionSource source = null) : base(failure.message, exception)
    {
        Failure = failure;
        AssertionSource = source;
    }

    public TestFailure Failure { get; }
    public AssertionSource AssertionSource { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"[PawnHistory] [Failed] {Failure.testId}: ");
        sb.Append(Failure);

        if (AssertionSource != null)
            sb.AppendLine("\n" + AssertionSource);
        if (InnerException != null)
            sb.AppendLine("\n" + InnerException);

        sb.AppendLine();
        sb.Append(StackTrace);

        return sb.ToString();
    }
}
