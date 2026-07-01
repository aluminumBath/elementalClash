# Elementborn — What's Left

**Honest status:** everything that can exist as code, shaders, tests, CI, and documentation is **done and
statically verified**. What remains is hands-on work that can only happen in the **Unity Editor**, in
**Blender / a 2D image editor**, and on **real devices** — plus optional enhancements you may or may not want.

Nothing below is missing *code* for the current design; it's the work that lives outside a code repo.

## Already done (in this repo)
- All gameplay systems: elemental combat + gacha roll, factions & aggression, economy/ownership/taming,
  mounts & vehicles + comfort vignette, element travel, the wild bestiary, rare companions, weather +
  day/night, the HUD, interaction prompts, saving, the shop, and the defend/dash polish.
- Cel/toon shaders (lit + a vertex-color toggle, sky, water, comfort vignette) and the low-poly
  **mesh-terrain** generator + builder.
- 26 EditMode test files + PlayMode tests and a GameCI workflow.
- Docs: this file, `INDEX.md`, `README.md`, `GETTING_STARTED.md`, `DEPLOYMENT.md`, `ART_GUIDE.md`,
  `PALETTE.md`, `UI_SPRITES.md`.

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
largest remaining effort.
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

## Optional code enhancements (I can build these on request)
Not required for the current design, but the natural next code steps — each would ship with tests + docs
like everything else:
- **Wire UI sprites + TextMeshPro** into the six UI controllers (serialized sprite/font fields + slicing),
  per `UI_SPRITES.md`.
- **Terrain biome-blend** — soft color transitions at biome borders on the mesh terrain.
- **Skip creation on load** — remember a created character so returning players load straight into the world.
- **Audio hooks** — footsteps, ability SFX, ambient, music.
- **Settings menu** — turn/snap, comfort-vignette strength, volume, graphics.
- **Save slots / autosave interval / multiple characters.**
- **More content** — extra enemies, creatures, vehicles, biomes, POIs (all data-driven via the Core catalogs).

Tell me which and I'll add them.

## What genuinely can't be done outside the Editor/devices
- Creating scenes, prefabs, materials, and the Humanoid rig (editor-only assets).
- Modeling and painting meshes and UI sprites (Blender / 2D tools).
- Compiling, building players, and on-device testing (Unity Editor + hardware).
