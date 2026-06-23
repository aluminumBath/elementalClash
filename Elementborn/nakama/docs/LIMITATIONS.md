# Limitations & known gaps

An honest catalogue of what **can't** be produced from this code-only environment, plus a concrete starter fix
for each. Everything that *can* exist as code, shaders, tests, CI, and docs is done; the items below need an
artist, a device, the Unity Editor, a platform program, or a design decision. This is engineering hygiene, not
legal or clinical advice.

## Hard external blockers

### 1. Organic 3D models
**Missing:** Creatures (the dragons, mermaid, the exotic apex set, roc/thunderbird, the octopus and serpent
bosses, the sea creatures), playable characters, the three guide NPCs and Willow's companions (Gunnar, the
parrot, blobfish, mushroom, chameleon), the plants (snaptrap, vines, the lilies), buildings, and weapons. All
are faceted placeholder prefabs or a shared reused prefab today.
**Why not here:** Modeling, rigging, and skinning organic meshes needs a DCC tool (Blender/Maya) and an artist;
it isn't deterministic code.
**Starter fix:** Model and rig in Blender — the project already imports `.ply`/`.fbx` with vertex colors and a
humanoid rig mapping (see `ART_GUIDE.md`, `MODELS.md`). Do the player and the most-seen creatures first.
`MountSummoner`, `CompanionSummoner`, and `FishSpawner` already accept a real prefab and fall back to a
placeholder, so finished assets drop in with **no code change**. An asset-store base mesh or a text-to-3D
generator is a reasonable starting block to retopologize and hand-finish.

### 2. Concept art & textures (2D)
**Missing:** The generated sprites, glyphs, and palette are flat synthetic placeholders. No painted concept art,
character portraits, or hand-painted/PBR textures.
**Why not here:** Painting and texture work needs an artist (or image generation plus heavy hand-finishing).
**Starter fix:** Concept, then hand-finish; author textures in Substance/Krita/Photoshop. Keep `ToonPalette` as
the canonical color source so new art stays on-model.

### 3. Real water rendering
**Missing:** Water is the stylised `ToonWater` shader on a flat plane plus tinted underwater fog
(`WaterVolume`). No reflections, refraction, depth caustics, shoreline foam meshes, or physical buoyancy.
**Why not here:** True water needs reflection/refraction render passes and per-scene tuning in the Editor;
buoyancy needs physics and scene setup.
**Starter fix:** Reflections via URP planar reflections (a reflection camera to a `RenderTexture`) or SSR;
refraction by sampling `_CameraOpaqueTexture`; add depth-based color and caustics. For buoyancy, sample the
known water height (already tracked by `WaterSurface`/`WaterVolume`) and apply an Archimedes-style force in
`FixedUpdate`.

### 4. Console SDKs & ports
**Missing:** Xbox / PlayStation / Switch builds.
**Why not here:** Console SDKs are gated behind ID@Xbox, PlayStation Partners, and the Nintendo Developer
program, plus NDA'd Unity console modules, dev-kit hardware, and certification — none obtainable or runnable in
this container (see `PORTING.md`).
**Starter fix:** Apply to the platform programs; once approved, install the NDA'd Unity console module and build
on a dev kit. The code is already portable, and consoles ship the **flat/third-person** build, not VR. Budget
for cert requirements (suspend/resume, controller disconnect, storage handling).

### 5. On-headset VR tuning
**Missing:** Comfort vignette strength, locomotion speed and turn style, gesture-recognition thresholds, and
interaction reach are sensible defaults, not validated on a headset.
**Why not here:** Needs a real Quest/PCVR device and human playtesting; can't be simulated here.
**Starter fix:** Sideload to a Quest (see `DEPLOYMENT.md`), playtest, and tune the already-exposed settings
(`Settings`/`SettingsController`) and gesture thresholds (`GestureProfile`/`StanceResolver`). Default to
teleport plus snap-turn and watch for motion sickness.

### 6. Unity scenes, prefabs, materials, the rig, the Input Actions asset — MOSTLY MITIGATED
**Missing:** Editor-authored binaries aren't hand-writable as text here: `.unity` scenes, `.prefab`s, `.mat`
materials, the Humanoid rig, and a serialized `.inputactions` asset.
**Now provided:** A generator builds the scene and rig prefabs for you — run **Elementborn ▸ Bootstrap** (see
`BOOTSTRAP.md`). It creates first-person/third-person/VR rigs with the real components wired and a playable
`Bootstrap.unity` carrying `GameBootstrap` + `GameFlowController`. Press Play and the flat/third-person path runs
end to end.
**Still an Editor step:** a real **character model** (the rig
body is a capsule), the **VR** XR-plugin + controller poses, and — if you want one — a serialized
`.inputactions` asset (bindings already exist in code via `InputBindings`). Cel-shaded **materials** are now
generated too — **Elementborn ▸ Materials ▸ Build Cel-Shaded Materials** (see `BOOTSTRAP.md`).

## In-code gaps (fixable — flagged for a decision)

### 7. Shared Interact button & HUD prompt ownership — RESOLVED
Implemented as `InteractionArbiter` + `IInteractable` (a pull model): each interactable offers an interaction
(distance, priority, verb, action) given the player's position; the arbiter collects offers, picks the best
(highest priority, ties to nearest), owns the single HUD prompt, and routes one Interact press. The five former
pollers — `PlayerInteractor`, `PlantControlController`, `GuideNpcController`, `SidekickFeedPoint`,
`ParfaFrogController` — are now `IInteractable` providers and no longer touch input or the HUD. `PickBest` is
unit-tested (`InteractionArbiterTests`). `PlayerInteractor` adds an `InteractionArbiter` to the rig
automatically.

### 8. `FindObjectOfType` at startup (minor perf)
**Issue:** ~22 `FindObjectOfType` calls, mostly in `Awake`/`Start` caching.
**Starter fix:** Cache via serialized references or a small service locator; in Unity 6 prefer
`FindFirstObjectByType`/`FindAnyObjectByType`. Low priority — these run at startup, not per frame.

## Content-depth follow-ups (designed, intentionally shallow)

- **Mode picker at creation** (Standard vs Evolution + starting element) and pacing for when the second element
  unlocks — `EvolutionController` is the mechanism they'd drive.
- **Predator aggro & the full creature mix-attack AI loop** (telegraphs/phases; rider firing the mount's mix
  attack).
- **Shop UI** (Willow sells / Parfa buys are service text today), **dialogue/quest UI** (NPC lines surface via
  event + log), a **food-item inventory** (sidekick foods are flavour), and full **hand-over-hand VR climbing**.
