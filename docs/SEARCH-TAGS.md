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

## Implementation

`ItemTags` and `ItemTagRules` classify game metadata through typed enums. Items cache compact `ItemCategory` IDs rather than copies of English aliases. `CategoryHierarchy` adds shared parent categories, such as Legs to Armour and Arrow to Ammo.

`SearchVocabulary` holds English synonyms and explicit word forms. For example, boots resolves to boot, then to Legs. Axes explicitly resolves to axe; arbitrary suffixes are never stripped. Name and prefab substring matching remain literal and accent-insensitive.

Each query term resolves its category once. If no direct results exist, typo candidates from the shared vocabulary are computed once per term and reused across items. Title words remain cached per item. Favourites and title-priority ranking are unchanged. Description inference is not used.

## Per-item overrides

Ctrl + middle-click a recipe to edit its categories. Added categories include their parent categories; explicit exclusions are applied after inheritance, so an excluded category stays disabled. Automatically detected parent categories can also be switched off independently. Reset to automatic removes only the selected prefab's record when saved.

Custom words match whole query words, ignoring case and accents, and participate in the usual typo fallback. They do not gain title-ranking priority. Names, prefab names and displayed titles remain searchable regardless of category exclusions.

The profile configuration stores overrides by the exact prefab ID. Catalogue rebuilds and mod removal never prune these records. Reinstalling a mod with the same prefab IDs restores its overrides; an item renamed to a different prefab ID is treated as a different item. Unknown saved category names are retained across plugin versions. Unreadable override data is logged and editing is disabled to avoid overwriting it.
