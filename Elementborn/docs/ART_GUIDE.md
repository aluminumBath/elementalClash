# Elementborn — Low-Poly Art & Texture Guide

**Pipeline: Blender + flat vertex colors + the `Elementborn/ToonLit` cel shader.**

This guide replaces every code-built placeholder in the project with hand-made low-poly art.
It assumes the cheapest, most Wind-Waker-friendly workflow: model a clean low-poly shape, paint
**flat per-face colors** directly into the mesh (no UV unwrapping, no texture painting), and let
the toon shader do the shading and the outline. Work top-to-bottom or jump to the object you need;
§9 suggests an order, and §10 maps every asset to the script/prefab slot it plugs into.

---

## 0. The look, and why vertex colors

The art style is **low-poly silhouette + flat color + cel shading**. The model itself stays flat
and unlit-looking in Blender; the shader adds the banded light, the cool shadow tint, and the black
inverted-hull outline at runtime.

Flat vertex colors are the ideal fit here because:

- **No UVs, no textures.** Color lives in the mesh's color attribute. You never unwrap or paint a map.
- **One material for everything.** Every vertex-colored mesh can share a single `ToonLit` material,
  which means the `MeshCombiner` can merge whole structures into a handful of draw calls — exactly
  what a standalone Quest wants.
- **Tiny memory.** Vertex colors cost almost nothing next to textures.

**The golden rule:** model the shape, flood faces with flat colors, and let the shader light it.
Never bake lighting, gradients, or ambient occlusion into the mesh — the toon ramp expects flat input.

### The shader is ready for this

`Elementborn/ToonLit` now has a **"Use Vertex Colors"** toggle (off by default, so existing
placeholder materials are unchanged). When you make real art, turn it on: the shader multiplies the
painted vertex color into the albedo. Keep **Base Color = white** so the vertex color shows true.
Banding (Ramp Steps), Shadow Tint, Rim, and Outline all come from the material, not the mesh.

---

## 1. One-time setup

### 1.1 Blender scene & scale

- Scene Properties → Units: **Metric, Unit Scale 1.0**. One Blender meter equals one Unity meter.
- Model to real scale: a person is ~1.8 m tall, a door ~2 m, a rowboat ~3 m long.
- **Shade Flat** (Object → Shade Flat). Low-poly toon wants faceted normals so each face takes one
  band of light. Use custom split normals only where you deliberately want a face to read smooth.

### 1.2 The vertex-color workflow (the core skill)

1. Select the mesh → Object Data Properties → **Color Attributes** → **+**.
   Set **Domain = Face Corner** and **Data Type = Byte Color**. Name it `Col`.
   (Face-corner data lets two touching faces hold hard, different colors — essential for flat-color art.)
2. Go to **Edit Mode**, switch to **Face select**, and select the faces you want in one color.
3. Switch to **Vertex Paint** mode, enable the **Face selection masking** toggle in the header, pick
   your brush color, then **Paint → Set Vertex Colors** (`Shift+K`) to flood the selected faces.
4. Repeat per color region. A whole prop is usually 3–6 flooded color groups.

Keep a small, fixed palette and reuse it across everything for cohesion. The shaders already define
the world's anchor hues you can sample from: sky top `(0.20, 0.45, 0.85)`, horizon
`(0.72, 0.86, 0.95)`, deep water `(0.06, 0.28, 0.45)`, shallow water `(0.30, 0.70, 0.74)`, foam white,
warm sun `(1.0, 0.96, 0.82)`. Pick element colors and stick to them: fire warm red/orange, water
teal/blue, earth brown/green, air pale blue/white, ice cyan, lightning yellow.

A ready-made **starter palette** of all these (32 colors, matching `TerrainColors` and the shaders) is in
`palette/` — run `elementborn_palette_blender.py` to load a native Blender palette, or eyedrop from
`elementborn_palette.png`. See `docs/PALETTE.md` for the full table and import steps.

### 1.3 Clean up before export

- **Recalculate normals outside** (Edit Mode → Mesh → Normals → Recalculate Outside). The outline
  pass inflates the mesh **along its normals**, so inverted or inconsistent normals produce broken
  outlines — this is the #1 cause of ugly outlines.
