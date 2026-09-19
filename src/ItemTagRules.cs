using System.Collections.Generic;
using ItemType = ItemDrop.ItemData.ItemType;
using SkillType = Skills.SkillType;

namespace WorkstationSearch
{
    internal static class ItemTagRules
    {
        internal static ItemCategory[] Build(ItemType type, SkillType skill, bool food,
            IEnumerable<ItemCategory> properties = null, string ammoType = null)
        {
            var categories = properties == null ? new List<ItemCategory>() : new List<ItemCategory>(properties);
            switch (type)
            {
                case ItemType.Legs: categories.Add(ItemCategory.Legs); break;
                case ItemType.Helmet: categories.Add(ItemCategory.Helmet); break;
                case ItemType.Chest: categories.Add(ItemCategory.Chest); break;
                case ItemType.Shoulder: categories.Add(ItemCategory.Shoulder); break;
                case ItemType.Hands: categories.Add(ItemCategory.Hands); break;
                case ItemType.Shield: categories.Add(ItemCategory.Shield); break;
                case ItemType.Utility: categories.Add(ItemCategory.Utility); break;
                case ItemType.Trinket: categories.Add(ItemCategory.Trinket); break;
                case ItemType.Ammo: categories.Add(ItemCategory.Ammo); break;
                case ItemType.AmmoNonEquipable: categories.Add(ItemCategory.Ammo); break;
                case ItemType.Material: categories.Add(ItemCategory.Material); break;
                case ItemType.Consumable: categories.Add(ItemCategory.Consumable); break;
                case ItemType.Trophy: categories.Add(ItemCategory.Trophy); break;
                case ItemType.Fish: categories.Add(ItemCategory.Fish); break;
                case ItemType.Tool: categories.Add(ItemCategory.Tool); break;
                case ItemType.Torch: categories.Add(ItemCategory.Torch); break;
            }
            // Consuming ammunition does not make a bow an arrow or bolt.
            if (type == ItemType.Ammo || type == ItemType.AmmoNonEquipable)
            {
                if (ammoType == "$ammo_arrows") categories.Add(ItemCategory.Arrow);
                else if (ammoType == "$ammo_bolts") categories.Add(ItemCategory.Bolt);
            }
            bool weapon = type == ItemType.OneHandedWeapon || type == ItemType.TwoHandedWeapon ||
                type == ItemType.TwoHandedWeaponLeft || type == ItemType.Bow || type == ItemType.Attach_Atgeir;
            if (weapon) categories.Add(ItemCategory.Weapon);
            if (weapon || type == ItemType.Tool)
                switch (skill)
                {
                    case SkillType.Swords: categories.Add(ItemCategory.Swords); break;
                    case SkillType.Knives: categories.Add(ItemCategory.Knives); break;
                    case SkillType.Clubs: categories.Add(ItemCategory.Clubs); break;
                    case SkillType.Polearms: categories.Add(ItemCategory.Polearms); break;
                    case SkillType.Spears: categories.Add(ItemCategory.Spears); break;
                    case SkillType.Axes: categories.Add(ItemCategory.Axes); break;
                    case SkillType.Bows: categories.Add(ItemCategory.Bows); break;
                    case SkillType.Crossbows: categories.Add(ItemCategory.Crossbows); break;
                    case SkillType.Pickaxes: categories.Add(ItemCategory.Pickaxes); break;
                    case SkillType.ElementalMagic: categories.Add(ItemCategory.ElementalMagic); break;
                    case SkillType.BloodMagic: categories.Add(ItemCategory.BloodMagic); break;
                    case SkillType.Fishing: categories.Add(ItemCategory.Fishing); break;
                }
            if (food) categories.Add(ItemCategory.Food);
            return CategoryHierarchy.Expand(categories);
        }
    }
}
