using System;
using System.Collections.Generic;
using System.Linq;

namespace PawnHistory.Source.Helper;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<T> DistinctBy<TKey>(Func<T, TKey> keySelector)
        {
            var seen = new HashSet<TKey>();

            foreach (var item in source)
            {
                if (seen.Add(keySelector(item)))
                    yield return item;
            }
        }

        public string JoinToString(Func<T, string> selector, string separator = ", ")
        {
            if (source == null)
                return string.Empty;

            return string.Join(separator, source.Select(selector));
        }

        public string JoinToString(string separator = ", ")
        {
            if (source == null)
                return string.Empty;

            return string.Join(separator, source);
        }
    }
}