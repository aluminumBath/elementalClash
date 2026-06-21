# Elementborn — Unity VR scaffold

A cross-platform elemental-combat game for **Meta Quest (standalone)**, **PCVR**, and **flat
first-person PC** from one Unity 6 (URP) codebase — fast elemental combat, a gacha-style
character roll, and a cel-shaded look.

> **Original lexicon (legal safety).** Franchise-specific terms have been replaced with original ones
> throughout the code and UI. Generic words (fire, water, earth, air, ice, lightning, metal, lava,
> blood) are kept — they aren't anyone's IP.
>
> | Concept | Term used here |
> |---|---|
> | using an element | **Channeling** (e.g. a fire **channeler**) |
> | master of all four elements | **the Confluence** |
> | fire sub-art (lava) | **Magmacraft** |
> | water sub-art (blood) | **Sanguine Grip** |
> | earth sub-art (metal) | **Oreshaping** |
> | air sub-art | **Flight** |

## What this is (and isn't)
The **code** spine: platform-agnostic logic, input providers, presentation hooks, tests, CI, and a
cel shader. It is **not** scenes, prefabs, materials, terrain, or art — those are built in the Unity
editor / Blender and are called out where relevant.

## Documentation
Start with **`docs/INDEX.md`** (a map of all the docs). Quick links: `docs/GETTING_STARTED.md` (zip →
running game), `docs/DEPLOYMENT.md` (setup, scene/prefab wiring, builds, CI), `docs/WHATS_LEFT.md` (the
remaining-work roadmap), and `docs/ART_GUIDE.md` + `docs/PALETTE.md` + `docs/UI_SPRITES.md` (art).

## Architecture
Logic is decoupled from platform and presentation so all three modes share one core and stay
testable:

`Input provider` → device-agnostic **`ChannelingIntent`** → **`AbilitySystem`** (or **`Weapons`**)
resolves it against the **`ChannelerLoadout`** → **`AbilityOutcome`** → presentation (VFX / melee /
dash / barrier).

