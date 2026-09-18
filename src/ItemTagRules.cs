using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkstationSearch
{
    internal static class ItemTagRules
    {
        private static readonly Dictionary<string, string> Slots = new Dictionary<string, string> {
            { "Legs", "leg legs boots boot greaves leggings pants trousers armour armor" },
            { "Helmet", "helmet helmets hat hats head headgear armour armor" },
            { "Chest", "chest tunic cuirass breastplate torso armour armor" },
            { "Shoulder", "shoulder shoulders cape capes cloak cloaks" },
            { "Hands", "hand hands glove gloves gauntlets" },
            { "Shield", "shield shields" }, { "Utility", "utility accessory accessories" },
            { "Trinket", "trinket trinkets accessory accessories" },
            { "Ammo", "ammo ammunition" }, { "AmmoNonEquipable", "ammo ammunition" },
            { "Material", "material materials resource resources" },
            { "Consumable", "consumable consumables" }, { "Trophy", "trophy trophies" },
            { "Fish", "fish" }, { "Tool", "tool tools" }, { "Torch", "torch torches" }
        };
        private static readonly Dictionary<string, string> Skills = new Dictionary<string, string> {
            { "Swords", "sword swords" }, { "Knives", "knife knives dagger daggers" },
            { "Clubs", "club clubs mace maces" }, { "Polearms", "polearm polearms atgeir atgeirs" },
            { "Spears", "spear spears" }, { "Axes", "axe axes" }, { "Bows", "bow bows" },
            { "Crossbows", "crossbow crossbows" }, { "Pickaxes", "pickaxe pickaxes mining" },
            { "ElementalMagic", "magic elemental" }, { "BloodMagic", "magic blood" },
            { "Fishing", "fishing" }
        };
        internal static string[] Build(string type, string skill, bool food, IEnumerable<string> properties = null, string ammoType = null)
        {
            var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (Slots.TryGetValue(type, out var aliases)) tags.UnionWith(aliases.Split(' '));
            // Only ammunition gets these tags. Bows also specify the ammo they
            // consume, but should not appear in a search for arrows.
            if (type == "Ammo" || type == "AmmoNonEquipable")
            {
                if (string.Equals(ammoType, "$ammo_arrows", StringComparison.Ordinal))
                    tags.UnionWith(new[] { "arrow", "arrows" });
                else if (string.Equals(ammoType, "$ammo_bolts", StringComparison.Ordinal))
                    tags.UnionWith(new[] { "bolt", "bolts" });
            }
            bool weapon = type == "OneHandedWeapon" || type == "TwoHandedWeapon" ||
                type == "TwoHandedWeaponLeft" || type == "Bow" || type == "Attach_Atgeir";
            if (weapon) tags.UnionWith(new[] { "weapon", "weapons" });
            if ((weapon || type == "Tool") && Skills.TryGetValue(skill, out aliases)) tags.UnionWith(aliases.Split(' '));
            if (food) tags.Add("food");
            if (properties != null) tags.UnionWith(properties);
            return tags.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        }
    }
}
