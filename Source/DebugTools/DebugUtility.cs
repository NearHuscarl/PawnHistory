using System;
using PawnHistory.Source.Helper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.DebugTools;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class DebugIgnoreAttribute : Attribute;

public static class DebugUtility
{
    public static string FormatDict(Dictionary<string, string> dict) => "[" + dict.Select(p => $"{p.Key}={p.Value}").JoinToString() + "]";

    public static string FormatSequence<T>(IEnumerable<T> values)
    {
        if (values == null)
            return "null";

        return "[" + values.Select(v => v?.ToString() ?? "null").JoinToString() + "]";
    }
    
    public static string Format(object obj)
    {
        if (obj == null) return "null";

        var type = obj.GetType();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        var parts = new List<string>();

        foreach (var prop in props)
        {
            if (prop.HasAttribute<DebugIgnoreAttribute>())
                continue;

            var value = prop.GetValue(obj);
            parts.Add($"{prop.Name}={FormatValue(value)}");
        }

        foreach (var field in fields)
        {
            if (field.HasAttribute<DebugIgnoreAttribute>())
                continue;

            var value = field.GetValue(obj);
            parts.Add($"{field.Name}={FormatValue(value)}");
        }

        return $"{type.Name}[{parts.JoinToString()}]";
    }

    private static string FormatValue(object value)
    {
        if (value == null) return "null";

        if (value is IEnumerable enumerable && value is not string)
        {
            var items = new List<string>();

            foreach (var item in enumerable)
                items.Add(item?.ToString() ?? "null");

            return $"[{items.JoinToString()}]";
        }

        var type = value.GetType();
        if (!type.IsPrimitive && type != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(type))
        {
            return $"({LangUtility.Truncate(value.ToString(), 100)})";
        }

        return LangUtility.Truncate(value.ToString(), 100);
    }
}
