using System.Collections.Generic;
using System.Linq;

namespace WorkstationSearch
{
    // Semantic IDs are independent of the words used to search for them.
    internal enum ItemCategory
    {
        Legs,
        Helmet,
        Chest,
        Shoulder,
        Hands,
        Shield,
        Utility,
        Trinket,
        Ammo,
        Material,
        Consumable,
        Trophy,
        Fish,
        Tool,
        Torch,
        Swords,
        Knives,
        Clubs,
        Polearms,
        Spears,
        Axes,
        Bows,
        Crossbows,
        Pickaxes,
        ElementalMagic,
        BloodMagic,
        Fishing,
        Armour,
        Accessory,
        Arrow,
        Bolt,
        Magic,
        Weapon,
        Food,
        Blunt,
        ResistBlunt,
        Slash,
        ResistSlash,
        Pierce,
        ResistPierce,
        ResistChop,
        ResistPickaxe,
        Fire,
        ResistFire,
        Frost,
        ResistFrost,
        Lightning,
        ResistLightning,
        Poison,
        ResistPoison,
        Spirit,
        ResistSpirit,
        ResistDamage,
        ResistNonPlayer,
        ResistPhysical,
        ResistElemental
    }

    internal static class CategoryHierarchy
    {
        internal static ItemCategory[] Expand(IEnumerable<ItemCategory> categories)
        {
            var result = new HashSet<ItemCategory>(categories);
            foreach (var category in categories)
                switch (category)
                {
                    case ItemCategory.Legs: result.Add(ItemCategory.Armour); break;
                    case ItemCategory.Helmet: result.Add(ItemCategory.Armour); break;
                    case ItemCategory.Chest: result.Add(ItemCategory.Armour); break;
                    case ItemCategory.Utility: result.Add(ItemCategory.Accessory); break;
                    case ItemCategory.Trinket: result.Add(ItemCategory.Accessory); break;
                    case ItemCategory.Arrow: result.Add(ItemCategory.Ammo); break;
                    case ItemCategory.Bolt: result.Add(ItemCategory.Ammo); break;
                    case ItemCategory.ElementalMagic: result.Add(ItemCategory.Magic); break;
                    case ItemCategory.BloodMagic: result.Add(ItemCategory.Magic); break;
                }
            return result.OrderBy(category => category).ToArray();
        }
    }
}
