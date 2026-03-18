using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace PawnHistory.Source.DebugTools;

public class DumpConfig
{
    public int IndentSize { get; set; } = 2;
    public int MaxDepth { get; set; } = 3;
    public int MaxCollectionItems { get; set; } = 10;

    public HashSet<Type> LeafTypes { get; set; } = [
        typeof(Pawn),
        typeof(Map),
        typeof(Thing),
        typeof(Hediff),
        typeof(Faction),
        ];
}

public static class DebugUtility
{
    public static string Format(object obj)
    {
        if (obj == null) return "null";

        var type = obj.GetType();
        var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        var parts = new List<string>();

        foreach (var prop in props)
        {
            var value = prop.GetValue(obj);
            parts.Add($"{prop.Name}={FormatValue(value)}");
        }

        return $"{type.Name}[{string.Join(", ", parts)}]";
    }

    private static string FormatValue(object value)
    {
        if (value == null) return "null";

        if (value is IEnumerable enumerable && value is not string)
        {
            var items = new List<string>();

            foreach (var item in enumerable)
                items.Add(item?.ToString() ?? "null");

            return $"[{string.Join(", ", items)}]";
        }

        return value.ToString();
    }
}
