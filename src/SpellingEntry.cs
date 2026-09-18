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
        internal readonly string[] Tags;
        private readonly string sourceName;
        private readonly string[] words;
        internal SpellingEntry(string name, string prefab, string[] tags = null, string originalName = null)
        {
            Name = name;
            Prefab = prefab;
            Tags = tags ?? new string[0];
            sourceName = originalName;
            words = Regex.Matches(Normalize((name ?? "") + " " + (sourceName ?? "")), @"[\p{L}\p{N}]+").Cast<Match>().Select(m => m.Value)
                .Concat(Tags.Select(Normalize)).Distinct().ToArray();
        }
        internal SpellingEntry WithDisplayName(string title)
        {
            title = Regex.Replace(title ?? "", "<[^>]*>", "").Trim();
            return title.Length == 0 || title == Name ? this : new SpellingEntry(title, Prefab, Tags, sourceName ?? Name);
        }
        internal bool MatchesTerm(string term) => SearchQuery.Contains(Name, term) || SearchQuery.Contains(Prefab, term) ||
            SearchQuery.Contains(sourceName, term) ||
            Tags.Any(tag => CultureInfo.InvariantCulture.CompareInfo.Compare(tag, term,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0);
        internal bool DirectMatches(string[] terms) => terms.All(MatchesTerm);
        internal bool FuzzyMatches(string[] terms)
        {
            foreach (var term in terms)
            {
                if (MatchesTerm(term)) continue;
                string normalized = Normalize(term);
                if (normalized.Length < 4 || normalized.Any(c => !char.IsLetterOrDigit(c)) ||
                    !words.Any(word => OneEdit(normalized, word))) return false;
            }
            return true;
        }
        private static string Normalize(string value)
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
