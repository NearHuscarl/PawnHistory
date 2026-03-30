using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
