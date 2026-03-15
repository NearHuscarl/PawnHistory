using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private static readonly DumpConfig defaultConfig = new();

    public static string Dump(object obj, DumpConfig config = null)
    {
        var visited = new HashSet<object>();
        return DumpInternal(obj, 0, visited, config ?? defaultConfig);
    }

    private static string DumpInternal(object obj, int indent, HashSet<object> visited, DumpConfig config)
    {
        if (obj == null) return "null";

        var type = obj.GetType();
        if (type.IsPrimitive || obj is string || obj is decimal)
            return obj.ToString().Replace("\r", " ").Replace("\n", " ");

        if (visited.Contains(obj))
            return "<circular>";

        if (config.LeafTypes.Contains(type))
            return obj.ToString();

        var indSize = config.IndentSize;

        if (indent / indSize >= config.MaxDepth)
            return obj.ToString();

        visited.Add(obj);

        var sb = new StringBuilder();
        var ind = new string(' ', indent);
        var ind2 = new string(' ', indent + indSize);
        var ind3 = new string(' ', indent + indSize * 2);

        sb.AppendLine($"{ind}{type.Name}");
        sb.AppendLine($"{ind}{{");

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            object value;

            try
            {
                value = prop.GetValue(obj);
            }
            catch
            {
                value = "<error>";
            }

            sb.Append($"{ind2}{prop.Name}: ");

            if (value is IEnumerable enumerable && value is not string)
            {
                sb.AppendLine("[");
                var count = 0;

                foreach (var item in enumerable)
                {
                    if (count++ >= config.MaxCollectionItems)
                    {
                        sb.AppendLine($"{ind3}...");
                        break;
                    }
                    sb.AppendLine($"{ind3}{DumpInternal(item, indent + (indSize * 2), visited, config)},");
                }
                sb.AppendLine($"{ind2}],");
            }
            else
            {
                var dumped = DumpInternal(value, indent + indSize, visited, config);
                if (dumped.Contains('\n'))
                    sb.Append(dumped);
                else
                    sb.AppendLine($"{dumped},");
            }
        }

        sb.AppendLine($"{ind}}}");
        visited.Remove(obj);

        return sb.ToString();
    }
}