`Elementborn.Core` holds no MonoBehaviours (pure C#, unit-tested). `Elementborn.Game` holds the
Unity glue.

## Layout
- `Assets/Elementborn/Core` — elements, loadout, gacha, combat, abilities, weapons, status, knockback (pure logic)
- `Assets/Elementborn/Game` — bootstrap, input providers, combat/creation controllers, code-built UI
- `Assets/Elementborn/Game/Combat` — projectiles, melee, weapon holder/pickups, Sanguine Grip, enemy
- `Assets/Elementborn/Art/Shaders` — `ToonLit`, `ToonSky`, `ToonWater` cel shaders
- `Assets/Tests/{EditMode,PlayMode}` — tests; `.github/workflows/tests.yml` — GameCI

## Channeling & character creation
- Pick a **path**: an **element** (fire/water/earth/air) or a **weapon**.
- An element roll uses luck: the base element, a ~5% **sub-art** (Magmacraft / Sanguine Grip /
  Oreshaping / Flight), or a ~0.1% **Confluence** (all four). One locked roll, no re-roll.
- `GachaRoller` + `CharacterCreationService` are pure and tested; `CharacterCreationUI` is code-built
  uGUI (Path → Element/Weapon → Reveal → Begin). Flat works out of the box; VR needs the XRI UI
  raycaster + input module on the canvas.

## Combat
- All four elements have primary + secondary + defend (and air gets a comfort **dash/glide**, not free flight).
- Sub-arts modify them: **Magmacraft** adds burn, **Sanguine Grip** swaps water's secondary for a
  Control grip, **Oreshaping** adds damage and tags hits so they can shatter metal, **Flight**
  lengthens the air glide.
- Status (slow / stun / burn / control), knockback, and a chasing `EnemyController` all run off the
  same `Damageable`.

## Weapons (players without an element)
Six weapons — **hammer, sword, long bow, shield, dagger, sai** — each in **wood / metal / ice**.
Melee weapons swing a short forward hit, the long bow fires, the shield raises a strong block.
Material sets the tier and a weakness:

| Material | Damage | Shatters to | Extra |
|----------|--------|-------------|-------|
| Wood  | weak (x0.7)   | **fire**                 | — |
| Metal | strong (x1.3) | **Oreshaping** (metal channeling) | — |
| Ice   | mid (x1.0)    | **water**                | slows on hit |

`WeaponHolder` breaks the weapon when a matching hit lands (using `DamageInfo.Variant`, so Oreshaping
is distinct from plain earth), leaving you unarmed until you grab another. You pick a wood starter at
creation; `WeaponPickup` objects scattered on the map provide the rest (channelers have no holder, so pickups ignore them).

## World map
A procedural, seeded world you can regenerate freely or pin with a seed:
- **Data model** (`WorldMap`, `WorldRegion`, `PointOfInterest`) — named regions with a biome,
  position, danger level, neighbour links, and POIs; queries for the capital, all POIs, and weapon caches.
- **Biomes** — capital city, swamp, marsh, beach, desert, forest temple, volcano, island, cloud
  temple, plus plains / mountains as connective tissue.
- **Generator** (`WorldGenerator`, pure + tested) — scatters regions, assigns biomes by position
  (islands/beaches on the coast, volcanoes/deserts inland, cloud & forest temples remote), links
  nearest neighbours, and fills each region with POIs (enemy counts + weapon caches), naming
  everything from **original** name pools. Deterministic per seed.
- **In-game map** (`WorldMapView`) — a code-built top-down screen: biome-coloured nodes, connection
  lines, labels, and a detail panel listing a region's POIs on click. `WorldMapController` holds/generates it.
- **Scene spawner** (`WorldSpawnPlacer`) — drops `WeaponPickup` prefabs at weapon-cache POIs and
  enemy prefabs by suggested count (assign prefabs + a map->world scale).

The **sculpted terrain** — the landmasses, biome meshes, and the islands/volcanoes you actually walk
on — is built on top of this data in Unity terrain / Blender; the code gives you the named, connected,
populated world to place it against.

## Terrain (from the world data)
The ground is shaped by the same `WorldMap`, in two layers:
- **`TerrainGenerator`** (Core, pure + tested) — projects each region's biome profile onto a
  heightmap (blended by distance so biomes transition), gives volcanoes cones with craters, drops
  region-free areas to the seabed so regions read as landmasses, and adds fractal noise. Outputs a
  `TerrainModel` (normalised height grid + dominant-biome grid + sea level). Deterministic per seed.
- **`TerrainBuilder`** (Game) — applies that model to a Unity `Terrain` at runtime: sets the
  heightmap and, if you assign a `TerrainLayer` per biome, paints a splatmap from the biome grid.
  Shape works with no art assigned; the `TerrainLayer` textures and any hand-sculpt refinement are
  the editor/art layer on top. `WorldSpawnPlacer` can snap spawns to the terrain surface.

Keep `TerrainBuilder.terrainSize` equal to `WorldGenConfig.MapSize` (default 1200) so regions, the
map, and spawns all line up in world space.

## Buildings & structures (POI-driven)
Structures come straight from the POIs the world already produces — no separate level editing:
- **`StructureGenerator`** (Core, pure + tested) maps each `PoiType` to a `StructureKind` (city ->
  town, temple -> temple, camp -> tents, dock -> dock, dungeon -> ruin, market -> stalls, landmark ->
  standing stones, cache -> crate, ...) and assembles a `BuildingPlan`: a list of `PlacedPart`s
  (chunky boxes, pillars, pyramidal roofs, cones) in local space, seeded per POI for variety.
- **`StructureBuilder`** (Game) instantiates a plan into flat-shaded primitive meshes (generated and
  shared), coloured with the toon palette; box bodies get colliders.
- **`StructurePlacer`** (Game) walks `WorldMap.AllPois()`, generates each plan, and builds it at the
  POI's position on the terrain (capped via `maxStructures`); each structure's parts are then merged
  per material by **`MeshCombiner`**, so a town is a handful of draw calls instead of dozens.

## Visual style (Wind Waker-leaning)
Cel-shaded throughout, set up from code:
- **`Elementborn/ToonLit`** — banded lighting + inverted-hull outline on everything.
- **`Elementborn/ToonSky`** — a procedural skybox: top/horizon/ground gradient, a soft sun disc, and
  drifting toon cloud bands.
- **`Elementborn/ToonWater`** — a stylised water surface: deep/shallow gradient, banded lighting,
  moving foam lines, gentle vertex waves, and a fresnel sparkle.
- **`SceneStyleController`** (Game) applies the lighting pass in one call — the toon sky, a warm
  "sun" directional light, gradient ambient, and stylised distance fog. **`WaterSurface`** (Game)
  builds a code water plane at sea level (or `TerrainBuilder` drives it so it matches the terrain).

Flat, saturated colours (`ToonPalette`) and chunky geometry complete the silhouette. What's left to
fully sell it is hand-made low-poly art and texture work — the shaders and palette are the framework.

## Sample flow
`GameFlowController` runs the whole loop in code — **Boot -> Character creation -> World map ->
Spawned world** — and builds the creation and map screens itself:
- gates player control (disables movement + combat and frees the cursor during menus, restores them in the world),
- on the creation screen's *Begin*, applies the rolled loadout to the player and moves on,
- generates the world and shows the map; *Enter the world* (using the selected region, or the capital) advances,
- enables gameplay and builds the world: terrain (`TerrainBuilder`), then POI structures
  (`StructurePlacer`), then pickups/enemies (`WorldSpawnPlacer`) if assigned.

Drop it on an empty GameObject; it finds the rig's `PlayerCombatController` / `FirstPersonRig` /
`WeaponHolder` at runtime (or assign them, plus a VR locomotion behaviour). It runs end to end even
with no rig or spawn prefabs assigned, so you can try the loop immediately.

## Art / cel shader
`Elementborn/ToonLit` (URP): banded diffuse, shadow tint, rim light, and a geometry-based
inverted-hull outline (VR-safe, not post-process). Swap the placeholder font/UI.Text for TMP in
production. Targets Unity 6 URP; macros may need tweaks on older URP.

## Gameplay
- **Scoring** — `ScoreSystem` (Core, pure + tested): a decaying combo multiplier, high score, and
  change events. `ScoreController` (Game) holds it, shows a small score/combo HUD, and is the static
  hook enemies award through on death.
- **Enemy variety** — `EnemyArchetypes` (Core) defines five kinds (Grunt, Brute, Runner, Archer,
  Elementalist) with their own stats, and `EnemySelector` picks one by a region's danger and biome.
  `EnemyController` applies them (melee close in, ranged kite) and awards score on death;
  `WorldSpawnPlacer` assigns archetypes per region.
- **Death & respawn** — `RespawnController` watches the player's `Damageable`; on death it disables
  control, shows a countdown overlay, then revives at the spawn point (resetting the combo). Enemies
  destroy on death; the player's `Damageable` keeps `DestroyOnDeath` off.

## Factions & aggression
- **`FactionRules`** (Core, pure + tested) decides hostility from faction + element + whether the target
  has provoked you: anyone who attacks you turns hostile; **Wild** element-fighters tolerate their own
  element but attack other-element channelers (ignoring weapon users and peaceful folk); **Bandits**
  attack everyone but their own kind and civilians; **Civilians** are peaceful until hit; **Allies** fight
  Wild/Bandit and like the player; the **Player** never auto-aggros.
- **`FactionMember`** (Game) tags a character with its faction/element, holds a short **grudge** against
  attackers, registers in a global list, and finds the **nearest hostile** target. `EnemyController` uses
  it to acquire targets (so enemies fight each other and bandits, not just you), and `Projectile` / melee
  call `FactionMember.RegisterHit` so attacking a neutral turns it hostile.
- **Element-typed enemies** — `EnemyComposition.Pick` (Core) rolls each enemy's allegiance, element, and
  kind by biome + danger: most are **Wild** channelers of a biome-leaning element (Volcano→fire, etc.),
  the rest weapon **Bandits**. `WorldSpawnPlacer` applies the plan and also scatters peaceful
  **`CivilianController`** townsfolk (Faction Civilian) at cities/villages/markets — they wander and flee
  from fighters. The player's element is tagged onto its `FactionMember` at character creation.

## Economy, ownership & taming
- **Currency** — `Wallet` (Core, pure + tested) holds mixed denominations (silver, ruby, emerald,
  sapphire, diamond on a ×5 value ladder); prices are silver-values and spending "makes change". Enemies
  drop silver (and the odd ruby) on death.
- **Creatures as data** — `CreatureCatalog` (Core) defines every mount/companion (element, who may own
  it, rideable, price, tame chance). The bestiary and rare companions plug their behaviour in later.
- **Taming** — `TamingRules` (Core, pure + tested): you need the creature's specific **lure** *and* it
  must be **weakened** (≤25% health), then success is its tame chance; the lure is spent on any attempt.
  `Tameable` (Game) bridges a weakened creature to `PlayerInventory.TryTame`.
- **Ownership** — `PlayerInventory` (Game, singleton) tracks the wallet, lures, owned creatures, and the
  player's element (so you only get creatures appropriate to you). `TryBuy` checks element + affordability.
- **House → respawn** — `HousePlot.TryClaim` buys one home and `RespawnController` respawns you there;
  claiming a new plot relocates home.

## Mounts, vehicles & comfort
- **`MountController`** (Game) makes any vehicle or large creature rideable: it seats the player rig,
  disables their own locomotion, and drives movement by type — ground mounts ride the terrain, boats sit
  at the water line, flyers move in full 3D. Steering is smooth; reads WASD/Space/Ctrl by default with
  input-action hooks for VR thumbsticks.
- **`ComfortVignette`** (Game + `Elementborn/ComfortVignette` shader) is a stereo-aware tunnelling vignette
  on the player camera (mask generated in code) that fades in with speed to ease motion sickness.
- **`PlayerInteractor`** (Game) is one contextual button (E / a VR button) that mounts or dismounts,
  tames a weakened creature, or claims a house plot — whichever is nearest.
- **Vehicles as data** — `VehicleCatalog` (Core) defines the craft: fire galleons and air skiffs
  (element-locked flyers), open-to-all rowboats and sailboats; `Locomotion.For` gives each creature its
  movement type. Prefabs/visuals are the editor/Blender pass.

## Element travel (water crossing)
- **`ElementTravel.ModeFor`** (Core, pure + tested) maps element → crossing ability: water → ice floe,
  air → bubble, others → none.
- **`ElementTravelController`** (Game) is one toggle (F / a VR button) that summons the right craft for
  your element and mounts you on it: a **water** channeler surfs a code-built **ice floe**; an **air**
  channeler floats in a **bubble** that disables attacks (the mount's combat flag). Both ride the water
  line via `MountController`, so you summon them over/near water.

## Bestiary (wild creatures)
- **`CreatureController`** (Game) is a wild creature: it wanders peacefully (faction Neutral, so enemies
  ignore it) but fights back when attacked — how you weaken one to tame it. Locomotion follows its kind:
  ground beasts walk the terrain, water creatures swim at the surface, flyers hover.
- **`Wildlife.Pick`** (Core, pure + tested) chooses which bestiary creature roams a biome (fire dragons
  in volcanoes, water dragons/mermaids on coasts, moles/cats inland, dragonflies/jellyfish at cloud
  temples). `WorldSpawnPlacer` scatters them per region.
- **Lures** — `LurePickup` grants a creature's taming lure on pickup; the spawner places them in markets,
  shrines, and camps for nearby wildlife, so taming has its inputs.
- **Riding what you own** — `MountSummoner` (toggle M / a VR button) summons a tamed rideable creature
  beside you as a `MountController` mount and seats you; toggling again dismisses it.

## Companions (rare combat allies)
- **`CompanionController`** (Game) is a tamed ally that follows you, then chases and attacks the nearest
  hostile with its kind's element and on-hit status, shrugging off what it's immune to, and using its
  trick. **`DamageImmunity`** (Core, pure + tested) expresses the exact cases: the **water cat** shrugs
  water but ice still bites; the **ice cat** is the mirror; the **phoenix** ignores fire (and is reborn
  once on death).
- **Tricks** — the **spider** lays flammable **`WebTrap`**s that root enemies (a Control status) and, if a
  trapped enemy catches fire, ignite for a burst; the **cats** and **hound** blink to flank; the storm
  **squirrel** stuns with lightning, the **ice cat** chills.
- **`CompanionProfiles`** (Core) holds each companion's attack element/variant, status, immunity, and
  flags; `CompanionSpawns` places rare tameable companions by biome (phoenix in volcanoes, etc.).
- **`CompanionSummoner`** (toggle C / a VR button) calls every companion you own to fight; toggling sends
  them away. Placeholder prefab for all until per-creature art.

## The shop & vehicle ownership
- **`Merchant`** is an interactable shopkeeper (place one at a market POI). It can stock specific creatures
  and vehicles, or — left empty — sell everything purchasable. `PlayerInteractor` shows "[E] Shop" near it.
- **`ShopController`** is a single code-built buy menu any merchant opens: it lists the items your element
  allows and that you don't already own, each a button priced in gems (via `Wallet.Breakdown`). Buying
  spends from the wallet and toasts the result; while open it gates combat + movement and frees the cursor.
- **`PlayerInventory`** now tracks owned **vehicles** too (`OwnsVehicle`, `TryBuyVehicle`, element-gated like
  creatures), and they're saved/loaded alongside everything else.
- **`MountSummoner`** prefers a tamed rideable creature but falls back to summoning an **owned vehicle**, so
  a purchased boat or skiff is rideable with the same toggle.

## Playability glue (HUD, interaction prompts, saving)
- **`GameHud`** (Game, singleton) shows a currency readout (top-left, polled from the wallet), a
  contextual interaction prompt (bottom-centre), and a transient toast for results. Code-built canvas.
- **`PlayerInteractor`** now scans for the nearest interactable each frame and asks the HUD to show a
  prompt — "[E] Ride", "[E] Tame Horse", "Weaken it first", "[E] Claim home" — then toasts the outcome
  when you press the button (e.g. the tame result, or whether a home was claimed).
- **`SaveSystem` / `SaveController`** persist progression — wallet, lures, owned creatures, the house, and
  the player's element — to a JSON file in the platform's persistent data folder. Loads on start, saves on
  quit, with manual F5 (save) / F9 (load) keys. `PlayerInventory.ToSave()/LoadFrom()` do the mapping.

## Weather & day/night
- **`WeatherProfiles` / `WeatherEffects`** (Core, pure + tested) decide which weather a biome can throw
  (blizzards in the mountains, sandstorms and heat hazes in the desert, rain and hurricanes on the coast,
  tornadoes on the plains) and how each one slightly nudges a channeler's element — rain favours water and
  dampens fire, a heat haze favours fire, a blizzard chills it, etc. (multipliers stay within ~0.8–1.2).
- **`WeatherController`** (Game, singleton) rolls new weather for the biome the player is standing in,
  shows code-built particles + fog, and feeds the per-element multiplier to `PlayerCombatController`, which
  scales a channeler's attack power by it.
- **`DayNightCycle`** (Game) rotates the sun, dims it at night, lerps ambient day↔night, and tracks the
  sun across the ToonSky so the sun disc moves. Pairs with `SceneStyleController`.

## Building & running
Flat PC, PCVR, and Meta Quest all build from this one codebase (`GameBootstrap` auto-detects VR vs flat).
The full step-by-step — requirements, Unity 6 setup, OpenXR config, the one-time scene/prefab wiring,
signing, performance, and CI — lives in **`docs/DEPLOYMENT.md`**.

- **Terrain:** `MeshTerrainBuilder` is a drop-in alternative to the Unity-Terrain `TerrainBuilder` that
  builds a faceted, vertex-colored low-poly mesh (via the pure `MeshTerrainGenerator`) rendered with
  `ToonLit` — cel bands **and** outline. Ground snapping works with either path through `TerrainHeight`.
- **Defend** now grants a brief damage-reduction shield (`Damageable.Shield` + `BarrierResponder`), and
  the air **dash** raises the VR comfort vignette while it runs.
- **Palette:** `palette/` holds a ready-made 32-color starter palette matching `TerrainColors` and the
  shaders — a Blender import script (native palette), a labeled PNG swatch, and a `.gpl`; see `docs/PALETTE.md`.
- **Art:** `docs/ART_GUIDE.md` is a full object-by-object guide to replacing every code-built
- **UI art:** `docs/UI_SPRITES.md` gives exact sprite sizes, anchors, 9-slice borders, and colors for
  every code-built screen (HUD, score, death overlay, creation, shop, map), so 2D art drops straight in.
 is a full object-by-object guide to replacing every code-built
  placeholder with hand-made low-poly art using Blender + flat vertex colors. The `Elementborn/ToonLit`
  shader now has a **Use Vertex Colors** toggle (off by default) for that art.

## Remaining work
1. **Hand-made art** — geometry, buildings, sky/water, lighting, and per-structure mesh-combining come
   from code; what remains is low-poly models + textures and a per-platform `QualityTierController`.
2. **World scale** — streaming / LOD for large worlds.
3. **The scaffold is feature-complete.** Every gameplay system is in — factions, element enemies,
   civilians, economy/taming, mounts, element travel, the bestiary, companions, weather + day/night, the
   HUD, interaction prompts, saving, and the shop + vehicle ownership — and `docs/ART_GUIDE.md` now covers
   turning the code-built placeholders into finished low-poly art. What remains is the art itself plus the
   one-time scene/prefab wiring in `docs/DEPLOYMENT.md`.

## Setup
Open in Unity 6 (URP). Let Package Manager resolve `Packages/manifest.json`, then create the VR rig
(OpenXR + XR Interaction Toolkit) and a flat rig; wire input providers, `PlayerCombatController`,
`CharacterCreationController`, and a `Damageable` per the notes above. `GameBootstrap` auto-detects
VR vs flat. For the full sample loop, add a `GameFlowController` to an empty object (see **Sample flow**).
