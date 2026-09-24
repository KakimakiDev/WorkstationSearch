using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkstationSearch
{
    internal sealed class CategoryOverride
    {
        // Names, rather than enum numbers, remain stable across plugin updates.
        // Unknown names are retained so a newer category survives a downgrade.
        internal readonly HashSet<string> Added = new HashSet<string>(StringComparer.Ordinal);
        internal readonly HashSet<string> Disabled = new HashSet<string>(StringComparer.Ordinal);
        internal string[] Terms = new string[0];
        internal bool IsEmpty => Added.Count == 0 && Disabled.Count == 0 && Terms.Length == 0;
        internal CategoryOverride Copy()
        {
            var copy = new CategoryOverride { Terms = (string[])Terms.Clone() };
            copy.Added.UnionWith(Added);
            copy.Disabled.UnionWith(Disabled);
            return copy;
        }
        internal ItemCategory[] Apply(IEnumerable<ItemCategory> detected)
        {
            var added = Enum.GetValues(typeof(ItemCategory)).Cast<ItemCategory>().Where(c => Added.Contains(c.ToString()));
            return CategoryHierarchy.Expand(detected.Concat(added)).Where(c => !Disabled.Contains(c.ToString())).ToArray();
        }
        internal void Toggle(ItemCategory category, ItemCategory[] detected)
        {
            string key = category.ToString();
            if (Apply(detected).Contains(category))
            {
                Added.Remove(key);
                Disabled.Add(key);
            }
            else
            {
                Disabled.Remove(key);
                if (!Apply(detected).Contains(category)) Added.Add(key);
            }
        }
        internal static string[] ParseTerms(string text) => (text ?? "").Split(
            new[] { ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    internal sealed class CategoryOverrides
    {
        private readonly Dictionary<string, CategoryOverride> items = new Dictionary<string, CategoryOverride>(StringComparer.Ordinal);
        internal CategoryOverride Get(string prefab) => items.TryGetValue(prefab, out var value) ? value.Copy() : new CategoryOverride();
        internal void Set(string prefab, CategoryOverride value)
        {
            if (string.IsNullOrEmpty(prefab)) throw new ArgumentException("Missing item prefab ID.", nameof(prefab));
            if (value.IsEmpty) items.Remove(prefab);
            else items[prefab] = value.Copy();
        }
        internal SpellingEntry Apply(SpellingEntry entry)
        {
            if (entry == null || !items.TryGetValue(entry.Prefab, out var value)) return entry;
            return entry.WithSearchCategories(value.Apply(entry.Categories), value.Terms);
        }
        // No reconciliation with installed items: absent mods must retain their overrides.
        internal string Save() => "1:" + string.Join(";", items.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p =>
            Encode(p.Key) + "|" + Join(p.Value.Added.OrderBy(x => x, StringComparer.Ordinal)) + "|" +
            Join(p.Value.Disabled.OrderBy(x => x, StringComparer.Ordinal)) + "|" + Join(p.Value.Terms)));
        internal static CategoryOverrides Load(string text)
        {
            var result = new CategoryOverrides();
            if (string.IsNullOrEmpty(text)) return result;
            if (!text.StartsWith("1:", StringComparison.Ordinal)) throw new FormatException("Unsupported category override format.");
            foreach (string record in text.Substring(2).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] fields = record.Split('|');
                if (fields.Length != 4) throw new FormatException("Invalid category override record.");
                var value = new CategoryOverride { Terms = Split(fields[3]) };
                value.Added.UnionWith(Split(fields[1]));
                value.Disabled.UnionWith(Split(fields[2]));
                result.Set(Uri.UnescapeDataString(fields[0]), value);
            }
            return result;
        }
        private static string Encode(string value) => Uri.EscapeDataString(value);
        private static string Join(IEnumerable<string> values) => string.Join(",", values.Select(Encode));
        private static string[] Split(string text) => text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.UnescapeDataString).ToArray();
    }
}
