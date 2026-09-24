# Changelog

## 0.6.3

- Increase mouse-wheel scrolling speed in the classification menu.

## 0.6.2

- Replace category text markers with clickable checkbox controls.

- Assign Valheim fonts before text components activate, preventing missing LiberationSans font warnings when opening the category editor.

## 0.6.1

- Remove the approximate-matches label while keeping typo fallback.

## 0.6.0

- Add Ctrl + middle-click to edit an item type's search categories and custom search words.
- Support disabling automatic categories, adding categories with inherited aliases, and resetting an item to automatic detection.
- Save overrides per prefab in the local profile, retaining entries when mods or items are removed.
- Refresh matching recipe rows after saving while preserving custom titles, favourites and title ranking.
- Block crafting shortcuts while the editor is open and keep edits separate until Save.

## 0.5.1 - Release candidate

- Classify metadata using typed game enums and compact category IDs.
- Define shared armour, ammunition, accessory and magic relationships once.
- Separate English synonyms and explicit plural/spelling forms from item metadata.
- Resolve category words once per query term, including shared typo candidates only when needed.
- Preserve existing aliases, title ranking, favourites, partial-name search and direct-before-fuzzy matching.
- Add vocabulary compatibility and metadata classification regression checks.

## 0.5.0 - Release candidate

- Rename Craft Search to Workstation Search, retaining the plugin ID and favourite configuration.
- Search Craft and Upgrade recipes by visible title, localized name, prefab name, and cached metadata aliases.
- Add persistent middle-click favourites and title-priority result ordering.
- Add conservative offline typo fallback and ammunition singular/plural aliases.
- Reuse recipe rows while typing and synchronize selection markers across hidden and visible rows.
- Support custom row titles such as Harpoon: Karve.
- Add portable build/package scripts and automated pure-search checks.

Earlier 0.1 through 0.4.4 versions were local Craft Search testing builds.
