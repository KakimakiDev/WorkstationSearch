using System.Collections.Generic;
using System.Linq;

namespace WorkstationSearch
{
    internal static class SearchCatalog
    {
        private static readonly Dictionary<Recipe, SpellingEntry> Entries = new Dictionary<Recipe, SpellingEntry>();
        private static ObjectDB database;
        private static string language;
        private static int count;
        internal static void Rebuild()
        {
            database = ObjectDB.instance;
            language = Localization.instance.GetSelectedLanguage();
            count = database ? database.m_recipes.Count : 0;
            var current = new HashSet<Recipe>();
            if (database) foreach (var recipe in database.m_recipes)
            {
                if (!recipe || !recipe.m_item) continue;
                current.Add(recipe);
                string name = Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name);
                string prefab = recipe.m_item.name;
                var tags = ItemTags.Get(recipe.m_item.m_itemData.m_shared);
                if (!Entries.TryGetValue(recipe, out var old) || old.Name != name || old.Prefab != prefab || !old.Categories.SequenceEqual(tags))
                    Entries[recipe] = new SpellingEntry(name, prefab, tags);
            }
            foreach (var recipe in new List<Recipe>(Entries.Keys))
                if (!current.Contains(recipe)) Entries.Remove(recipe);
        }
        internal static void Ensure()
        {
            if (database != ObjectDB.instance || language != Localization.instance.GetSelectedLanguage() ||
                (database && count != database.m_recipes.Count)) Rebuild();
        }
        internal static SpellingEntry Get(Recipe recipe)
        {
            if (!recipe || !recipe.m_item) return null;
            if (!Entries.TryGetValue(recipe, out var entry))
            {
                entry = new SpellingEntry(Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name), recipe.m_item.name,
                    ItemTags.Get(recipe.m_item.m_itemData.m_shared));
                Entries[recipe] = entry;
            }
            return entry;
        }
    }
}
