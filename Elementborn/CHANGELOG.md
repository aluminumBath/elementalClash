# Changelog

All notable changes to Elementborn are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/); the project aims for [Semantic Versioning](https://semver.org/).
`tools/bump-version.sh` rolls the `[Unreleased]` section into a dated version on release.

## [Unreleased]

### Added
- Release pipeline: the `VERSION` file, this changelog, `tools/bump-version.sh`, `tools/validate.sh`, and the
  `release` and `docs` GitHub Actions workflows.
- `tools/ip-guard.sh` and `tools/validate.sh` wired into CI as gates (license-free, run on every push/PR).

## [0.1.0] - 2026-06-22
### Added
- Initial cross-platform scaffold from one Unity 6 (URP) codebase: VR / flat / third-person elemental combat,
  character creation with a gacha roll, and the cel-shaded look.
- A seeded open world — regions, biomes, low-poly terrain, and POI-driven structures.
- Bestiary and rare companions, mounts and vehicles, economy / taming, weather and day-night, the HUD,
  interaction prompts, and slot-aware saving.
- The arena and underwater layers, VR motion combat with a stance layer, and four hidden signature moves.
- Joinable factions, interactive plants, exotic apex creatures with two-element mix attacks, the three guide
  NPCs with Willow's sidekick menagerie, the evolution mode, and data-driven modding.
- 44 EditMode test files plus PlayMode tests, a GameCI workflow, the cel-shaded shader suite, and the full
  documentation set under `docs/`.
