using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Profiling;

namespace PawnHistory.Source.Helper;

public static class ListExtensions
{
    public static float Median<T, TKey>(this IList<T> source, Func<T, TKey> keySelector) where TKey : IConvertible
    {
        if (source == null || source.Count == 0)
            throw new InvalidOperationException("Cannot compute median of empty list.");

        var values = source
            .Select(x => Convert.ToDouble(keySelector(x)))
            .OrderBy(x => x)
            .ToList();

        var count = values.Count;
        var mid = count / 2;

        if (count % 2 == 1)
            return (float)values[mid];

        return (float)((values[mid - 1] + values[mid]) / 2.0);
    }

    /// <summary>
    /// A less retarded way of accessing element via index, supports negative indices.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public static bool TryAt<T>(this IList<T> source, int index, out T value)
    {
        var resolvedIndex = index < 0 ? source.Count + index : index;

        if (resolvedIndex >= 0 && resolvedIndex < source.Count)
        {
            value = source[resolvedIndex];
            return true;
        }

        value = default;
        return false;
    }

    public static T At<T>(this IList<T> source, int index)
    {
        if (!source.TryAt(index, out var value))
            throw new IndexOutOfRangeException($"Index {index} is out of range for a list of {source.Count} elements.");

        return value;
    }

    public static IEnumerable<T> TakeLast<T>(this IList<T> source, int count)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (count <= 0)
            yield break;

        var start = Math.Max(source.Count - count, 0);

        for (var i = start; i < source.Count; i++)
            yield return source[i];
    }
}
