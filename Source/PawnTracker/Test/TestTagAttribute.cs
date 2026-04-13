using System;

namespace PawnHistory.Source.PawnTracker.Test;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestTagAttribute(string tag) : Attribute
{
    public string Tag { get; } = tag;
}