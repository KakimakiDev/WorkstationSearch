using System.Collections.Generic;

namespace WorkstationSearch
{
    // Retain immutable search text across UI row destruction/recreation.
    internal sealed class RowEntryCache
    {
        private readonly Dictionary<SpellingEntry, Dictionary<string, SpellingEntry>> entries =
            new Dictionary<SpellingEntry, Dictionary<string, SpellingEntry>>();
        private readonly SpellingEntry unknown = new SpellingEntry("", "");
        private CategoryOverrides overrides;
        private int count;

        internal SpellingEntry Get(SpellingEntry source, string title, CategoryOverrides currentOverrides)
        {
            if (!ReferenceEquals(overrides, currentOverrides) || count >= 16384)
            {
                entries.Clear();
                count = 0;
                overrides = currentOverrides;
            }
            if (source == null && string.IsNullOrWhiteSpace(title)) return null;
            source = source ?? unknown;
            title = title ?? "";
            if (!entries.TryGetValue(source, out var titles))
                entries[source] = titles = new Dictionary<string, SpellingEntry>();
            if (!titles.TryGetValue(title, out var entry))
            {
                entry = currentOverrides.Apply(source.WithDisplayName(title));
                titles[title] = entry;
                count++;
            }
            return entry;
        }
    }
}
