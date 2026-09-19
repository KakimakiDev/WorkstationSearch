using System;
using System.Linq;
using System.Collections.Generic;
using WorkstationSearch;

internal static class VocabularyRegression
{
    private static readonly List<Tuple<SpellingEntry, string[]>> Legacy = new List<Tuple<SpellingEntry, string[]>>();
    internal static void Run(Action<string, bool> check)
    {
        Verify(check, ItemDrop.ItemData.ItemType.Legs, Skills.SkillType.None, "leg legs boots boot greaves leggings pants trousers armour armor");
        Verify(check, ItemDrop.ItemData.ItemType.Helmet, Skills.SkillType.None, "helmet helmets hat hats head headgear armour armor");
        Verify(check, ItemDrop.ItemData.ItemType.Chest, Skills.SkillType.None, "chest tunic cuirass breastplate torso armour armor");
        Verify(check, ItemDrop.ItemData.ItemType.Shoulder, Skills.SkillType.None, "shoulder shoulders cape capes cloak cloaks");
        Verify(check, ItemDrop.ItemData.ItemType.Hands, Skills.SkillType.None, "hand hands glove gloves gauntlets");
        Verify(check, ItemDrop.ItemData.ItemType.Shield, Skills.SkillType.None, "shield shields");
        Verify(check, ItemDrop.ItemData.ItemType.Utility, Skills.SkillType.None, "utility accessory accessories");
        Verify(check, ItemDrop.ItemData.ItemType.Trinket, Skills.SkillType.None, "trinket trinkets accessory accessories");
        Verify(check, ItemDrop.ItemData.ItemType.Ammo, Skills.SkillType.None, "ammo ammunition");
        Verify(check, ItemDrop.ItemData.ItemType.AmmoNonEquipable, Skills.SkillType.None, "ammo ammunition");
        Verify(check, ItemDrop.ItemData.ItemType.Material, Skills.SkillType.None, "material materials resource resources");
        Verify(check, ItemDrop.ItemData.ItemType.Consumable, Skills.SkillType.None, "consumable consumables");
        Verify(check, ItemDrop.ItemData.ItemType.Trophy, Skills.SkillType.None, "trophy trophies");
        Verify(check, ItemDrop.ItemData.ItemType.Fish, Skills.SkillType.None, "fish");
        Verify(check, ItemDrop.ItemData.ItemType.Tool, Skills.SkillType.None, "tool tools");
        Verify(check, ItemDrop.ItemData.ItemType.Torch, Skills.SkillType.None, "torch torches");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Swords, "sword swords weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Knives, "knife knives dagger daggers weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Clubs, "club clubs mace maces weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Polearms, "polearm polearms atgeir atgeirs weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Spears, "spear spears weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Axes, "axe axes weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Bows, "bow bows weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Crossbows, "crossbow crossbows weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Pickaxes, "pickaxe pickaxes mining weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.ElementalMagic, "magic elemental weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.BloodMagic, "magic blood weapon weapons");
        Verify(check, ItemDrop.ItemData.ItemType.OneHandedWeapon, Skills.SkillType.Fishing, "fishing weapon weapons");
        // Compare every historical alias against every category, including exclusions.
        foreach (var record in Legacy)
            foreach (var alias in Legacy.SelectMany(row => row.Item2).Distinct())
                check("legacy cross-category match: " + alias,
                    record.Item1.DirectMatches(new[] { new SearchTerm(alias) }) == record.Item2.Contains(alias));
        check("axes resolves explicitly", SearchVocabulary.Resolve("axes") == ItemCategory.Axes);
        check("glass not stemmed", SearchVocabulary.Resolve("glass") == null);
        check("unknown word not stemmed", SearchVocabulary.Resolve("axess") == null);
        check("inherited armour stored once", CategoryHierarchy.Expand(new[] { ItemCategory.Legs, ItemCategory.Chest })
            .Count(c => c == ItemCategory.Armour) == 1);
        check("arrow inherits ammo", CategoryHierarchy.Expand(new[] { ItemCategory.Arrow }).Contains(ItemCategory.Ammo));
        check("classification compact", ItemTagRules.Build(ItemDrop.ItemData.ItemType.Legs, Skills.SkillType.None, false).Length == 2);
        var legs = new SpellingEntry("Relic", "Relic01", ItemTagRules.Build(ItemDrop.ItemData.ItemType.Legs, Skills.SkillType.None, false));
        check("case and accents in aliases", new SearchQuery("BÓÓTS").SelectMatches(new[] { legs }, x => x, out bool approximate).Count == 1 && !approximate);
        check("partial aliases are not expanded", !legs.DirectMatches(new[] { new SearchTerm("boo") }));
        check("plural typo remains eligible", new SearchQuery("botts").FuzzyMatches(legs));
        var ordinary = new SpellingEntry("Glass axe", "Object");
        check("names still use literal partial search", new SearchQuery("glass ax").SelectMatches(new[] { ordinary }, x => x, out approximate).Count == 1 && !approximate);
    }
    private static void Verify(Action<string, bool> check, ItemDrop.ItemData.ItemType type, Skills.SkillType skill, string aliases)
    {
        var entry = new SpellingEntry("Relic", "Object01", ItemTagRules.Build(type, skill, false));
        Legacy.Add(Tuple.Create(entry, aliases.Split(' ')));
        foreach (var alias in aliases.Split(' '))
            check(type + "/" + skill + "/" + alias, entry.DirectMatches(new[] { new SearchTerm(alias) }));
    }
}
