using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WorkstationSearch
{
    internal sealed class SpellingEntry
    {
        internal readonly string Name;
        internal readonly string Prefab;
        internal readonly ItemCategory[] Categories;
        private readonly string sourceName;
        private readonly string[] words;
        internal SpellingEntry(string name, string prefab, ItemCategory[] categories = null, string originalName = null)
        {
            Name = name;
            Prefab = prefab;
            Categories = categories ?? new ItemCategory[0];
            sourceName = originalName;
            words = Regex.Matches(Normalize((name ?? "") + " " + (sourceName ?? "")), @"[\p{L}\p{N}]+").Cast<Match>().Select(m => m.Value)
                .Distinct().ToArray();
        }
        internal SpellingEntry WithDisplayName(string title)
        {
            title = Regex.Replace(title ?? "", "<[^>]*>", "").Trim();
            return title.Length == 0 || title == Name ? this : new SpellingEntry(title, Prefab, Categories, sourceName ?? Name);
        }
        internal bool MatchesTerm(SearchTerm term) => SearchQuery.Contains(Name, term.Text) || SearchQuery.Contains(Prefab, term.Text) ||
            SearchQuery.Contains(sourceName, term.Text) ||
            (term.Category.HasValue && Categories.Contains(term.Category.Value));
        internal bool DirectMatches(SearchTerm[] terms) => terms.All(MatchesTerm);
        internal bool FuzzyMatches(SearchTerm[] terms)
        {
            foreach (var term in terms)
            {
                if (MatchesTerm(term)) continue;
                string normalized = term.Normalized;
                if (normalized.Length < 4 || normalized.Any(c => !char.IsLetterOrDigit(c))) return false;
                if (!words.Any(word => OneEdit(normalized, word)) &&
                    !term.ApproximateCategories.Any(category => Categories.Contains(category))) return false;
            }
            return true;
        }
        internal static string Normalize(string value)
        {
            var result = new StringBuilder();
            foreach (char c in value.Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) result.Append(char.ToLowerInvariant(c));
            return result.ToString().Normalize(NormalizationForm.FormC);
        }
        // At most one insertion, deletion, substitution, or adjacent swap.
        internal static bool OneEdit(string a, string b)
        {
            if (Math.Abs(a.Length - b.Length) > 1) return false;
            int i = 0;
            while (i < a.Length && i < b.Length && a[i] == b[i]) i++;
            if (i == Math.Min(a.Length, b.Length)) return true;
            if (a.Length != b.Length)
                return a.Length > b.Length ? a.Substring(i + 1) == b.Substring(i) : a.Substring(i) == b.Substring(i + 1);
            if (a.Substring(i + 1) == b.Substring(i + 1)) return true;
            return i + 1 < a.Length && a[i] == b[i + 1] && a[i + 1] == b[i] && a.Substring(i + 2) == b.Substring(i + 2);
        }
    }
}
