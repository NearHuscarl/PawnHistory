using System.Diagnostics;

namespace PawnHistory.Source.PawnTracker.Test;

internal sealed class AssertionSource(string memberName, string filePath, int lineNumber, int skipFrames = 1)
{
    public string MemberName { get; } = memberName;
    public string FilePath { get; } = filePath;
    public int LineNumber { get; } = lineNumber;
    public StackTrace StackTrace { get; } = new(skipFrames, true);

    public override string ToString() => $"{FilePath}:{LineNumber} in {MemberName}\n{StackTrace}";
}