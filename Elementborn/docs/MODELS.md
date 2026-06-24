# Elementborn — Placeholder Models

Procedurally generated **medium-poly** placeholder meshes (19 models) for the geometric objects in the world, so you
have stand-ins better than primitives while the real art is modeled. They're faceted and flat-colored with
the project palette. They are **not** a replacement for the organic Blender work (creatures, characters) —
those still need modeling.

Each model ships in two formats, from the same geometry — pick the one for your tool:
- **Unity-ready:** `Assets/Elementborn/Art/Models/*.obj` (+ `*.mtl`) — imports with no plugins; each color
  is a material.
- **Blender (vertex colors):** `Assets/Elementborn/Art/Models/blender_ply/*.ply` — imports with per-vertex
  colors, matching the `ART_GUIDE.md` vertex-color workflow.

## The set

| Model | Tris | Placeholder for |
| --- | --- | --- |
| `crate` | 84 | weapon cache / props (WeaponCache POI, WeaponPickup) |
| `barrel` | 144 | props (villages, markets, camps) |
| `rock` | 192 | terrain scatter |
| `tree` | 360 | forests, plains |
| `bush` | 240 | ground cover |
| `fence` | 48 | camps, villages, farms |
| `house` | 54 | village building (`StructureKind.Village`) |
| `pedestal` | 36 | shrine / pickup plinth (WeaponPickup, LurePickup, Shrine) |
| `sword` | 48 | weapon (`WeaponType.Sword`) |
| `hammer` | 60 | weapon (`WeaponType.Hammer`) |
| `chest` | 60 | treasure / weapon cache (alt to crate) |
| `market_stall` | 144 | Market structures (striped canopy + counter) |
| `tent` | 7 | Camp structures |
| `dock` | 108 | Dock structures (sits over water) |
| `shield` | 240 | weapon (`WeaponType.Shield`) — lies flat, reorient in editor |
| `signpost` | 76 | landmarks / wayfinding |
| `rock_small` | 120 | terrain scatter (small) |
| `pine_tree` | 124 | forests / mountains (conifer variant) |
| `campfire` | 636 | Camp ambiance (stone ring + logs + flames) |

## Scale & orientation
- Units = **meters, Y-up**, resting on the ground plane (base near y = 0) — authored Unity-native. A few thin pieces (`shield`, weapons) sit flat and may need rotating to their held orientation in the editor.

## Importing
- **Unity (OBJ):** drop the `.obj` in; Unity creates a material per color from the `.mtl`. For the toon
  look, switch each material's shader to `Elementborn/ToonLit` (or assign your own). These use **materials**,
  not vertex colors.
- **Blender (PLY):** File ▸ Import ▸ Stanford (.ply); it comes in with **vertex colors**. Some Blender
  versions import PLY Z-up — if it lands on its side, rotate −90° on X, then apply and export FBX to Unity
  as usual (Unity reads the vertex colors; enable **Use Vertex Colors** on the `ToonLit` material).

## Notes
- Faceted, flat-colored, recognizable — **not final art**. Refine or replace them: the PLYs drop into
  Blender with their colors, or model fresh per `ART_GUIDE.md`.
- Pipeline choice: for the `ToonLit` vertex-color path use **PLY → Blender → FBX**; for a fast colored
  drop-in use the **OBJ**.
- Colors are palette hues (wood, metal, stone, leaf, wall, roof) — see `PALETTE.md`.

## Creature & character models (runtime binding)

The placeholders above are for **geometric world props**. **Creatures** are still rendered from one shared
primitive prefab per spawn type — so a Phoenix and a Direstalker look the same until you bind real models.
There's now a code path for that, with a safe fallback.

**Drop-in convention:** put each model prefab under a `Resources` folder at
`Assets/Elementborn/Resources/Models/Creatures/<Name>` (an imported `.fbx` works — Unity treats the imported
model as a loadable prefab). At runtime `CreatureModelLibrary` loads it and attaches it to the spawned creature,
hiding the primitive placeholder. **If no model is found, the placeholder stays** — the game runs unchanged until
you add art, so you can bind creatures one at a time.

