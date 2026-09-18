using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace WorkstationSearch
{
    internal sealed class SearchQuery
    {
        private readonly string[] terms;
        internal SearchQuery(string text) => terms = (text ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        internal bool IsEmpty => terms.Length == 0;
        internal bool FuzzyMatches(SpellingEntry entry) => entry != null && entry.FuzzyMatches(terms);
        internal List<T> RankMatches<T>(IEnumerable<T> source, Func<T, SpellingEntry> entry)
        {
            if (IsEmpty) return source.ToList();
            string phrase = string.Join(" ", terms);
            // Stable sorting keeps the game's order for equally relevant rows.
            // Prefab and tag matches remain eligible but do not earn title rank.
            return source.Select(row => {
                var item = entry(row);
                string name = item == null ? null : item.Name;
                return new {
                    Row = row,
                    TitleTerms = terms.Count(term => Contains(name, term)),
                    ExactTitle = name != null && CultureInfo.InvariantCulture.CompareInfo.Compare(name, phrase,
                        CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0
                };
            }).OrderByDescending(row => row.TitleTerms)
              .ThenByDescending(row => row.ExactTitle).Select(row => row.Row).ToList();
        }
        internal List<T> SelectMatches<T>(IEnumerable<T> source, Func<T, SpellingEntry> entry, out bool approximate)
        {
            var candidates = source.ToList();
            var matches = candidates.FindAll(row => {
                var item = entry(row);
                return item == null || item.DirectMatches(terms);
            });
            approximate = false;
            if (!IsEmpty && matches.Count == 0)
            {
                matches = candidates.FindAll(row => FuzzyMatches(entry(row)));
                approximate = matches.Count > 0;
            }
            return matches;
        }
        internal bool Matches(string displayName, string prefabName)
        {
            foreach (string term in terms)
                if (!Contains(displayName, term) && !Contains(prefabName, term)) return false;
            return true;
        }
        internal static bool Contains(string text, string term) => text != null &&
            CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, term, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
    }
}
