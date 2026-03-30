using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnHistory.Source.Helper;

public static class EnumerableExtensions
{
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();

        foreach (var item in source)
        {
            if (seen.Add(keySelector(item)))
                yield return item;
        }
    }

    public static string JoinToString<T>(this IEnumerable<T> source, Func<T, string> selector, string separator = ", ")
    {
        if (source == null)
            return string.Empty;

        return string.Join(separator, source.Select(selector));
    }

    public static string JoinToString<T>(this IEnumerable<T> source, string separator = ", ")
    {
        if (source == null)
            return string.Empty;

        return string.Join(separator, source);
    }
}