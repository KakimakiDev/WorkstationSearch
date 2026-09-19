using System.Collections.Generic;
using System.Linq;

namespace WorkstationSearch
{
    internal static class SearchVocabulary
    {
        // English vocabulary only. Metadata classification belongs in ItemTagRules.
        private static readonly Dictionary<string, string> WordForms = new Dictionary<string, string>
        {
            { "legs", "leg" },
            { "boots", "boot" },
            { "helmets", "helmet" },
            { "hats", "hat" },
            { "shoulders", "shoulder" },
            { "capes", "cape" },
            { "cloaks", "cloak" },
            { "hands", "hand" },
            { "gloves", "glove" },
            { "shields", "shield" },
            { "accessories", "accessory" },
            { "trinkets", "trinket" },
            { "materials", "material" },
            { "resources", "resource" },
            { "consumables", "consumable" },
            { "trophies", "trophy" },
            { "tools", "tool" },
            { "torches", "torch" },
            { "swords", "sword" },
            { "knives", "knife" },
            { "daggers", "dagger" },
            { "clubs", "club" },
            { "maces", "mace" },
            { "polearms", "polearm" },
            { "atgeirs", "atgeir" },
            { "spears", "spear" },
            { "axes", "axe" },
            { "bows", "bow" },
            { "crossbows", "crossbow" },
            { "pickaxes", "pickaxe" },
            { "arrows", "arrow" },
            { "bolts", "bolt" },
            { "weapons", "weapon" },
            { "armour", "armor" },
        };
        private static readonly Dictionary<string, ItemCategory> Aliases = new Dictionary<string, ItemCategory>();
        private static readonly KeyValuePair<string, ItemCategory>[] FuzzyWords;

        static SearchVocabulary()
        {
            Add(ItemCategory.Legs, "leg", "boot", "greaves", "leggings", "pants", "trousers");
            Add(ItemCategory.Helmet, "helmet", "hat", "head", "headgear");
            Add(ItemCategory.Chest, "chest", "tunic", "cuirass", "breastplate", "torso");
            Add(ItemCategory.Shoulder, "shoulder", "cape", "cloak");
            Add(ItemCategory.Hands, "hand", "glove", "gauntlets");
            Add(ItemCategory.Shield, "shield");
            Add(ItemCategory.Utility, "utility");
            Add(ItemCategory.Trinket, "trinket");
            Add(ItemCategory.Ammo, "ammo", "ammunition");
            Add(ItemCategory.Material, "material", "resource");
            Add(ItemCategory.Consumable, "consumable");
            Add(ItemCategory.Trophy, "trophy");
            Add(ItemCategory.Fish, "fish");
            Add(ItemCategory.Tool, "tool");
            Add(ItemCategory.Torch, "torch");
            Add(ItemCategory.Swords, "sword");
            Add(ItemCategory.Knives, "knife", "dagger");
            Add(ItemCategory.Clubs, "club", "mace");
            Add(ItemCategory.Polearms, "polearm", "atgeir");
            Add(ItemCategory.Spears, "spear");
            Add(ItemCategory.Axes, "axe");
            Add(ItemCategory.Bows, "bow");
            Add(ItemCategory.Crossbows, "crossbow");
            Add(ItemCategory.Pickaxes, "pickaxe", "mining");
            Add(ItemCategory.ElementalMagic, "elemental");
            Add(ItemCategory.BloodMagic, "blood");
            Add(ItemCategory.Fishing, "fishing");
            Add(ItemCategory.Armour, "armor");
            Add(ItemCategory.Accessory, "accessory");
            Add(ItemCategory.Arrow, "arrow");
            Add(ItemCategory.Bolt, "bolt");
            Add(ItemCategory.Magic, "magic");
            Add(ItemCategory.Weapon, "weapon");
            Add(ItemCategory.Food, "food");
            Add(ItemCategory.Blunt, "blunt");
            Add(ItemCategory.ResistBlunt, "resist-blunt");
            Add(ItemCategory.Slash, "slash");
            Add(ItemCategory.ResistSlash, "resist-slash");
            Add(ItemCategory.Pierce, "pierce");
            Add(ItemCategory.ResistPierce, "resist-pierce");
            Add(ItemCategory.ResistChop, "resist-chop");
            Add(ItemCategory.ResistPickaxe, "resist-pickaxe");
            Add(ItemCategory.Fire, "fire");
            Add(ItemCategory.ResistFire, "resist-fire");
            Add(ItemCategory.Frost, "frost");
            Add(ItemCategory.ResistFrost, "resist-frost");
            Add(ItemCategory.Lightning, "lightning");
            Add(ItemCategory.ResistLightning, "resist-lightning");
            Add(ItemCategory.Poison, "poison");
            Add(ItemCategory.ResistPoison, "resist-poison");
            Add(ItemCategory.Spirit, "spirit");
            Add(ItemCategory.ResistSpirit, "resist-spirit");
            Add(ItemCategory.ResistDamage, "resist-damage");
            Add(ItemCategory.ResistNonPlayer, "resist-nonplayer");
            Add(ItemCategory.ResistPhysical, "resist-physical");
            Add(ItemCategory.ResistElemental, "resist-elemental");
            // Keep plural spellings eligible for typo correction, too.
            FuzzyWords = Aliases.Concat(WordForms.Select(form =>
                new KeyValuePair<string, ItemCategory>(form.Key, Aliases[form.Value]))).ToArray();
        }
        private static void Add(ItemCategory category, params string[] words)
        {
            foreach (var word in words) Aliases.Add(word, category);
        }
        internal static ItemCategory? Resolve(string normalized)
        {
            if (WordForms.TryGetValue(normalized, out var canonical)) normalized = canonical;
            return Aliases.TryGetValue(normalized, out var category) ? category : (ItemCategory?)null;
        }
        internal static ItemCategory[] ResolveTypo(string normalized) => FuzzyWords
            .Where(word => SpellingEntry.OneEdit(normalized, word.Key)).Select(word => word.Value).Distinct().ToArray();
    }

    internal sealed class SearchTerm
    {
        internal readonly string Text;
        internal readonly string Normalized;
        internal readonly ItemCategory? Category;
        private ItemCategory[] approximateCategories;
        internal SearchTerm(string text)
        {
            Text = text;
            Normalized = SpellingEntry.Normalize(text);
            Category = SearchVocabulary.Resolve(Normalized);
        }
        // Resolve once per query term, only if direct results are empty.
        internal ItemCategory[] ApproximateCategories => approximateCategories ??
            (approximateCategories = SearchVocabulary.ResolveTypo(Normalized));
    }
}
