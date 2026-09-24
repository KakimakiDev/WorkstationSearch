# Workstation Search

A client-side Valheim mod that adds search and favourites to the crafting and upgrade menus. Requires BepInEx 5.

## Features

- Search Craft and Upgrade recipes by item name, including partial names.
- Find items by category, such as `boots`, `arrows`, `helmet`, or `food`.
- Combine names and categories, such as `iron boots` or `resist-frost cloak`.
- Keep favourite recipes at the top, with favourites saved between sessions.
- Prioritise item-name matches over category-only matches.
- Find close spelling matches when a search has no direct results.

## Installation

Install on each player's client. A dedicated server does not need the mod.

1. Install BepInEx 5 for Valheim.
2. Close Valheim.
3. Extract the Workstation Search ZIP into your Valheim game folder or mod manager profile, merging the `BepInEx` folder. The plugin should be at `BepInEx/plugins/WorkstationSearch/WorkstationSearch.dll`.
4. Launch Valheim with BepInEx and open the crafting menu.

To update, close Valheim and replace the installed `WorkstationSearch.dll` with the new version. To uninstall, remove that DLL.

## How to use

| Action | Control |
| --- | --- |
| Search recipes | Click the search field and type |
| Clear search | Click X |
| Finish typing | Enter or Escape |
| Toggle favourite | Middle-click a recipe |

Matching favourites appear first, followed by other matching recipes. Within each group, matches in the item title take priority over category matches. Equally relevant results keep the game's sorting order.

Switching between Craft and Upgrade keeps your search. Closing the inventory clears it. Search changes take effect after an active craft or upgrade finishes; favourites cannot be toggled during that operation.

Favourites are shared across Craft, Upgrade, and characters in the same mod profile. They are saved in `BepInEx/config/local.valheim.craftsearch.cfg`.

## Search behaviour

Every word in your search must match. Item names support partial words, while category terms match whole words. Searches ignore capitalisation and accents. Localized names, internal prefab names, and custom recipe titles are searchable. Item descriptions are not searched.

If no direct matches are found, the mod checks for small spelling mistakes in words of four or more characters. These results are labelled as approximate, and your typed query stays unchanged.

See [search categories](docs/SEARCH-TAGS.md) for the available terms.

## Compatibility

Works with hand crafting and workstations that use Valheim's standard crafting menu. Processing stations without that menu do not receive a search field. Recipe unlocks, station requirements, crafting costs, and item quality are unchanged.

Modded items receive search categories from their equipment type, weapon skill, ammunition type, food stats, damage, and resistance metadata. Items using custom types may have fewer category matches, but their names remain searchable.

Mods that replace the crafting menu may need additional compatibility support. Mouse and keyboard are supported; controller-only search and favourite controls are not currently available.

## Reporting issues

[Open an issue](https://github.com/KakimakiDev/WorkstationSearch/issues) with your Valheim and mod versions, the workstation and tab, your search text, and what you expected to happen. Include other crafting UI mods and relevant log entries. Screenshots help with layout or selection problems.

## Development

See [building and testing](docs/BUILDING.md) for source-build instructions.

## License

[GPL-3.0-only](LICENSE.txt).