- **Default name = the `CreatureKind` enum name.** e.g. `Resources/Models/Creatures/Phoenix.fbx` binds to
  `CreatureKind.Phoenix`. The full list of kinds is in `Core/Creatures.cs`.
- **Files named by colour/shape instead?** Add one line per creature to `CreatureModelNames.Aliases`
  (`Core/CreatureModelNames.cs`): `{ CreatureKind.Phoenix, "ember_bird_red" }` points Phoenix at
  `Resources/Models/Creatures/ember_bird_red`. That alias line is the only code change a binding needs.

**Getting your generated FBX in:** unzip each archive, then move the `.fbx` (and its textures) into
`Assets/Elementborn/Resources/Models/Creatures/`. Rename to the kind it represents, or add an alias. Set the
import scale / orientation so the model rests at local origin facing +Z (it's attached at the host's local zero).
Switch its material(s) to `Elementborn/ToonLit` for the cel look.

**Where it hooks:** wild & tamed creatures via `CreatureController.Start`; summoned mounts via `MountSummoner`;
companions via `CompanionSummoner`. The attach is idempotent (one model per creature). Mapping is pure and
unit-tested (`CreatureModelNamesTests`); the loader caches hits and misses.

## Creature model mapping (Meshy AI batch)

The Meshy AI models in `Assets/generated_assets/` are wired in `CreatureModelNames.Aliases`. `tools/import-creature-models.sh` extracts the mapped ones into `Resources/Models/Creatures/<alias>/<alias>.fbx` (each in its own folder so textures don't collide). After running it, import in Unity and switch materials to `Elementborn/ToonLit`.

| CreatureKind | Model (alias) |
| --- | --- |
| WaterDragon | Azure_Wave_Dragon |
| Phoenix | Fire_Phoenix |
| Thunderbird | Thunderbird |
| Roc | Giant_Eagle |
| Dog | Patchwork_Pup |
| Spider | Antler_Spider_Creature |
| Crab | Coral_Crab_Spider |
| Snake | Teal_Serpent |
| EarthCat | Leaf_Cub |
| Horse | Blue_Dino_Mount |
| Goldkoi | Blue_Gold_Tuna |
| Skimfin | Teal_Fantasy_Fish |
| Gillcloak | Abyss_Angler |
| Tidewarden | Purple_Kraken |
| Direstalker | Shadow_Wolf |
| Skytyrant | Storm_Wyvern |
| Ridgewing | Blue_Fantasy_Bird |
| Glidewisp | Fawn_Sprite |

**No close model in this batch** (still primitive fallback — generate or assign one and add an alias line): FireDragon (no fire dragon was generated), Mermaid, EarthMole, AirDragonfly, AirJellyfish, WaterCat, IceCat, ElectricSquirrel, Eel, Monkey, Crocodile, Rhino, Tiger.

The other ~110 archives are props, gear, characters, structures, and VFX (weapons, banners, gems, portals, scholars, fish schools, etc.) — not creature kinds; place those where their own systems expect them.

## Non-creature model bindings (Meshy AI batch)

Beyond creatures, these maps in `Core/ModelBindings.cs` bind NPCs, sidekick pets, weapons/gear, and the player to batch models; the Game-layer `ModelLibrary.Attach(path, host)` loads them with the same primitive fallback. `tools/import-creature-models.sh` extracts all of them into `Resources/Models/<Category>/<alias>/<alias>.fbx`. Hooks: guide NPCs (`GuideNpcController`), weapon pickups (`WeaponPickup`), the player body (`ThirdPersonRig`), and sidekick orbiters (`NpcSidekickOrbiter`, opt-in via *Use Sidekick Model*).

**NPCs** (`NpcModelNames`): Willow → Verdant_Dryad · Kiana → Azure_Water_Mage · Parfa → Steamwright_Adventurer.

**Sidekick pets** (`SidekickModelNames`, loose stand-ins): Gunnar → Moss_Wolf · Parrot → Teal_Hornbill · Blobfish → Lure_Fish · Mushroom → Luminescent_Mushroom. **Chameleon** has no match → primitive (generate one).

**Weapons / gear** (`WeaponModelNames`): Sword → Emberblade · LongBow → Gilded_Arc_Bow · Shield → Azure_Aegis · Hammer → Stormcleaver_Axe (a heavy stand-in). **Dagger** and **Sai** have no match → primitive.

**Player** (`PlayerModelNames` + `PlayerModelBinder`): the rig adds a `PlayerModelBinder` to its body, which prefers a **rigged humanoid** prefab at `Resources/Models/Characters/PlayerRigged/PlayerRigged` (skinned mesh + Animator) and drives its locomotion — it measures the body's planar speed and feeds a `Speed` float (0 idle → 0.5 walk → 1 run, eased) to the Animator's 1D blend tree (`Core/LocomotionAnimation`, unit-tested). With no rigged prefab it falls back to the static Windborne_Traveler mesh. To enable animation: rig a humanoid (Mixamo / Meshy-rig), build a prefab whose Animator Controller has a float `Speed` param driving idle/walk/run, and drop it at that path.

**Structures / set-dressing / VFX** (`PropCatalog`): a reference registry of batch models (rift_portal → Azure_Arc_Portal, checkpoint_spire → Azure_Crystal_Spire, throne → Throne_of_the_Crystal, vine_gate, mushroom_grove, treasure_chest, banner, crystal_pool, radiant_tree). These are **placed in scenes by hand**, not code-bound — the catalog just makes the names discoverable.

**Items / loot** (`ItemModelNames` + `WorldItemPickup`): items can sit in the world now. Drop a `WorldItemPickup` (set its item id + amount) anywhere; it shows the item's model and grants it on touch. Mapped: ember_shard → Emberstone_Gem, river_pearl → Pearl_Oyster, old_relic → Triskelion_Disc, elemental_charm → Prismatic_Helix_Gem. Potions, foods, and raw materials have no fitting batch model → primitive (they stay icon-only in the inventory UI regardless).

## Animation event hooks

Once the player uses a rigged model, `PlayerModelBinder` adds an **`AnimationEventReceiver`** to it. Author Unity Animation Events on the clips and point them at these methods — each plays the right sound (`AudioController`) and raises a C# event (carrying the world position) for VFX, camera shake, hit-stop, or damage timing:

| Event name (author exactly) | Sound | C# event |
| --- | --- | --- |
| `Footstep` / `FootstepLeft` / `FootstepRight` | footstep | `Stepped(pos, isLeft)` |
| `FootstepWater` | footstep_water | `Stepped` |
| `Jump` / `Land` | jump / land | `Jumped` / `Landed` |
| `AttackWindup` | — | `AttackWoundUp` |
| `AttackSwing` | whoosh_short | `Swung` |
| `AttackImpact` | hit_soft | `Impacted` ← best for hit-stop / shake |
| `CastCharge` | — | `CastCharged` |
| `CastRelease` | wind_whoosh | `CastReleased` |
| `Dodge` | whoosh_short | `Dodged` |
| `Hurt` | hit_soft | `WasHurt` |
| `Vocalize` | — | `Vocalized` |

New footstep/jump/land clips load from `Resources/Audio/` (`footstep`, `footstep_water`, `jump`, `land`); missing files simply don't play. If your clips have **no** authored footstep events, add **`ProceduralFootsteps`** to the body instead — it emits steps from distance travelled (gated on grounded + moving) and shares the same sound path. The static-mesh fallback gets `ProceduralFootsteps` automatically, so footsteps work today.

## Game-feel subscribers

Concrete subscribers turn the animation events above into juice — all asset-free (no particles/materials to wire), all listening on `AnimationEventReceiver`'s **static** broadcast events so they need no reference to the runtime-spawned player:

| Service | Listens to | Effect |
| --- | --- | --- |
| `Feel/CameraShaker` | `AnyImpacted` / `AnyLanded` / `AnyWasHurt` | Decaying screen-plane shake (`Core/ShakeOffset`, unit-tested). Added to the **third-person camera only** (never VR). Runs at execution order 2000 so it offsets *after* the rig places the camera, and on unscaled time so hit-stop doesn't stall it. |
| `Feel/HitStop` | `AnyImpacted` | Brief `Time.timeScale` dip (~0.06× for 0.1 s real) restored on unscaled time. Self-bootstrapping. |
| `Feel/FlashFeedback` | `AnyImpacted` / `AnyCastReleased` | A short-lived fading point light — warm spark on impact, larger cyan burst on cast release (`Feel/TransientLight` + `Feel/LightFade`). Self-bootstrapping. |

So `AttackImpact` fires shake + hit-stop + spark together; `Land` and `Hurt` shake; `CastRelease` flashes cyan. Want more? Subscribe any system to the instance events (per-character) or add a static broadcast for `Stepped`/`Swung`/`Dodged` and a matching service.

## Combat impact feedback (real hits)

The hooks above fire from *animation* events; this layer makes the same juice fire from *actual damage*. `Damageable.Apply` broadcasts `CombatFeedback.Hit(pos, amount, element)` on any hit ≥ 1 and `CombatFeedback.Defeated(pos, element)` on death — a tiny global channel carrying the world position, so presentation reacts without referencing the combatants:

| Reactor | Reacts to | Effect |
| --- | --- | --- |
| `Combat/HitReaction` | its own `Damageable.Health.Damaged` | Squash-pop on the model child (`Core/HitFeedback.SquashScale`, unit-tested) + flash to white via a `MaterialPropertyBlock` (originals restored exactly, no allocations). Scales the **child**, leaving the CharacterController untouched. Required automatically on every `EnemyController`. |
| `Feel/FlashFeedback` | `CombatFeedback.Hit` / `Defeated` | Element-tinted spark at each hit (Fire orange, Water blue, Earth green, Air pale-cyan), bigger burst on defeat. |
| `Feel/CameraShaker` | `CombatFeedback.Hit` | Shakes only for hits **within 12 m** of the camera, scaled by strength (`Core/HitFeedback.Intensity01`) — taking damage yourself is ~0 m, so it always registers; distant skirmishes stay calm. |
| `Feel/HitStop` | `CombatFeedback.Hit` | Punches time only for **heavy** hits (≥ ~0.5 intensity) near the main camera, so chip damage and far fights don't stutter. |

Net effect: cast Fire at an enemy → it flashes white and squashes, an orange spark pops at the hit, a heavy blow briefly freezes time, and a hit near you shakes the view; on defeat a larger burst fires. Any other `Damageable` (creatures, bosses) gets the lights/shake/hit-stop for free; add `HitReaction` to give it the squash-and-flash too.

### Floating damage numbers

`Feel/DamageNumbers` (self-bootstrapping) also listens on `CombatFeedback.Hit` and spawns a `Feel/FloatingNumber` over each hit: a world-space TextMeshPro that billboards to the camera, climbs, pops, and fades, then self-destroys — motion from the unit-tested `Core/DamagePopup` (`Format` rounds to an integer, ≥ 1 for any real hit; `Evaluate` gives the rise/alpha/scale curves). Numbers are tinted by element via the shared `ElementColor` and grow a little with hit strength. Text uses TMP's default font (the same one the UI uses); if no TMP font is configured the service stays silent rather than render broken glyphs. Chip/DoT ticks never appear — they fall below the `amount >= 1` threshold that raises `CombatFeedback.Hit`, and `HitReaction` ignores sub-1 ticks too so burning targets don't flicker.
