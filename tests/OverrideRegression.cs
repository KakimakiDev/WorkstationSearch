using System;
using System.Linq;
using WorkstationSearch;

internal static class OverrideRegression
{
    internal static void Run(Action<string, bool> check)
    {
        var store = new CategoryOverrides();
        var item = new SpellingEntry("Modded Relic", "ExampleMod_Relic", new[] { ItemCategory.Material });
        var edit = store.Get(item.Prefab);
        edit.Added.Add("Legs");
        edit.Disabled.Add("Material");
        edit.Terms = CategoryOverride.ParseTerms("dragon, sailing dragon\tWinter");
        store.Set(item.Prefab, edit);
        var applied = store.Apply(item);
        check("override adds category and parent", applied.Categories.Contains(ItemCategory.Legs) && applied.Categories.Contains(ItemCategory.Armour));
        check("override disables detected category", !applied.Categories.Contains(ItemCategory.Material));
        check("override enables synonyms", Match(applied, "boots") && Match(applied, "armor"));
        check("disabled aliases no longer match", !Match(applied, "resource"));
        check("custom words match alongside category", Match(applied, "dragon boots"));
        check("custom words case insensitive", Match(applied, "SAILING"));
        check("custom words are whole words", !Match(applied, "sail"));
        check("custom word typo fallback", new SearchQuery("sialing").FuzzyMatches(applied));
        check("custom words deduplicated", edit.Terms.Length == 3);
        check("automatic catalogue unchanged", item.Categories.SequenceEqual(new[] { ItemCategory.Material }) && !Match(item, "dragon"));
        var title = applied.WithDisplayName("Relic: Karve #1");
        check("custom display names retain overrides", Match(title, "karve dragon boots"));
        check("original name retained with overrides", Match(title, "modded dragon"));
        check("overrides after display names retain source name", Match(store.Apply(item.WithDisplayName("Relic: Karve #1")), "modded dragon"));
        check("override data isolated from draft mutation", !store.Get(item.Prefab).Added.Contains("Food"));
        edit.Added.Add("Food");
        check("saved copy isolated from draft mutation", !store.Apply(item).Categories.Contains(ItemCategory.Food));
        var cancelled = store.Get(item.Prefab);
        cancelled.Disabled.Clear();
        check("cancel leaves saved override unchanged", !store.Apply(item).Categories.Contains(ItemCategory.Material));
        var loaded = CategoryOverrides.Load(store.Save());
        check("reload preserves effective search", Match(loaded.Apply(item), "dragon boots") && !Match(loaded.Apply(item), "resource"));
        var other = new SpellingEntry("Modded Relic", "OtherMod_Relic", new[] { ItemCategory.Tool });
        check("same title different prefab unaffected", !Match(loaded.Apply(other), "dragon") && Match(loaded.Apply(other), "tools"));
        string absentData = loaded.Save();
        // The absent mod has no live item. A different item can still be edited and saved.
        var otherEdit = new CategoryOverride { Terms = new[] { "workshop" } };
        loaded.Set(other.Prefab, otherEdit);
        loaded = CategoryOverrides.Load(loaded.Save());
        check("removed mod override remains after unrelated edits and restart", Match(loaded.Apply(item), "dragon boots"));
        check("readded prefab gets saved override with new automatic metadata", Match(loaded.Apply(
            new SpellingEntry("Renamed Relic", item.Prefab, new[] { ItemCategory.Fire })), "fire dragon boots"));
        loaded.Set(other.Prefab, new CategoryOverride());
        check("reset only deletes selected prefab", loaded.Save() == absentData);
        loaded.Set(item.Prefab, new CategoryOverride());
        check("reset restores automatic categories and removes custom words", Match(loaded.Apply(item), "material") && !Match(loaded.Apply(item), "dragon"));
        check("empty overrides round trip", CategoryOverrides.Load(loaded.Save()).Save() == "1:");
        var escaped = new CategoryOverride { Terms = new[] { "caf\u00e9", "a|b;c%", "x,y" } };
        escaped.Added.Add("FutureCategory");
        escaped.Disabled.Add("FutureResistance");
        loaded.Set("mod;item|%,\u00e9", escaped);
        var roundTrip = CategoryOverrides.Load(loaded.Save()).Get("mod;item|%,\u00e9");
        check("escaped identifiers and words preserved", roundTrip.Terms.SequenceEqual(escaped.Terms));
        check("unknown categories retained across versions", roundTrip.Added.Contains("FutureCategory") && roundTrip.Disabled.Contains("FutureResistance"));
        check("accent insensitive custom word", Match(loaded.Apply(new SpellingEntry("Object", "mod;item|%,\u00e9")), "cafe"));
        check("unknown category ignored safely", roundTrip.Apply(new ItemCategory[0]).Length == 0);
        bool badDataRejected = false;
        try { CategoryOverrides.Load("2:future"); } catch (FormatException) { badDataRejected = true; }
        check("unsupported format rejected for preservation", badDataRejected);
        badDataRejected = false;
        try { CategoryOverrides.Load("1:broken|record"); } catch (FormatException) { badDataRejected = true; }
        check("malformed records rejected for preservation", badDataRejected);
        foreach (ItemCategory category in Enum.GetValues(typeof(ItemCategory)))
        {
            var draft = new CategoryOverride();
            draft.Toggle(category, new[] { category });
            check("disable detected " + category, !draft.Apply(new[] { category }).Contains(category));
            draft.Toggle(category, new[] { category });
            check("restore detected " + category, draft.IsEmpty);
            draft.Toggle(category, new ItemCategory[0]);
            check("enable missing " + category, draft.Apply(new ItemCategory[0]).Contains(category));
        }
        var hierarchy = new CategoryOverride();
        hierarchy.Added.Add("Legs");
        hierarchy.Disabled.Add("Armour");
        check("explicit parent exclusion wins", hierarchy.Apply(new ItemCategory[0]).SequenceEqual(new[] { ItemCategory.Legs }));
        var customOnly = new CategoryOverrides();
        customOnly.Set(item.Prefab, new CategoryOverride { Terms = new[] { "sailing" } });
        check("custom words do not expand unrelated categories", customOnly.Apply(item).Categories.SequenceEqual(item.Categories));
        var literal = new SpellingEntry("Dragon Sword", "Sword");
        var q = new SearchQuery("dragon");
        check("title rank above custom words", q.RankMatches(new[] { applied, literal }, x => x).First() == literal);
        check("favourites still outrank title", FavoriteSet.First(q.RankMatches(new[] { applied, literal }, x => x), x => x == applied).First() == applied);
    }
    private static bool Match(SpellingEntry entry, string text) =>
        entry.DirectMatches(text.Split(' ').Select(word => new SearchTerm(word)).ToArray());
}