- **Apply all transforms** (Object → Apply → All Transforms) so scale is 1 and rotation 0 in Unity.
- Set the **origin** where it should pivot: base-center for buildings/props, hips for characters,
  waterline-center for boats.

### 1.4 Export & import

- **Export:** File → Export → **FBX** (or glTF 2.0; both carry vertex colors). For FBX use
  Forward `-Z`, Up `+Y`, apply scalings, and export **Mesh** (props) or **Armature + Mesh**
  (characters). Vertex colors export automatically with the color attribute.
- **Import in Unity:** select the asset, set **Scale Factor = 1**, Generate Colliders only for static
  scenery you want collidable. For characters set **Rig → Animation Type = Humanoid** and configure
  the humanoid rig mapping. Unity's FBX importer brings vertex colors in automatically.

### 1.5 The shared material

- Create one Material, **Shader = `Elementborn/ToonLit`**, enable **Use Vertex Colors**, set
  **Base Color = white**, **Outline Color** dark, **Outline Width ≈ 0.012**, **Ramp Steps = 2–3**,
  and a cool **Shadow Tint**. Reuse this single material across all vertex-colored meshes.

### 1.6 The per-object checklist (use this for every section below)

1. Block the silhouette from primitives; keep subdivisions low.
2. Stay within the poly budget (§8).
3. Shade Flat; recalculate normals outside.
4. Add the `Col` face-corner color attribute; flood faces with flat colors.
5. Apply all transforms; set the pivot/origin.
6. Export FBX (vertex colors included).
7. Import; assign the shared `ToonLit` material (Use Vertex Colors on).
8. Add the collider/rig the placeholder used (see the prefab note in §3).
9. Drop it into the prefab slot the controller references (§10) and test in Play mode.

---

## 2. Environment

### 2.1 Terrain

There are now two terrain components — pick one on your world object:

- **`MeshTerrainBuilder` (recommended for the look).** A drop-in that builds a **faceted, vertex-colored
  low-poly mesh** from the same `TerrainGenerator` heights/biomes and renders it with `ToonLit`, so it
  gets the cel bands **and the outline** a Unity Terrain can't. The mesh, its per-chunk colliders, and
  the per-biome face colors are all generated for you. Your only terrain "art" job is **tuning the
  palette** in `TerrainColors.ForBiome` (Core) and, optionally, hand-sculpting afterward. Keep
  `meshResolution` coarse (≈65–129) for the low-poly facets, and set `terrainSize` to your world's
  MapSize. Assign it to `GameFlowController`'s mesh-terrain slot **instead of** the Unity `TerrainBuilder`.
- **`TerrainBuilder` (Unity Terrain).** Keeps a standard Unity Terrain and paints flat solid-color
  `TerrainLayer`s per biome. Unity Terrain uses its own shader, so `ToonLit` and the outline don't apply,
  but flat color layers still read as stylized. Use this if you prefer Unity's sculpting/LOD tooling.

Ground snapping for spawns, structures, mounts, and creatures works with **either** path — everything
samples height through `TerrainHeight`, which prefers the mesh terrain and falls back to Unity Terrain.

Either way, color by biome with your fixed palette (§1.2) and keep height contrast gentle and readable.

### 2.2 Water

`WaterSurface` builds a plane with the `ToonWater` shader — **no modeling needed**, just tuning:

- Colors: `_ShallowColor`, `_DeepColor`, `_FoamColor`.
- Foam: `_FoamBands`, `_FoamWidth`, `_FoamSpeed`.
- Waves: `_WaveAmp`, `_WaveFreq`, `_WaveSpeed` (subdivide the plane a little so vertex waves show).
- Banding/sparkle: `_LightBands`, `_Sparkle`.

### 2.3 Sky

`ToonSky` is fully procedural (no mesh). Tune `_TopColor`, `_HorizonColor`, `_GroundColor`,
`_SunColor`/`_SunSharpness`, and the clouds (`_CloudColor`, `_CloudAmount`, `_CloudScale`,
`_CloudSpeed`). `DayNightCycle` drives `_SunDirection` over time, so pick colors that read well from
dawn to dusk. Match the horizon color to your terrain for a seamless join.

### 2.4 Buildings & structures (per `StructureKind`)

