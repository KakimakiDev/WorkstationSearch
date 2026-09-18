using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkstationSearch
{
    internal sealed class FavoriteSet
    {
        private readonly HashSet<string> items = new HashSet<string>(StringComparer.Ordinal);
        internal FavoriteSet(string saved)
        {
            foreach (var token in (saved ?? "").Split(';'))
                if (token.Length > 0) items.Add(Uri.UnescapeDataString(token));
        }
        internal bool Contains(string key) => key != null && items.Contains(key);
        internal void Toggle(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!items.Remove(key)) items.Add(key);
        }
        internal string Save() => string.Join(";", items.OrderBy(x => x, StringComparer.Ordinal).Select(Uri.EscapeDataString));
        internal static List<T> First<T>(IEnumerable<T> source, Func<T, bool> favorite)
        {
            var first = new List<T>();
            var rest = new List<T>();
            foreach (var item in source) (favorite(item) ? first : rest).Add(item);
            first.AddRange(rest);
            return first;
        }
    }
}
