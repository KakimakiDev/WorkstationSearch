# Search categories

Tags come from item metadata, not descriptions. These initial aliases are English; item names still use the game's selected language. Custom modded items work when they use the standard categories and ammo keys.

| Category | Aliases |
| --- | --- |
| Legs | leg, legs, boot, boots, greaves, leggings, pants, trousers, armor, armour |
| Helmet | helmet, helmets, hat, hats, head, headgear, armor, armour |
| Chest | chest, tunic, cuirass, breastplate, torso, armor, armour |
| Shoulder | shoulder, shoulders, cape, capes, cloak, cloaks |
| Hands | hand, hands, glove, gloves, gauntlets |
| Shield | shield, shields |
| Utility | utility, accessory, accessories |
| Trinket | trinket, trinkets, accessory, accessories |
| Ammunition | ammo, ammunition |
| Arrow ammunition | arrow, arrows |
| Bolt ammunition | bolt, bolts |
| Material | material, materials, resource, resources |
| Consumable | consumable, consumables |
| Trophy | trophy, trophies |
| Fish | fish |
| Tool | tool, tools |
| Torch | torch, torches |
| Weapon | weapon, weapons |
| Food stats present | food |

Weapon/tool skill metadata additionally supplies:

| Skill | Aliases |
| --- | --- |
| Swords | sword, swords |
| Knives | knife, knives, dagger, daggers |
| Clubs | club, clubs, mace, maces |
| Polearms | polearm, polearms, atgeir, atgeirs |
| Spears | spear, spears |
| Axes | axe, axes |
| Bows | bow, bows |
| Crossbows | crossbow, crossbows |
| Pickaxes | pickaxe, pickaxes, mining |
| Elemental magic | magic, elemental |
| Blood magic | magic, blood |
| Fishing | fishing |

Positive base damage supplies `fire`, `frost`, `poison`, `lightning`, `spirit`, `blunt`, `slash`, or `pierce`.

Explicit resistance, strong resistance or immunity supplies `resist-` followed by the damage-type name, for example `resist-frost`. It does not supply the offensive damage tag. Effects granted indirectly through status effects are not inferred.

Armour sets, recipe ingredients, biomes, food-stat rankings, and custom equipment slots are not inferred. For example, `iron boots` needs `iron` in the name or prefab and a matching leg-equipment alias. Unknown ammo types keep generic ammunition tags rather than being guessed as arrows or bolts.

Names and tags are cached in memory after joining. Opening inventory reconciles changed data, and normal recipe-list updates capture displayed row titles. No disk dictionary or network lookup is used.
