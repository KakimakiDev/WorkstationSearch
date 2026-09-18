using System;
using System.Linq;
using WorkstationSearch;

internal static class Program
{
    private static int Main()
    {
        Check("empty query", new SearchQuery(null).Matches(null, null));
        Check("whitespace", new SearchQuery(" \t ").IsEmpty);
        Check("case-insensitive substring", new SearchQuery("RON").Matches("Iron sword", "SwordIron"));
        Check("unordered terms", new SearchQuery("sword iron").Matches("Iron sword", "SwordIron"));
        Check("all terms required", !new SearchQuery("iron bow").Matches("Iron sword", "SwordIron"));
        Check("accent-insensitive localized name", new SearchQuery("epee").Matches("Épée en fer", "SwordIron"));
        Check("prefab fallback", new SearchQuery("swordiron").Matches("Épée en fer", "SwordIron"));
        Check("no match", !new SearchQuery("bronze").Matches("Iron sword", "SwordIron"));
        Check("null fields", !new SearchQuery("iron").Matches(null, null));
        Check("literal punctuation", !new SearchQuery(".*").Matches("Iron sword", "SwordIron"));
        var favorites = new FavoriteSet("");
        favorites.Toggle("Iron sword");
        favorites.Toggle("Bronze axe");
        var rows = new[] { "Iron axe", "Iron sword", "Bronze axe", "Iron bow" };
        Check("favorites first, stable groups", FavoriteSet.First(rows, favorites.Contains).SequenceEqual(
            new[] { "Iron sword", "Bronze axe", "Iron axe", "Iron bow" }));
        var query = new SearchQuery("iron");
        Check("search excludes unrelated favorites", FavoriteSet.First(rows.Where(x => query.Matches(x, null)), favorites.Contains)
            .SequenceEqual(new[] { "Iron sword", "Iron axe", "Iron bow" }));
        Check("no search matches", FavoriteSet.First(rows.Where(x => new SearchQuery("silver").Matches(x, null)), favorites.Contains).Count == 0);
        favorites = new FavoriteSet(favorites.Save());
        Check("favorites persist", favorites.Contains("Iron sword") && favorites.Contains("Bronze axe"));
        favorites.Toggle("Iron sword");
        Check("unfavorite persists", !new FavoriteSet(favorites.Save()).Contains("Iron sword"));
        favorites.Toggle("mod;item%é");
        Check("escaped names persist", new FavoriteSet(favorites.Save()).Contains("mod;item%é"));
        Check("duplicate upgrade rows preserved", FavoriteSet.First(new[] { "Iron axe", "Bronze axe", "Bronze axe" }, favorites.Contains)
            .SequenceEqual(new[] { "Bronze axe", "Bronze axe", "Iron axe" }));
        Check("no favorites retains order", FavoriteSet.First(rows, new FavoriteSet(null).Contains).SequenceEqual(rows));
        var sword = new SpellingEntry("Iron Sword", "SwordIron");
        Check("adjacent swap", new SearchQuery("iorn sword").FuzzyMatches(sword));
        Check("missing letter", new SearchQuery("iron swrd").FuzzyMatches(sword));
        Check("extra letter", new SearchQuery("ironn sword").FuzzyMatches(sword));
        Check("substitution", new SearchQuery("iron swxrd").FuzzyMatches(sword));
        Check("two edits rejected", !new SearchQuery("iron swxxd").FuzzyMatches(sword));
        Check("short typo rejected", !new SearchQuery("irn").FuzzyMatches(sword));
        Check("partial word kept with typo", new SearchQuery("iorn sw").FuzzyMatches(sword));
        Check("every term required", !new SearchQuery("iorn bow").FuzzyMatches(sword));
        Check("accent typo", new SearchQuery("epeee").FuzzyMatches(new SpellingEntry("Épée", "Sword")));
        var corpus = new[] { sword, new SpellingEntry("Iron Axe", "AxeIron"), new SpellingEntry("Bronze Sword", "SwordBronze") };
        var found = new SearchQuery("iorn sw").SelectMatches(corpus, x => x, out bool approximate);
        Check("fallback selection", approximate && found.SequenceEqual(new[] { sword }));
        found = new SearchQuery("iron").SelectMatches(corpus, x => x, out approximate);
        Check("direct mode", !approximate && found.Count == 2);
        found = new SearchQuery("iron").SelectMatches(new[] { sword, new SpellingEntry("Iroon Axe", "Other") }, x => x, out approximate);
        Check("direct match excludes fuzzy alternatives", !approximate && found.SequenceEqual(new[] { sword }));
        found = new SearchQuery("zzzz").SelectMatches(corpus, x => x, out approximate);
        Check("no false status on no results", !approximate && found.Count == 0);
        var greaves = new SpellingEntry("Iron Greaves", "ArmorIronLegs", ItemTagRules.Build("Legs", "None", false));
        var boots = new SpellingEntry("Leather Boots", "LeatherLegs", ItemTagRules.Build("Legs", "None", false));
        var helmet = new SpellingEntry("Iron Helmet", "ArmorIronHelmet", ItemTagRules.Build("Helmet", "None", false));
        var armor = new[] { helmet, boots, greaves };
        found = new SearchQuery("boots").SelectMatches(armor, x => x, out approximate);
        Check("literal boots do not suppress slot matches", !approximate && found.SequenceEqual(new[] { boots, greaves }));
        found = new SearchQuery("iron boots").SelectMatches(armor, x => x, out approximate);
        Check("name plus slot synonym", !approximate && found.SequenceEqual(new[] { greaves }));
        found = new SearchQuery("iorn boots").SelectMatches(armor, x => x, out approximate);
        Check("typo plus slot synonym", approximate && found.SequenceEqual(new[] { greaves }));
        found = new SearchQuery("iron bots").SelectMatches(armor, x => x, out approximate);
        Check("typo in synonym", approximate && found.SequenceEqual(new[] { greaves }));
        Check("metadata works without name hints", new SearchQuery("pants").SelectMatches(new[] {
            new SpellingEntry("Modded Relic", "Relic01", ItemTagRules.Build("Legs", "None", false)) }, x => x, out approximate).Count == 1);
        Check("tags do not change favourite priority", FavoriteSet.First(
            new SearchQuery("boots").SelectMatches(armor, x => x, out approximate), x => x == greaves).First() == greaves);
        Check("weapon skill aliases", ItemTagRules.Build("OneHandedWeapon", "Knives", false).Contains("dagger"));
        Check("nonweapon default skill ignored", !ItemTagRules.Build("Material", "Swords", false).Contains("sword"));
        Check("food from stats", ItemTagRules.Build("Consumable", "None", true).Contains("food"));
        var resistant = new SpellingEntry("Protective Cloak", "Cape01", ItemTagRules.Build("Shoulder", "None", false, new[] { "resist-frost" }));
        Check("resistance separated from offensive damage", new SearchQuery("frost").SelectMatches(new[] { resistant }, x => x, out approximate).Count == 0);
        Check("explicit resistance query", new SearchQuery("resist-frost cloak").SelectMatches(new[] { resistant }, x => x, out approximate).Count == 1 && !approximate);
        Check("unknown mod category safe", ItemTagRules.Build("CustomSlot", "None", false).Length == 0);
        var arrowTags = ItemTagRules.Build("Ammo", "None", false, ammoType: "$ammo_arrows");
        var boltTags = ItemTagRules.Build("Ammo", "None", false, ammoType: "$ammo_bolts");
        var silver = new SpellingEntry("Silver Arrow", "ArrowSilver", arrowTags);
        var wood = new SpellingEntry("Wood Arrow", "ArrowWood", arrowTags);
        var fire = new SpellingEntry("Fire Arrow", "ArrowFire", arrowTags);
        var bolt = new SpellingEntry("Iron Bolt", "BoltIron", boltTags);
        var ammo = new[] { silver, wood, fire, bolt };
        found = new SearchQuery("arrows").SelectMatches(ammo, x => x, out approximate);
        Check("plural arrows includes all arrow types despite ArrowSilver prefab", !approximate && found.SequenceEqual(new[] { silver, wood, fire }));
        Check("singular arrow unchanged", new SearchQuery("arrow").SelectMatches(ammo, x => x, out approximate).SequenceEqual(found));
        Check("plural bolts excludes arrows", new SearchQuery("bolts").SelectMatches(ammo, x => x, out approximate).SequenceEqual(new[] { bolt }) && !approximate);
        Check("material plus plural ammo", new SearchQuery("silver arrows").SelectMatches(ammo, x => x, out approximate).SequenceEqual(new[] { silver }));
        Check("bows not labelled arrows", !ItemTagRules.Build("Bow", "Bows", false, ammoType: "$ammo_arrows").Contains("arrows"));
        Check("custom ammo not guessed", !ItemTagRules.Build("Ammo", "None", false, ammoType: "$ammo_custom").Contains("arrows"));
        Check("non equipable ammo", ItemTagRules.Build("AmmoNonEquipable", "None", false, ammoType: "$ammo_bolts").Contains("bolts"));
        Check("plural arrow favourites first", FavoriteSet.First(found, x => x == fire).SequenceEqual(new[] { fire, silver, wood }));
        var chestTags = ItemTagRules.Build("Chest", "None", false);
        var rag = new SpellingEntry("Rag Tunic", "RagChest", chestTags);
        var carapace = new SpellingEntry("Carapace Breastplate", "CarapaceChest", chestTags);
        var bronze = new SpellingEntry("Bronze Plate Tunic", "BreastplateBronze", chestTags);
        var ask = new SpellingEntry("Breastplate of Ask", "AskChest", chestTags);
        var chestRows = new[] { rag, carapace, bronze, ask };
        var chestQuery = new SearchQuery("breastplate");
        var ranked = chestQuery.RankMatches(chestQuery.SelectMatches(chestRows, x => x, out approximate), x => x);
        Check("title above tag and prefab, stable within groups", ranked.SequenceEqual(new[] { carapace, ask, rag, bronze }));
        var exact = new SpellingEntry("Breastplate", "GenericChest", chestTags);
        Check("exact title above longer titles", chestQuery.RankMatches(new[] { carapace, exact, ask }, x => x).First() == exact);
        Check("favourites stay first with title rank inside groups", FavoriteSet.First(ranked, x => x == rag || x == ask)
            .SequenceEqual(new[] { ask, rag, carapace, bronze }));
        Check("blank search preserves game sort", new SearchQuery(" ").RankMatches(chestRows, x => x).SequenceEqual(chestRows));
        Check("mixed name and tag terms rank by title coverage", new SearchQuery("bronze breastplate").RankMatches(new[] {
            bronze, new SpellingEntry("Bronze Breastplate", "Chest02", chestTags) }, x => x).Last() == bronze);
        Check("accent and case insensitive title ranking", new SearchQuery("EPEE").RankMatches(new[] {
            new SpellingEntry("Other", "epee", new[] { "epee" }), new SpellingEntry("Épée", "Sword02") }, x => x).First().Name == "Épée");
        Check("plural ammo ties preserve order", new SearchQuery("arrows").RankMatches(new[] { fire, silver, wood }, x => x)
            .SequenceEqual(new[] { fire, silver, wood }));
        var harpoon = new SpellingEntry("Abyssal Harpoon", "SpearChitin", new[] { "weapon" });
        var karve = harpoon.WithDisplayName("<color=orange>Harpoon: Karve #1</color>");
        var longship = harpoon.WithDisplayName("Harpoon: Longship #2");
        Check("display title indexed per row", new SearchQuery("karve").SelectMatches(new[] { karve, longship }, x => x, out approximate)
            .SequenceEqual(new[] { karve }) && !approximate);
        Check("underlying recipe name still searchable", new SearchQuery("abyssal").SelectMatches(new[] { karve }, x => x, out approximate).Count == 1);
        Check("display title typo fallback", new SearchQuery("kavre").SelectMatches(new[] { karve }, x => x, out approximate).Count == 1 && approximate);
        Check("formatting stripped from title", karve.Name == "Harpoon: Karve #1");
        Check("empty title retains recipe name", object.ReferenceEquals(harpoon, harpoon.WithDisplayName(" ")));
        Check("display title has ranking priority", new SearchQuery("karve").RankMatches(new[] {
            new SpellingEntry("Other upgrade", "KarveOther"), karve }, x => x).First() == karve);
        Check("source catalogue not changed by row titles", harpoon.Name == "Abyssal Harpoon");
        Console.WriteLine("65 search, ranking, row title, tag, typo and favourite behavior checks passed.");
        return 0;
    }
    private static void Check(string name, bool result)
    {
        if (!result) throw new Exception("Failed: " + name);
    }
}