`StructureGenerator` emits box "parts" per `StructureKind`, `StructureBuilder` assembles them, and the
`MeshCombiner` merges each structure into few draw calls. To replace the box placeholders, model one
low-poly building per kind, vertex-colored in a few groups (walls / roof / trim). Generic workflow:
walls from a beveled cube, an inset or extruded gable roof, a door/window as differently-colored
faces, optional trim. Keep the pivot at the base center. Per-kind cues:

- **Village** — small gabled cottage; warm walls, dark roof.
- **City** — taller stone block; banners, more windows.
- **Temple / Shrine** — stepped base, columns, gold trim, a colored element glyph face.
- **Dungeon** — dark stone arch/entrance.
- **Market** — open stalls with awnings (place a `Merchant` here — it's your shop).
- **Dock** — wooden planks over water (an element-travel launch point; place near water).
- **Camp** — tents (bandit camps spawn hostile/neutral factions here).
- **Arena** — a ring with low walls/columns.
- **Landmark** — a statue, monolith, or great tree.
- **WeaponCache** — a chest or crate (a `WeaponPickup` spawns here).

These meshes become the prefabs `StructureBuilder`/`StructurePlacer`/`WorldSpawnPlacer` reference.

---

## 3. Characters (humanoids)

> **Prefab note (read once).** Keep the art focused on the model and colors; for component wiring,
> match the existing placeholder prefab and `docs/DEPLOYMENT.md`. One important nuance: the
> **interaction system** (`PlayerInteractor`) finds mounts, tameable creatures, house plots, and
> merchants with a physics **OverlapSphere**, so those objects need a **collider**. Wild creatures you
> weaken and tame also need to receive your hits, so they keep a collider too. **Ally companions** are
> driven by the faction registry rather than physics and are deliberately collider-light to avoid your
> own projectiles hitting them — so when in doubt, **replicate the collider the placeholder used**.

### 3.1 The player rig

- Box-model a low-poly humanoid (torso, limbs, head; ~1.5k–3k tris). Flat-color skin and clothing.
- Rig with Rigify or a simple metarig, weight paint, export with the armature. In Unity set
  **Humanoid** and configure its bone mapping. `GameBootstrap`/`FirstPersonRig` expect a player prefab with a
  `CharacterController` plus the player scripts and the **"Player"** tag — see `DEPLOYMENT.md`.
- **VR is first-person**, so the body is mostly unseen: prioritize simple **hands/arms** parented to the
  controller anchors. A full body is optional (useful only for shadows or a mirror).

### 3.2 Enemies (`EnemyKind`: Grunt, Brute, Runner, Archer, Elementalist)

Reuse one humanoid base and vary silhouette + color:

- **Grunt** — baseline, muted colors.
- **Brute** — bulked torso/shoulders, larger scale, heavy.
- **Runner** — lean and light, fast read.
- **Archer** — slimmer, with a bow prop parented to the hand (kites at range).
- **Elementalist** — robed and **element-tinted**; color by its element (these spawn as Wild with an element).

Each variant becomes an enemy prefab for `WorldSpawnPlacer`/`EnemyController` and carries
`Damageable` + `FactionMember` + `CharacterController`.

### 3.3 Civilians

Simplest humanoids in friendly, varied flat colors. They use `CivilianController` + `FactionMember`
(Civilian) + `CharacterController`, and wander or flee from Wild/Bandit threats.

---

## 4. Creatures & companions (the bestiary)

For each, a shape cue, element color, and **locomotion** (from `Locomotion.For`, which controls how the
mount/creature moves: Ground / Water / Flying). Rideable creatures need a **rider/seat anchor** (an
empty at the back) since `MountController` seats the player at the transform.

**Mounts & wild creatures**

- **Horse** — Ground, rideable, purchasable. Standard mount; natural browns; add a saddle + rider anchor.
- **FireDragon** — Flying, fire. Winged serpent; red/orange; broad wings.
- **WaterDragon** — Flying, water. Sleek eastern dragon; teal/blue.
- **Mermaid** — Water. Humanoid torso + fish tail; aqua.
- **EarthMole** — Ground, earth. Chunky burrower; brown, big claws.
- **EarthCat** — Ground, earth. Stocky feline; earthy tones.
- **AirDragonfly** — Flying, air. Insectoid body + four wings; pale light-blue.
- **AirJellyfish** — Flying, air. Floating bell + tendrils; pale translucent-looking blue.

**Rare combat companions** (same low-poly approach; give a brighter accent color or a small glow for
their ability):

- **Spider** — Ground, earth. Eight legs, dark body; spins the web trap (§6.4).
- **WaterCat** — Water, water. Agile cat; blue; blinks (teleports) and resists water.
- **IceCat** — Ground, water/ice. White/ice-blue cat; slows enemies; resists ice.
- **Phoenix** — Flying, fire. Fiery bird; red/gold; burns enemies and self-revives once.
- **ElectricSquirrel** — Ground, fire/lightning. Small rodent; yellow; stuns with lightning.
- **Dog** — Ground, earth. Loyal hound; brown; dig-blinks to flank.

Keep creatures in the 800–2.5k tri range; flat-color by region. Companions are summoned via
`CompanionSummoner` (one shared `companionPrefab` placeholder today); wild creatures and mounts use
the shared `creaturePrefab`. Replace these with the per-creature models and assign per kind.

---

## 5. Vehicles (`VehicleKind`)

- **FireGalleon** — Flying, fire-locked, capacity 4. A grand flying ship; red/black/gold hull, big sails.
- **AirSkiff** — Flying, air-locked, capacity 2. A small sky boat; light wood + pale sails.
- **Rowboat** — Water, open to all, capacity 2. Tiny wooden boat + oars.
- **Sailboat** — Water, open to all, capacity 4. Wooden hull + mast + single sail.

Modeling: hull from a beveled cube, scoop the interior, add a cylinder mast and a subdivided-plane
sail (vertex-color it in soft bands). Add a **rider/seat anchor**, and set the **pivot at the waterline**
(boats) or base (flyers). `MountController.Configure` uses the vehicle's locomotion — water craft ride
at the water level, flyers move in full 3D. Vehicles are bought in the shop and summoned by
`MountSummoner` (its `vehiclePrefab` slot).

---

## 6. Props & effects

### 6.1 Weapons (`WeaponType` × `WeaponMaterial`)

Model each weapon (Hammer, Sword, LongBow, Shield, Dagger, Sai) low-poly with the **grip at the
pivot**, parented to the hand/`WeaponHolder`. Color by material so players can read it: **Wood** brown,
**Metal** steel grey, **Ice** pale cyan. This matters — the combat logic breaks wood by Fire, metal by
Oreshaping, and ice by Water, so the color is a gameplay tell.

### 6.2 Projectiles & ability shards

`AbilityVfxBinder`/`ProceduralProjectiles` spawn small element meshes — swap in tiny vertex-colored
shapes with **bright** flat colors: fireball (orange sphere), water jet (stretched droplet, blue),
earth shard (angular rock, brown), air gust (thin ring/wisp, pale), ice spike (cyan), lightning bolt
(jagged, yellow). Keep them tiny and let the bright color sell the glow.

### 6.3 Element travel: ice floe & bubble

- **Ice floe** (water travel): a low-poly flat ice disc/hexagon, pale cyan, slightly irregular edges.
  Replaces the disc the `ElementTravelController` builds.
- **Bubble** (air travel): an icosphere with a **transparent** material (not opaque `ToonLit`) — use a
  simple URP unlit transparent soft-blue so the player can see out. Minimal modeling.

### 6.4 Web trap (Spider companion)

A flat radial **web** mesh (a plane with a spun pattern, or a few crossed strands), semi-transparent.
Replaces the disc `WebTrap` builds when the spider roots enemies.

### 6.5 Pickups (`WeaponPickup`, `LurePickup`)

- **WeaponPickup** — the weapon model on a small pedestal, or floating with a slow spin.
- **LurePickup** — a small bait pouch/charm prop.
- Keep their **trigger colliders** so `OnTriggerEnter` pickup still fires.

### 6.6 Weather particle sprites (`WeatherController`)

Particles can't use vertex colors. Either keep the code's flat-color particles as-is, or supply a tiny
soft **sprite** (8–32 px) per type — a soft dot for snow, a short streak for rain, a fleck for sand —
and assign it to the particle material; the controller already sets the start color. This is the only
place you need a small 2D texture. Heat-haze can stay a subtle tint.

---

## 7. UI (2D — a separate pipeline)

The HUD, character-creation menu, shop, and world map are code-built uGUI using the built-in font, so
vertex colors don't apply. To art the UI, make **2D sprites** (panels, buttons, gem icons for the five
currencies, a prompt frame) and a proper **font**, then swap the code's `Image` colors/sprites and the
`Text` font. Keep it flat, clean, and high-contrast to match the Wind-Waker tone. This is an
image-editor task, outside the Blender flow, but listed so it isn't forgotten.

