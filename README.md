# Workstation Search

Search crafting and upgrade recipes, keep favourites at the top, and find equipment by category as well as name. A client-side Valheim mod for BepInEx 5.

## Features

- Search both Craft and Upgrade at stations using Valheim's standard crafting menu.
- Search localized item names, internal prefab names, and displayed titles from compatible mods.
- Middle-click recipes to toggle persistent favourites.
- Search categories such as `iron boots`, `arrows`, `helmet`, `food`, and `resist-frost cloak`.
- Title matches rank above category-only matches within favourites and other results.
- Conservative typo fallback helps with searches such as `iorn sword` when direct search finds nothing.
- Cached names and reused recipe rows avoid rebuilding the entire list on every keystroke.

## Install

Requires Valheim with BepInEx 5 installed. Install on the client, not the dedicated server.

1. Close Valheim.
2. Extract the release archive into your game or mod profile, merging its `BepInEx` folder.
3. Launch the modded game and open a crafting station.

If upgrading from a testing build named **Craft Search**, remove the old `CraftSearch.dll` before installing `WorkstationSearch.dll`. Do not load both. The plugin retains `local.valheim.craftsearch` as its internal ID so existing favourites continue to work.

Remove `WorkstationSearch.dll` to uninstall.

## Controls

| Action | Control |
| --- | --- |
| Filter recipes | Click the search field and type |
| Clear search | Click X |
| Finish typing | Enter or Escape |
| Toggle favourite | Middle-click a recipe |

Closing inventory clears the query. Switching Craft/Upgrade keeps it. Search changes wait for an active craft or upgrade to finish; favourite toggles are ignored during that operation.

Favourites are saved by item type, shared across Craft/Upgrade and characters in the same mod profile. Configuration is stored in `BepInEx/config/local.valheim.craftsearch.cfg`.

## Matching and ordering

Every search word must match a name or category. Names accept partial words; category aliases match whole words. Matching ignores case and accents. Descriptions are not searched.

Matching favourites appear first. Within each group, items matching more search words in their visible title rank higher, with exact full-title matches winning ties. Equal matches retain the game's sorting order.

If there are no direct results in the current tab, words of four or more characters allow one insertion, deletion, substitution, or adjacent letter swap. Approximate results are labelled. Shorter terms remain strict. The mod never changes the typed query.

See [search categories](docs/SEARCH-TAGS.md) for aliases and limits.

## Compatibility

Works through the standard InventoryGui recipe menu, including hand crafting and compatible modded stations. It preserves recipe unlocks, station requirements, costs and item quality. Processing stations without that menu do not receive a search field.

Mods that replace the crafting UI or add rows after this mod's final list hook require separate compatibility testing. Custom row titles are captured when the list is built. Items with custom equipment slots require additional category mappings. Mouse/keyboard search and middle-click favourites are supported; controller-only text entry and favourite controls are not implemented.

No online service, downloaded dictionary, or third-party fuzzy-search library is used.

## Build and test

Use Windows with the .NET 8 SDK and .NET Framework 4.8 targeting pack. Game and BepInEx assemblies must come from your own installation; they are not included in this repository.

```powershell
./build.ps1 -GameManaged 'D:/SteamLibrary/steamapps/common/Valheim/valheim_Data/Managed' -BepInExCore 'D:/ValheimProfile/BepInEx/core'
```

The script runs the pure search tests and builds the plugin. To run only the tests, including on a machine without Valheim:

```powershell
dotnet run --project tests/WorkstationSearch.Tests.csproj -c Release
```

Package a built plugin with `./package.ps1`. The output is a manual-install ZIP under `artifacts`. This is not yet a Thunderstore submission package.

Version 0.5.0 is the initial Workstation Search release candidate, renamed from Craft Search 0.4.4. Automated checks do not replace in-game testing. See [validation](docs/VALIDATION.md) for release checks and limitations.

## Reporting issues

Include your Valheim and mod versions, the station/tab, query, expected result, other UI mods, and relevant log entries. A screenshot is useful for layout or selection issues. Review logs before sharing personal details.

## License

GPL-3.0-only. See [LICENSE.txt](LICENSE.txt). Valheim, Unity and BepInEx dependencies remain under their respective licences and are not bundled with the source repository.
