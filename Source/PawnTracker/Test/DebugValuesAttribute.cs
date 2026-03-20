using System;

namespace PawnHistory.Source.PawnTracker.Test;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
public class DebugValuesAttribute(params int[] values) : Attribute
{
    public int[] Values { get; } = values;
}