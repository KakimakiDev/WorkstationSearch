# Release validation

The automated test executable contains 65 checks for matching, tags, typo tolerance, ranking, favourites, persistence, and displayed row titles. These run independently of Unity. CI runs those checks only; a full plugin build requires local game assemblies.

Before publishing a release binary, verify:

- Fresh install and upgrade from Craft Search with only one plugin DLL present.
- Favourites survive restart under the retained configuration ID.
- Craft and Upgrade at multiple stations, including upgrade-only stations.
- `arrows`, `bolts`, `iron boots`, `breastplate`, typo fallback and no-match searches.
- Narrow `silver knife` to one result, broaden to `silver`, and confirm a single selection marker agrees with the details panel.
- Middle-click favourites, change game sorting, and verify title priority within each group.
- Duplicate upgrade items retain their correct quality, costs and selection.
- Search and clear during/after crafting; confirm no incorrect item is crafted.
- Typing responsiveness with a large modded list and no unexpected recipe-log spam.
- Custom displayed titles, including Serpent Tow's Harpoon: Karve where installed.
- Search spacing, approximate-results label, clear-button appearance and different UI scales.

The renamed release candidate still requires this in-game pass. Prior testing-build feedback is not a claim that every mod combination or station has been verified.
