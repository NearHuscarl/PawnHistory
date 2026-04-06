using System;

namespace PawnHistory.Source.PawnTracker.Test;

internal class TestAssertionException(string message, Exception exception = null) : Exception(message, exception)
{
}
