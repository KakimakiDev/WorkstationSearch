using System;
using System.Linq;
using WorkstationSearch;

internal static class MetadataRegression
{
    internal static void Run(Action<string, bool> check)
    {
        var data = new ItemDrop.ItemData.SharedData {
            m_itemType = ItemDrop.ItemData.ItemType.Material,
            m_skillType = Skills.SkillType.None, m_ammoType = "",
            m_damages = new HitData.DamageTypes {
                m_fire = 1, m_frost = 1, m_poison = 1, m_lightning = 1,
                m_spirit = 1, m_blunt = 1, m_slash = 1, m_pierce = 1
            }
        };
        var offensive = new[] { ItemCategory.Fire, ItemCategory.Frost, ItemCategory.Poison, ItemCategory.Lightning,
            ItemCategory.Spirit, ItemCategory.Blunt, ItemCategory.Slash, ItemCategory.Pierce };
        check("positive base damage classified", offensive.All(category => ItemTags.Get(data).Contains(category)));
        data.m_damages = new HitData.DamageTypes { m_fire = -1 };
        check("nonpositive damage ignored", !offensive.Any(category => ItemTags.Get(data).Contains(category)));
        foreach (HitData.DamageType type in Enum.GetValues(typeof(HitData.DamageType)))
            foreach (HitData.DamageModifier modifier in Enum.GetValues(typeof(HitData.DamageModifier)))
            {
                data.m_damageModifiers.Clear();
                data.m_damageModifiers.Add(new HitData.DamageModPair { m_type = type, m_modifier = modifier });
                var entry = new SpellingEntry("Relic", "Object01", ItemTags.Get(data));
                bool expected = modifier == HitData.DamageModifier.Resistant ||
                    modifier == HitData.DamageModifier.VeryResistant || modifier == HitData.DamageModifier.Immune;
                check(type + "/" + modifier, entry.DirectMatches(new[] { new SearchTerm("resist-" + type.ToString().ToLowerInvariant()) }) == expected);
                check("resistance does not imply damage", !offensive.Any(category => entry.Categories.Contains(category)));
            }
        data.m_food = 1;
        check("health food", ItemTags.Get(data).Contains(ItemCategory.Food));
        data.m_food = 0; data.m_foodStamina = 1;
        check("stamina food", ItemTags.Get(data).Contains(ItemCategory.Food));
        data.m_foodStamina = 0; data.m_foodEitr = 1;
        check("eitr food", ItemTags.Get(data).Contains(ItemCategory.Food));
        data.m_foodEitr = 0;
        check("no food stats", !ItemTags.Get(data).Contains(ItemCategory.Food));
    }
}
