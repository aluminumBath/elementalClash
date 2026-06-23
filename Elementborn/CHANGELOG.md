# Changelog

All notable changes to Elementborn are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/); the project aims for [Semantic Versioning](https://semver.org/).
`tools/bump-version.sh` rolls the `[Unreleased]` section into a dated version on release.

## [Unreleased]

### Added
- **Map systems (Core)**: `MapNavigation` — **leyline-rift fast travel** (`FastTravelNetwork`: warp only to
  discovered rifts, nearest-rift, savable), **locate self** always (`Locator.Self`) and **locate friends** only
  with explicit opt-in (`LocationSharing`, off by default; `Locator.VisibleFriends` is consent-gated), plus
  **minimap math** (`Minimap.WorldToNormalized` / `WithinRange`) and `MapMarker`s. Pure rules + tests. **Now wired
  in-world**: `MapState` (runtime owner, seeded from a canonical `WorldMap` of seven rifts; persisted via
  `SaveData.discoveredRifts`/`shareLocation` through `PlayerInventory`), an always-on `MinimapHud`, a full
  `MapViewerController` (key **M**, also opened from a rift) drawing the overworld backdrop with tap-to-fast-travel,
  `LeylineRiftObject`/`LeylineRiftSpawner` (discover-on-approach crystals via the `InteractionArbiter`), and a
  shared `RigTeleporter` (the safe respawn-style warp). The overworld key-art is installed at
  `Resources/ElementbornUI/worldmap`. Remaining: a live friend-position feed (Nakama presence) — the consent-gated
  path and local opt-in are wired, but positions aren't pushed yet.
- **The Grimoire** — a discovery-driven tome with three sections that fill in as the player uncovers them:
  **Bestiary** (every creature, composed from `CreatureCatalog`/`CreatureHints`), **Attacks** (each element ×
  the moveset), and **Bloodlines** (a new `Bloodlines` catalog: the four base lines, the sub-art lines, the
  Confluence, and the rare **Dragonthorn** line — Ash's). Each entry reveals in tiers (`DiscoveryTier`:
  Unknown → Glimpsed → Known → Mastered) via `Grimoire.Redact`; the player's `GrimoireProgress` is savable and
  never downgrades. Core engine + catalogs are complete and unit-tested. **Now wired in-world**: a
  `GrimoireController` overlay (default key **G**, maroon-and-gold tome, three tabs with discovered/total counts,
  redacted entries), discovery hooks via the `QuestEvents` bus (a new `CreatureSighted` proximity event →
  Glimpsed, `CreatureDefeated` → Known, `CreatureTamed` → Mastered; a new `AbilityCast` event from
  `PlayerCombatController` → the matching Attacks entry + a glimpse of that element's base bloodline; carried
  lines/sub-arts/Confluence glimpsed on open), and save persistence folded into `PlayerInventory` (`SaveData`
  `grimoireKeys`/`grimoireTiers`). Remaining: bloodline **Mastered** via meeting a notable bearer; a shared VR
  opener (see `GRIMOIRE.md` / `VR_INPUT_MAP.md`).
- **Owner admin + signature hero (Ash Shadowthorn)**: `SignatureCharacter` (Core) defines the owner's hero — a
  three-element Channeler (Air/Water/Fire with Flight/Sanguine Grip/Magmacraft), a `DragonForm`, teal eyes /
  reddish-brown hair / Texan accent, and three named companions: **Apollo** (kitsune), **Artemis** (shadowhound,
  shadow-teleport via blink), **Iago** (iridescent phoenix that mirrors the hero's elements and is reborn once).
  `Social.AdminAccounts` treats any Gmail alias of the owner's address as a global `UserRole.Admin` (dots and
  +suffixes normalised), and can provision the owner into a `UserDirectory`. Unit-tested.
- **Weapons / stones / wands (Core stub)**: `Armory` (Core) — elemental **stones** (dropped by defeated
  Channelers and in each capitol throne room), irreversible **two-at-a-time fusion**, a capitol **weaponsmith**
  that either **imbues** a weapon (+damage/+durability + an element effect; fused imbues harder) or **forges a
  wand**, and **wands** that grant elemental spells, block other item slots, and cap at 6 spells (3 per element).
  Pure rules + tests; in-world wiring (loot drops, throne-room stones, smith NPC, equip enforcement) is staged in
  `WEAPONS.md`.
- **In-App Purchasing** (`com.unity.purchasing` 5.1.1 — the version Unity ships for 6000.5) added to the package
  manifest so real-money purchasing is available for future monetization. Not yet wired into any game system; the
  economy still runs entirely on soft currency (Silver) + gacha. IAP v5 is a breaking API change from v4, so any
  previously-imported 4.15.x sample scripts must be removed (or re-imported for v5).
- **Ability ladder** (`AbilityUnlocks`, Core): the combat kit unlocks with player level — Primary + Defend from
  the start, Sweep at 2, Heavy at 4, and the Secondary/Signature casts at 6. The combat controller gates each
  intent (a locked intent no-ops with a toast); Channelers and weapon users share one intent-keyed table, so the
  ladder applies to both kits on the same levels. A level-up announces new unlocks. Gating is skipped where no
  `ProgressionController` is present (e.g. the Arena). All numbers live in one table; unit-tested
  (`AbilityUnlocksTests`, +1 → 56 EditMode test files).
- **Per-element Sweep arc**: Sweep is now a wide, multi-target arc (`OutcomeKind.Sweep`) instead of a single
  projectile — every enemy in a 120° / 3.5 m fan in front is hit at once. Each element carries a distinct rider:
  Fire burns, Water shoves + slows, Earth shoves + briefly staggers (control), Air is pure max-knockback
  displacement. The cone math is the pure `SweepArc`; `SweepController` (added to both rigs in the bootstrap) is
  the presentation shell that overlaps, filters to the cone, dedupes, and hits everyone caught. Unit-tested
  (`SweepArcTests`, `SweepMovesetTests`, +2 → 58 EditMode test files). Documented in `VR_COMBAT.md`.
- **Per-element Heavy impact**: Heavy is now a committed **impact zone at range** (`OutcomeKind.Heavy`) — it
  lands 3 m in front of the caster and hits everything within a 2 m blast, knocking targets outward. A distinct
  third shape alongside Sweep (wide/near) and Primary (single travelling shot). Riders preserved per element:
  Fire meteor (burn on Magmacraft), Water geyser (slow), Earth ground slam (metal on Oreshaping), Air updraft
  (knock-up). Blast math is the pure `HeavyStrike`; `HeavyController` is the presentation shell. Unit-tested
  (`HeavyStrikeTests`, `HeavyMovesetTests`, +2 → 60 EditMode test files). Documented in `VR_COMBAT.md`.
- **Sweep/Heavy VFX + Heavy telegraph + charge scaling**:
  - **Sweep** now throws a fast forward fan of particles on cast (`AbilityFx.SpawnSweepFan`); still instant.
  - **Heavy** is now **telegraphed** — it marks a ground ring at a fixed impact point, waits
    `HeavyStrike.TelegraphSeconds` (0.5 s), then drops the blast, so it's dodgeable (step out of the ring and
    you're spared). The hit is resolved *after* the wind-up.
  - **Charge scales Heavy's blast**: holding the cast grows both the damage (already) and the impact radius
    (`HeavyStrike.RadiusForCharge`), and the telegraph ring grows to match. `AbilityOutcome` now carries an
    optional `Charge` (source-compatible; preserved through `Scaled`).
  - New `AbilityPalette` (Core) gives the shared colour language — element primary, sub-art as the
    secondary/accent — and `AbilityFx` (Game) builds the procedural, self-cleaning fan / ring / impact burst
    (no prefab assets needed; appearance is a placeholder pending a hand-made VFX pass).
  - +1 test (`AbilityPaletteTests`) and charge/telegraph cases added to the Heavy tests → 61 EditMode test files.
    Documented in `VR_COMBAT.md`.
- **Heavy now arcs in with a filling telegraph ring** (replaces land-after-delay): on cast the strike launches on
  a parabolic arc (`HeavyStrike.ArcPoint`) toward the fixed impact point while a ground ring fills like a clock
  over the flight, and the blast resolves on landing — still dodgeable, now legible. `AbilityFx` gained the
  filling ring (`SpawnTelegraphRing` + `SetRingFill`) and the travelling projectile (`SpawnHeavyTravel`); the arc
  path is unit-tested (`HeavyStrikeTests`).
- **`docs/VR_INPUT_MAP.md`** — an audit of VR vs desktop input. All combat + locomotion is bound in VR; the
  unmapped controls (Interact, the Quest/Inventory/Social/Character/Settings/Slots overlays, Element travel,
  Mount, Companion) are listed, along with a real binding **conflict** (right **A** drives both Dash and
  Recenter). Indexed in `INDEX.md`.
- **Dialogue & action coverage tests** (`DialogueAndActionCoverageTests`): every guide has a full spoken profile
  (greeting + service line + home/sidekick/appearance + valid element), every `NpcRole` is represented, every
  creature has a finding hint, and every quest is fully authored (title/summary/giver/objectives/reward). +1 →
  62 EditMode test files. (Enemy stats/actions were already covered by `EnemyArchetypesTests`.)
- **Placeholder/bug sweep**: no `TODO`/`FIXME`/`NotImplementedException`/empty-catch/stubbed logic anywhere; the
  only "placeholders" are intentional procedural meshes/VFX awaiting an art pass.

### Fixed
- **VR input conflict**: right controller **A** drove both Dash (combat) and Recenter (locomotion), so a press
  fired both. Recenter now lives on the **right thumbstick-click** (`primary2DAxisClick`) in `VrComfortLocomotion`,
  leaving A as Dash-only. `VR_INPUT_MAP.md` updated.
- Compile errors surfaced on import (Unity 6000.5.0f1):
  - `ScoreController` now exposes `EnsureInstance()` (mirrors `StaminaController`) — fixes `ArenaController`.
  - `IceTrap` imports `Elementborn.Core`, resolving `StatusEffect` / `StatusKind`.
  - `FactionMember`'s element field initializer is namespace-qualified so it binds to the enum, not the
    same-named `Element` property.
  - `GuideNpcController` uses a struct-safe sentinel (`string.IsNullOrEmpty(_info.Name)`) instead of comparing
    the `GuideNpcInfo` value type to null.
  - `SwimLocomotion` qualifies the XR `CommonUsages`, resolving the InputSystem/XR ambiguity.
  - `UiTheme` fully-qualifies `UnityEngine.UI.Slider` so the type isn't shadowed by the `Slider(...)` factory.
  - `UnderwaterTests` adds `using UnityEngine;` for `Vector3`.
  - `HealthTests` passes a concrete `Element` (neutral `Earth`) to `DamageInfo` instead of `null` — the
    element parameter is a non-nullable enum.
- `QuestController.Start(string)` → `StartQuest`: on a MonoBehaviour, `Start` is a reserved Unity lifecycle name
  and can't take parameters (Unity logged "Start() can not take parameters"). Caller in `DialogueController`
  updated; the pure `QuestLog.Start` is unaffected.
- Removed `com.unity.modules.vr` from the manifest: the legacy built-in VR module is deprecated and unavailable
  in Unity 6000.5 ("Package [com.unity.modules.vr@1.0.0] cannot be found"). The project's VR uses `UnityEngine.XR`
  (XRNode / InputDevices / CommonUsages) from `com.unity.modules.xr`, not legacy VR, so nothing depends on it. The
  doctor's XR-module invariant now requires only `com.unity.modules.xr`.
- **Combat presentation wiring**: the generated player rigs were missing every `OutcomeReady` listener except
  the one just added for Sweep, so most casts resolved but presented nothing. The bootstrap now adds the full
  set to both the flat and VR rigs — `AbilityVfxBinder` (Projectile: procedural visuals + audio + a damaging
  projectile), `MeleeController` (Melee), `SweepController` (Sweep), `HeavyController` (Heavy),
  `BarrierResponder` (Defend shield), `DashResponder` (Dash/Flight), `SanguineGripController` (Control). Each
  self-wires, so no manual references are needed; visuals are procedural placeholders pending an art pass.

### Changed
- **Package upgrades** (Unity 6000.5): Input System 1.11.2 → 1.19.0, XR Interaction Toolkit 3.0.7 → 3.5.1,
  XR Management 4.5.0 → 4.5.4, OpenXR 1.13.0 → 1.17.1.
- IP guard hardened: `ip-guard.sh` and the doctor's standing grep now catch the bare verb "bend"/"bends",
  closing the gap that let Channel-convention stragglers slip past. Fixed three: a player-facing
  "Bend an Element" button (→ "Channel an Element") and two doc comments.

## [0.2.0] - 2026-06-23

### Added
- **Online social layer** (backend-agnostic): identity + roles, notifications, feedback-to-admin, friends,
  session invites, direct/session messaging, and role-enforced moderation — all behind seven Core seams with an
  in-memory `LocalSocialBackend`, so it runs and unit-tests offline. `SocialHub` is the single access point.
- **Social UI** (`SocialMenuController`): a toggled in-game overlay (key J) for notifications, friends + invites,
  session chat, a feedback form, and a moderation panel, plus a reusable `UiTheme.Input` text field. Now added to
  the generated bootstrap scene.
- **Interaction arbiter** (`InteractionArbiter` + `IInteractable`): one owner of the Interact button and prompt;
  the five former pollers (world interactor, NPCs, sidekicks, plant control, the frogs) became offerers, ending
  the shared-button conflict.
- **Editor bootstrap generator**: one-click build of a playable scene + first-person / third-person / VR rig
  prefabs (Elementborn ▸ Bootstrap).
- **Material generator**: cel-shaded `.mat` assets from the toon shaders (Elementborn ▸ Materials), auto-applied
  to the generated rigs and ground, with a `SceneStyleController` for sky/ambient/fog in the scene.
- **Netcode (Nakama)**: client adapters for all seven seams behind the `ELEMENTBORN_NAKAMA` define, a server
  runtime module (feedback-to-admin, trusted notify, authoritative sessions, and a ban-gated match join), a local
  `docker-compose`, and the build/load setup.
- **VR comfort locomotion** (`VrComfortLocomotion`): snap/smooth turn, stick movement, recenter and height reset,
  driving the existing comfort vignette; the generated VR rig is now an XR-origin shape.
- **Project doctor** (`tools/doctor.sh`): one command asserting every invariant — gates, IP, `#if` balance,
  asmdef validity, the Element enum, retired tokens, and doc/test-count sync.
- Release pipeline: the `VERSION` file, this changelog, `tools/bump-version.sh`, `tools/validate.sh`, and the
  `release` and `docs` GitHub Actions workflows; `tools/ip-guard.sh` + `tools/validate.sh` wired into CI.
- Docs: `BOOTSTRAP.md`, `NETCODE.md`, `VR_SETUP.md`, `SOCIAL.md`, `LIMITATIONS.md`.

### Changed
- `SocialHub` builds its services from a swappable `ISocialBackend` (local by default, Nakama under the define).
- `Health.SetMax` resizes in place instead of replacing the object, so external Damaged/Died subscribers survive.
- Faction "Republicers" renamed to "Synodists"; a trademark sweep across shaders and docs; `Notification.MarkRead()`
  made public so backend stores can mark read.
- EditMode test files: 44 → 50 (social phases, the backend swap, the arbiter, cross-service hardening).

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
