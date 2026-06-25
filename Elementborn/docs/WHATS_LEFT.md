# Elementborn — What's Left

**Honest status:** everything that can exist as code, shaders, tests, CI, and documentation is **done and
statically verified**. What remains is hands-on work that can only happen in the **Unity Editor**, in
**Blender / a 2D image editor**, and on **real devices** — plus optional enhancements you may or may not want.

Nothing below is missing *code* for the current design; it's the work that lives outside a code repo.

## Already done (in this repo)
- All gameplay systems: elemental combat + gacha (the character roll **and** the currency-gated **Summon
  Beacon** loop), factions & aggression, economy/ownership/taming,
  mounts & vehicles + comfort vignette, element travel, the wild bestiary, rare companions, weather +
  day/night, the HUD, interaction prompts, saving, the shop, and the defend/dash polish.
- Cel/toon shaders (lit + a vertex-color toggle, sky, water, comfort vignette) and the low-poly
  **mesh-terrain** generator + builder.
- **Audio** (`AudioController` + 20 placeholder SFX), a **settings menu** (Esc), **save slots** (F8) with
  skip-creation-on-load, **biome-blended** terrain colours, a **third-person** rig, and **all UI on
  `UiTheme`/TextMeshPro** (every screen retrofitted). See `AUDIO.md` and `PORTING.md`.
- 124 EditMode test files + PlayMode tests and a GameCI workflow, plus `tools/ip-guard.sh` + `tools/validate.sh` + `tools/doctor.sh`
  (CI gates) and a tag-driven release + docs-publish pipeline (`VERSION`, `CHANGELOG.md`, `tools/bump-version.sh`).
- Docs: this file, `INDEX.md`, `README.md`, `GETTING_STARTED.md`, `DEPLOYMENT.md`, `ART_GUIDE.md`,
  `PALETTE.md`, `UI_SPRITES.md`, `GENERATED_ART.md`, `MODELS.md`, `AUDIO.md`, `PORTING.md`, `INPUT.md`, `VR_COMBAT.md`, `ARENA.md`, `UNDERWATER.md`, `CREATURES.md`, `HIDDEN_MOVES.md`, `FACTIONS.md`, `MODDING.md`, `PLANTS.md`, `NPCS.md`, `EVOLUTION.md`, `LIMITATIONS.md`, `SOCIAL.md`, `BOOTSTRAP.md`, `NETCODE.md`, `VR_SETUP.md`, `QUESTS.md`, `ITEMS.md`, `SELF_HOSTING.md`, `PROGRESSION.md`, `WORLD.md`.

## What's left — in order

### Phase 1 — Get it running (Unity Editor) · your turn
Install Unity 6, open the project, set URP / Linear color / the new Input System / OpenXR, wire the
bootstrap scene + prefabs, and press Play in flat mode, then VR.
→ Walkthrough: **`GETTING_STARTED.md`**. Full reference: **`DEPLOYMENT.md` §2–§4**.
Skill: Unity editor basics. First wiring is a few hours.

### Phase 2 — Verify the build (CI)
Push to GitHub; the GameCI workflow compiles the project and runs the EditMode/PlayMode tests — the real
compile gate (the in-repo checks are static only). Add the Unity license secrets.
→ **`DEPLOYMENT.md` §9**.

### Phase 3 — Art pass (Blender + 2D) · your turn
Replace the code-built placeholders with low-poly, vertex-colored meshes and 2D UI sprites. This is the
largest remaining effort. **Creature models now have a runtime drop-in:** put an `.fbx`/prefab at
`Resources/Models/Creatures/<CreatureKind>` (or add a `CreatureModelNames.Aliases` line) and it's used
automatically, with the primitive placeholder as fallback — so binding a creature is a per-file step, not a code
change. See **`MODELS.md` ▸ Creature & character models**.
→ **`ART_GUIDE.md`** (object-by-object), **`PALETTE.md`** (the colors), **`UI_SPRITES.md`** (UI sizes).
Skill: Blender + an image editor.

### Phase 4 — Build & ship · your turn
Build flat PC, PCVR, and Quest (Android); sign the Android build; submit to stores.
→ **`DEPLOYMENT.md` §5–§8 and §12**. Quick checklist below.

### Phase 5 — Playtest & iterate
On-device QA, the Quest performance pass, balance, bug-fixing, polish.
→ **`DEPLOYMENT.md` §10–§11**.

## Build & ship checklist (Phase 4 quick reference)
- [ ] Flat PC build runs (fastest smoke test)
- [ ] PCVR build runs in-headset (OpenXR)
- [ ] Quest: Android module, IL2CPP + ARM64, Single Pass Instanced
- [ ] Quest: release keystore created and assigned
- [ ] Quest perf: draw calls / triangles in budget, no post-process, 72–90 Hz
- [ ] Store listings (Meta, Steam) + privacy & age ratings

(Each step is detailed in `DEPLOYMENT.md`.)

## Optional code enhancements
Most of these are now **done** (each shipped with tests/docs like the rest):
- ✅ **Audio hooks** — `AudioController` + 20 synthesized placeholder SFX in `Resources/Audio/`, mapped to
  abilities/impacts/UI; volumes via the settings store. See **`AUDIO.md`**.
- ✅ **Settings menu** — `SettingsController` (Esc): volumes, mouse sensitivity, FOV, invert-Y, comfort
  vignette; persisted; applied live to audio, the rigs, and the vignette.
