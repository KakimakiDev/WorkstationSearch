# Changelog

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
