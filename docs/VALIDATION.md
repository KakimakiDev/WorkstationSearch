# Release validation

The automated test executable covers regression checks for matching, tags, typo tolerance, ranking, favourites, persistence, and displayed row titles. These run independently of Unity, with minimal test doubles for the game metadata types. A frozen legacy alias matrix checks both matching and exclusions; adapter tests cover damage, resistance and food. CI runs those checks only; a full plugin build requires local game assemblies.

Override regression checks also cover persistence after mod removal, unrelated edits, reinstallation, per-prefab isolation, inheritance, exclusion, reset, custom words, and unknown saved category names.

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
- Search spacing, clear-button appearance and different UI scales.
- Ctrl + middle-click opens the editor without toggling favourites; ordinary middle-click still toggles favourites.
- Category scrolling, custom-word typing, Save, Cancel, Escape, and Reset at different UI scales.
- Save changes both Craft and Upgrade rows immediately, including duplicate upgrades and custom titles.
- Closing the editor does not leak Escape or typed letters into game shortcuts; crafting is blocked behind the modal.
- Saved overrides survive restart, removal of a mod, edits to another item, and reinstallation of the original mod.
- Reset clears only the selected item after Save; Cancel leaves its saved settings unchanged.

The renamed release candidate still requires this in-game pass. Prior testing-build feedback is not a claim that every mod combination or station has been verified.
