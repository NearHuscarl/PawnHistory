using System;

namespace PawnHistory.Source.PawnTracker.Test;

[AttributeUsage(AttributeTargets.Method)]
public class DebugMapSizeAttribute(int size) : Attribute
{
    public int Size { get; } = size;
}
