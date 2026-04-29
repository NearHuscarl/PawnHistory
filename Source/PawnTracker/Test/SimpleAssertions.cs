using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PawnHistory.Source.DebugTools;

namespace PawnHistory.Source.PawnTracker.Test;

public sealed class SimpleAssertions<T>
{
    private readonly T actual;
    private readonly IEnumerable<T> actualMany;
    private bool negate;
    
    public SimpleAssertions(T actual)
    {
        this.actual = actual;
    }
    public SimpleAssertions(IEnumerable<T> actual)
    {
        this.actualMany = actual;
    }
    
    public SimpleAssertions<T> Not()
    {
        negate = !negate;
        return this;
    }
    
    public void Equal(T expected, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var source = new AssertionSource(memberName, filePath, lineNumber);
        AssertionRunner.RunAssertion(() =>
        {
            var passed = EqualityComparer<T>.Default.Equals(expected, actual);
            if (negate ? !passed : passed)
                return;

            Fail(negate ? "Expected values not to be equal." : "Expected values to be equal.", expected, actual);
        }, source);
    }

    public void NotEqual(T expected, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) =>
        Not().Equal(expected, memberName, filePath, lineNumber);

    public void Same(T expected, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var source = new AssertionSource(memberName, filePath, lineNumber);
        AssertionRunner.RunAssertion(() =>
        {
            var passed = ReferenceEquals(expected, actual);
            if (negate ? !passed : passed)
                return;

            var message = negate
                ? "Expected references not to be the same."
                : "Expected references to be the same.";
            Fail(message, expected, actual);
        }, source);
    }

    public void NotSame(T expected, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) =>
        Not().Same(expected, memberName, filePath, lineNumber);

    public void SequenceEqual(IEnumerable<T> expected, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var source = new AssertionSource(memberName, filePath, lineNumber);
        AssertionRunner.RunAssertion(() =>
        {
            if (expected == null && actual == null)
                return;
            
            var passed = expected != null && actualMany != null && expected.SequenceEqual(actualMany);
            if (negate ? !passed : passed)
                return;

            var message = negate
                ? "Expected sequences not to be equal in the same order."
                : "Expected sequences to be equal in the same order.";

            Fail(message, DebugUtility.FormatSequence(expected), DebugUtility.FormatSequence(actualMany));
        }, source);
    }
    
    public void Contain(T expected, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var source = new AssertionSource(memberName, filePath, lineNumber);
        AssertionRunner.RunAssertion(() =>
        {
            var passed = false;

            if (actual is string actualString && expected is string expectedString)
            {
                passed = actualString.Contains(expectedString);
            }
            else if (actualMany != null)
            {
                passed = actualMany.Contains(expected);
            }
            else
            {
                Fail($"Expected actual value to be a string or {nameof(IEnumerable<>)}.", expected, actualMany);
            }

            if (negate ? !passed : passed)
                return;

            var actualObj = (object)actual ?? actualMany;
            var message = negate
                ? "Expected collection not to contain item."
                : "Expected collection to contain item.";
            Fail(message, expected, actualObj);
        }, source);
    }
    
    public void LessThan(T expected, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var source = new AssertionSource(memberName, filePath, lineNumber);
        AssertionRunner.RunAssertion(() =>
        {
            var passed = Comparer<T>.Default.Compare(actual, expected) < 0;

            if (negate ? !passed : passed)
                return;

            var message = negate
                ? "Expected value not to be less than expected."
                : "Expected value to be less than expected.";
            Fail(message, expected, actual);
        }, source);
    }

    public void GreaterThan(T expected, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var source = new AssertionSource(memberName, filePath, lineNumber);
        AssertionRunner.RunAssertion(() =>
        {
            var passed = Comparer<T>.Default.Compare(actual, expected) > 0;

            if (negate ? !passed : passed)
                return;

            var message = negate
                ? "Expected value not to be greater than expected."
                : "Expected value to be greater than expected.";
            Fail(message, expected, actual);
        }, source);
    }
    
    public void True([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var source = new AssertionSource(memberName, filePath, lineNumber);
        AssertionRunner.RunAssertion(() =>
        {
            var passed = actual is true;
            if (negate ? !passed : passed)
                return;

            var message = negate
                ? "Expected value to be false."
                : "Expected value to be true.";
            
            Fail(message, "true", "false");
        }, source);
    }

    public void False([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) =>
        Not().True(memberName, filePath, lineNumber);

    public void Null([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var source = new AssertionSource(memberName, filePath, lineNumber);
        AssertionRunner.RunAssertion(() =>
        {
            var passed = actual == null;
            if (negate ? !passed : passed)
                return;

            var message = negate
                ? "Expected value to be not null."
                : "Expected value to be null.";
            
            Fail(message, "null", actual!.ToString());
        }, source);
    }

    public void NotNull([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) =>
        Not().Null(memberName, filePath, lineNumber);

    private void Fail(string message, object expected, object actual)
    {
        var ctx = TestManager.Ctx;

        throw new TestException(new TestAssertionFailure(
            ctx.TestId,
            message,
            expected?.ToString() ?? "null",
            actual?.ToString() ?? "null",
            false
        ));
    }
}