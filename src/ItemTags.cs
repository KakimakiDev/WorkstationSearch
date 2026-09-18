using System.Collections.Generic;

namespace WorkstationSearch
{
    internal static class ItemTags
    {
        internal static string[] Get(ItemDrop.ItemData.SharedData data)
        {
            var properties = new List<string>();
            var damage = data.m_damages;
            if (damage.m_fire > 0) properties.Add("fire");
            if (damage.m_frost > 0) properties.Add("frost");
            if (damage.m_poison > 0) properties.Add("poison");
            if (damage.m_lightning > 0) properties.Add("lightning");
            if (damage.m_spirit > 0) properties.Add("spirit");
            if (damage.m_blunt > 0) properties.Add("blunt");
            if (damage.m_slash > 0) properties.Add("slash");
            if (damage.m_pierce > 0) properties.Add("pierce");
            foreach (var modifier in data.m_damageModifiers)
                if (modifier.m_modifier == HitData.DamageModifier.Resistant ||
                    modifier.m_modifier == HitData.DamageModifier.VeryResistant ||
                    modifier.m_modifier == HitData.DamageModifier.Immune)
                    properties.Add("resist-" + modifier.m_type.ToString().ToLowerInvariant());
            return ItemTagRules.Build(data.m_itemType.ToString(), data.m_skillType.ToString(),
                data.m_food > 0 || data.m_foodStamina > 0 || data.m_foodEitr > 0, properties, data.m_ammoType);
        }
    }
}