- ✅ **Save slots** — `SaveSystem` is slot-aware (3 slots, back-compatible) + a `SaveSlotController` picker
  (F8) to switch/load/delete; saves now record the character + a timestamp.
- ✅ **Skip creation on load** — a finished save rebuilds the exact loadout and boots straight to the map.
- ✅ **Terrain biome-blend** — per-vertex colour blending softens biome/shoreline seams (toggle on the
  builder); faceted normals kept.
- ✅ **Third-person perspective** — `ThirdPersonRig` + a `ThirdPerson` boot mode. See **`PORTING.md`**.
- ✅ **All UI on `UiTheme`/TextMeshPro** — the foundation (`UiTheme`: TMP labels with a legacy-Text fallback,
  optional 9-slice sprites from `Resources/ElementbornUI/`, styled panels/buttons/sliders/toggles) **and**
  every screen now routes through it: the settings + save-slot menus plus the six existing controllers
  (`CharacterCreationUI`, `WorldMapView`, `GameHud`, `ShopController`, `ScoreController`, `RespawnController`).
  Buttons play the UI click; text falls back to legacy if TMP Essentials aren't imported.

- ✅ **Gamepad input scheme + rebinding** — a code-built `InputBindings` action map drives keyboard/mouse
  **and** gamepad together (left stick move, right stick look, triggers/buttons/d-pad for the rest). Every
  discrete action is **rebindable at runtime** via `RebindController` (F10 or Settings → Controls…), with
  overrides persisted to JSON. Contextual HUD prompts show **device-aware glyph tokens** that follow rebinds
  (with a brand-complete placeholder glyph set in `Art/UI/glyphs/` for Xbox/PlayStation/Switch) and a read-only
  Controls legend (F1). Movement/look sticks and the VR/XRI
  scheme are intentionally fixed. See **`INPUT.md`**.
- ✅ **VR motion combat (phase 1)** — a motion `GestureRecognizer` + per-element style table
  (`GestureProfile`) feeding a `VrGestureProvider`. A stance layer (hold-to-charge, guard, Water's ice-flow
  combo) and a dedicated arena mode (waves, dodging, combo scoring, stamina, plus Heavy/Sweep per element) are
  now in. See **`VR_COMBAT.md`** and **`ARENA.md`**.
- ✅ **Summon Beacon (gacha loop)** — a repeatable, currency-gated summon on top of the creature roster:
  two banners, **Sigils** to pull and **Motes** from duplicates, base rates with **hard pity**, a featured
  **50/50 + guarantee**, a ten-pull Epic floor, a Motes spark exchange, a **once-per-day free summon** with a **login-streak** bonus
  (UTC-midnight reset). The featured banner **rotates**
  (a themed beacon per Legendary, on a configurable day cycle; pity carries across rotations), and the Beacon
  has its **own sounds** (a cast whoosh + per-tier reveal stings). It tracks a **lifetime history** (pulls, tier
  counts, featured wins, Sigils spent / Motes earned) plus a **recent-pulls log** (last few Epic+ summons with
  ages), and **quest rewards can grant Sigils** as an in-world faucet beyond level-ups. Pure
  resolver/rotation/stats/history (`SummonRoller`, `SummonBannerCatalog`, `SummonStats`, `SummonHistory`) +
  controller + overlay (key **U** / VR hub), persisted. **Chained onboarding quests** teach the core procedures
  (channel → summon → claim-featured, and craft → equip) via new objective kinds + events and a quest-prerequisite
  system. See **`GACHA.md`** and **`QUESTS.md`**.

**Design backlog (new feature ideas — not yet built):**
- **Creature summoning spellbook** — craft an empty spellbook, then bind a creature by combining it with a drop
  of that creature's blood (which slightly lowers its friendliness / tamed level) and inscribing the creature's
  name; the finished page becomes that creature's summoning spell. Touches crafting + taming + the summon path.
- **Paralyzer (playable archetype)** — a class whose channeling can *lock out* an opponent's channeling for a
  short window; slightly more dexterous and faster than average, but weak against weapon users. New loadout/
  archetype + a "channel-locked" status that mirrors the existing stun/control statuses.
- **Custom NPC dialogue audio** — per-line voice/sound playback for NPC dialogue (an audio clip alongside each
  line, routed through `AudioController`). Touches the dialogue system + audio.

**Asset coverage (standing check):**
- **3D model prompt coverage** — confirm every model the game references (each `CreatureKind`, plus the
  `CreatureModelNames.Aliases` folder names and any other `Resources/Models/...` lookups) has a matching entry in
  `docs/MESHY_PROMPTS.md`. Anything without a prompt is effectively a "missing model zip" — there's nothing to
  generate or drop in for it yet — so it gets a prompt added to the sheet in the house style. Re-run whenever new
  creatures/models are added.

**Remaining roadmap code enhancements (optional):** none outstanding — the original roadmap's code items are all
in. What's left otherwise is genuinely Editor/Blender/device/console-SDK work below.

**Console (Xbox / PlayStation):** the code is portable and now gamepad-ready; the gated parts (licensed SDKs,
dev kits, cert) can't live here. See **`PORTING.md`**.

**More content** — extra enemies, creatures, vehicles, biomes, POIs (all data-driven via the Core catalogs).

## What genuinely can't be done outside the Editor/devices
- Creating scenes, prefabs, materials, and the Humanoid rig (editor-only assets).
- Modeling and painting meshes and UI sprites (Blender / 2D tools).
- Compiling, building players, and on-device testing (Unity Editor + hardware).
