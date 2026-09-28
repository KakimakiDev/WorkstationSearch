using System;
using WorkstationSearch;

internal static class RowEntryCacheRegression
{
    internal static void Run(Action<string, bool> check)
    {
        var cache = new RowEntryCache();
        var overrides = new CategoryOverrides();
        var source = new SpellingEntry("Iron Sword", "SwordIron");
        var first = cache.Get(source, "<color=blue>Stormblade</color>", overrides);
        check("recreated UI rows reuse their search index", ReferenceEquals(first,
            cache.Get(source, "<color=blue>Stormblade</color>", overrides)));
        var renamed = cache.Get(source, "Flameblade", overrides);
        check("enchanted names retain separate search entries", renamed.Name == "Flameblade" && first.Name == "Stormblade");
        var updated = new CategoryOverrides();
        updated.Set("SwordIron", new CategoryOverride { Terms = new[] { "winter" } });
        var customized = cache.Get(source, "<color=blue>Stormblade</color>", updated);
        check("saved custom terms invalidate cached entries", !ReferenceEquals(first, customized) &&
            new SearchQuery("winter").SelectMatches(new[] { customized }, x => x, out _).Count == 1);
        var translated = new SpellingEntry("Eisenschwert", "SwordIron");
        check("changed catalogue entry refreshes name index", cache.Get(translated, "", updated).Name == "Eisenschwert");
        check("empty unknown row stays unindexed", cache.Get(null, "", updated) == null);
        int lookups = 0;
        var rows = new[] { first, renamed };
        var matches = new SearchQuery("").SelectMatches(rows, x => { lookups++; return x; }, out _);
        check("idle search does not access spelling entries", lookups == 0 && matches.Count == 2);
    }
}
