// Minimal test doubles for metadata consumed by the production classifier.
// The plugin build separately verifies these accesses against Valheim assemblies.
internal class ItemDrop
{
    internal class ItemData
    {
        internal sealed class SharedData
        {
            internal ItemType m_itemType;
            internal Skills.SkillType m_skillType;
            internal float m_food, m_foodStamina, m_foodEitr;
            internal string m_ammoType;
            internal HitData.DamageTypes m_damages;
            internal System.Collections.Generic.List<HitData.DamageModPair> m_damageModifiers =
                new System.Collections.Generic.List<HitData.DamageModPair>();
        }
        internal enum ItemType { None, Legs, Helmet, Chest, Shoulder, Hands, Shield, Utility, Trinket, Ammo, AmmoNonEquipable, Material, Consumable, Trophy, Fish, Tool, Torch, OneHandedWeapon, TwoHandedWeapon, TwoHandedWeaponLeft, Bow, Attach_Atgeir }
    }
}
internal class Skills
{
    internal enum SkillType { None, Swords, Knives, Clubs, Polearms, Spears, Axes, Bows, Crossbows, Pickaxes, ElementalMagic, BloodMagic, Fishing }
}

internal class HitData
{
    internal enum DamageType { Blunt, Slash, Pierce, Chop, Pickaxe, Fire, Frost, Lightning, Poison, Spirit, Damage, NonPlayer, Physical, Elemental }
    internal enum DamageModifier { Normal, Resistant, VeryResistant, Immune, Weak }
    internal struct DamageTypes { internal float m_fire, m_frost, m_poison, m_lightning, m_spirit, m_blunt, m_slash, m_pierce; }
    internal struct DamageModPair { internal DamageType m_type; internal DamageModifier m_modifier; }
}
