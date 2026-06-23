# Changelog

All notable changes to Elementborn are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/); the project aims for [Semantic Versioning](https://semver.org/).
`tools/bump-version.sh` rolls the `[Unreleased]` section into a dated version on release.

## [Unreleased]

### Added
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

### Fixed
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
