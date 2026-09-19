using System.Collections.Generic;

namespace WorkstationSearch
{
    internal static class ItemTags
    {
        internal static ItemCategory[] Get(ItemDrop.ItemData.SharedData data)
        {
            var properties = new List<ItemCategory>();
            var damage = data.m_damages;
            if (damage.m_fire > 0) properties.Add(ItemCategory.Fire);
            if (damage.m_frost > 0) properties.Add(ItemCategory.Frost);
            if (damage.m_poison > 0) properties.Add(ItemCategory.Poison);
            if (damage.m_lightning > 0) properties.Add(ItemCategory.Lightning);
            if (damage.m_spirit > 0) properties.Add(ItemCategory.Spirit);
            if (damage.m_blunt > 0) properties.Add(ItemCategory.Blunt);
            if (damage.m_slash > 0) properties.Add(ItemCategory.Slash);
            if (damage.m_pierce > 0) properties.Add(ItemCategory.Pierce);
            foreach (var modifier in data.m_damageModifiers)
                if (modifier.m_modifier == HitData.DamageModifier.Resistant ||
                    modifier.m_modifier == HitData.DamageModifier.VeryResistant ||
                    modifier.m_modifier == HitData.DamageModifier.Immune)
                    switch (modifier.m_type)
                    {
                        case HitData.DamageType.Blunt: properties.Add(ItemCategory.ResistBlunt); break;
                        case HitData.DamageType.Slash: properties.Add(ItemCategory.ResistSlash); break;
                        case HitData.DamageType.Pierce: properties.Add(ItemCategory.ResistPierce); break;
                        case HitData.DamageType.Chop: properties.Add(ItemCategory.ResistChop); break;
                        case HitData.DamageType.Pickaxe: properties.Add(ItemCategory.ResistPickaxe); break;
                        case HitData.DamageType.Fire: properties.Add(ItemCategory.ResistFire); break;
                        case HitData.DamageType.Frost: properties.Add(ItemCategory.ResistFrost); break;
                        case HitData.DamageType.Lightning: properties.Add(ItemCategory.ResistLightning); break;
                        case HitData.DamageType.Poison: properties.Add(ItemCategory.ResistPoison); break;
                        case HitData.DamageType.Spirit: properties.Add(ItemCategory.ResistSpirit); break;
                        case HitData.DamageType.Damage: properties.Add(ItemCategory.ResistDamage); break;
                        case HitData.DamageType.NonPlayer: properties.Add(ItemCategory.ResistNonPlayer); break;
                        case HitData.DamageType.Physical: properties.Add(ItemCategory.ResistPhysical); break;
                        case HitData.DamageType.Elemental: properties.Add(ItemCategory.ResistElemental); break;
                    }
            return ItemTagRules.Build(data.m_itemType, data.m_skillType,
                data.m_food > 0 || data.m_foodStamina > 0 || data.m_foodEitr > 0, properties, data.m_ammoType);
        }
    }
}