---

## 8. Poly budgets & Quest performance

Rough standalone-Quest targets (pre-combine):

| Asset | Tris |
| --- | --- |
| Small props / pickups | 50–500 |
| Weapons | 100–400 |
| Buildings | 200–1,500 |
| Creatures / companions | 800–2,500 |
| Humanoids (player, enemies) | 1,500–3,000 |
| Vehicles | 1,000–4,000 |

- **One material to batch them all.** Because every vertex-colored mesh shares the single `ToonLit`
  material, the `MeshCombiner` collapses static scenery into very few draw calls. Keep static art on
  that one material to maximize batching.
- **Outline cost.** The outline is a second pass (the inflated hull is drawn again), so it roughly
  doubles an object's draw cost. Keep the width modest and consider a no-outline material (or
  Outline Width 0) on tiny or distant props.
- **LODs.** Decimate big or distant meshes (buildings, large creatures) into 1–2 LODs and set up a
  `LODGroup` in Unity.
- **Flat shading + low vertex counts** keep the toon bands crisp and the GPU light; avoid smooth
  normals except where a soft band is intended.
- **Lighting.** The toon shader is lit mostly by the single directional sun that `DayNightCycle`
  animates; bake static shadows only if you need them.

---

## 9. Suggested order of work (biggest visual change first)

