using System;
using System.Text;

namespace PawnHistory.Source.PawnTracker.Test;

internal class TestException(TestFailure failure, Exception exception = null) : Exception(failure.message, exception)
{
    public TestFailure Failure { get; } = failure;

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"[PawnHistory] [Failed] {Failure.label}: ");
        sb.Append($"{Failure.message}\n{StackTrace}");

        if (InnerException != null)
            sb.AppendLine("\n" + InnerException);

        return sb.ToString();
    }
}