1. **Tune `ToonSky` + `ToonWater` palettes** — instant Wind-Waker vibe, zero modeling.
2. **Player hands + a weapon** — you stare at these constantly in first-person/VR.
3. **Common enemies + civilians** — one humanoid base, recolored.
4. **Buildings for the first POIs you visit** — village house, market stall, dock.
5. **Terrain treatment** — flat-color layers now, mesh terrain later.
6. **Horse + the boats** — the most-used mount and vehicles.
7. **Creatures/companions you actually own and fight.**
8. **Projectiles/ability shards + ice floe/bubble + web.**
9. **UI sprites + font.**
10. **LODs, outline tuning, final Quest perf pass.**

---

## 10. Where each asset plugs into the code

| Asset | Script / prefab slot |
| --- | --- |
| Terrain | `TerrainBuilder` (Unity Terrain) — or replace per §2.1 |
| Water | `WaterSurface` (`ToonWater`) |
| Sky | RenderSettings skybox (`ToonSky`); `DayNightCycle` drives `_SunDirection` |
| Buildings | `StructureBuilder` / `StructurePlacer` prefabs, per `StructureKind` |
| Player | `GameBootstrap` / `FirstPersonRig` player prefab (Humanoid, "Player" tag) |
| Enemies | `WorldSpawnPlacer` enemy prefab → `EnemyController` |
| Civilians | `WorldSpawnPlacer` civilian prefab → `CivilianController` |
| Wild creatures | `WorldSpawnPlacer.creaturePrefab` → `CreatureController` + `Tameable` |
| Companions | `CompanionSummoner.companionPrefab` → `CompanionController` |
| Mounts | `MountSummoner.creaturePrefab` |
| Vehicles | `MountSummoner.vehiclePrefab`; bought via `ShopController` / `VehicleCatalog` |
| Lure pickups | `WorldSpawnPlacer` → `LurePickup` |
| Weapon caches | `WorldSpawnPlacer` → `WeaponPickup` → `WeaponHolder` |
| Ability shards | `AbilityVfxBinder` / `ProceduralProjectiles` |
| Ice floe / bubble | `ElementTravelController` |
| Web | `WebTrap` (from the Spider `CompanionController`) |
| Weather sprites | `WeatherController` particle material |
| UI | `GameHud` / `CharacterCreationUI` / `ShopController` / `WorldMapView` |

For the exact components on each prefab, cross-reference **`docs/DEPLOYMENT.md`**.

---

*Make the shape, flood the colors, let the shader light it. That single loop, repeated down this list,
turns the placeholder scaffold into the finished Wind-Waker-styled world.*
