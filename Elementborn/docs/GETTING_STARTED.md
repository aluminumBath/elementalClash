# Elementborn — Getting Started

A guided path from the downloaded scaffold to the game running on your screen, then in VR. Each step links
into `DEPLOYMENT.md` for exhaustive detail; this doc is the ordered "do this, then this" route.

## 0. Prerequisites
- **Unity 6 LTS (6000.0.x)** with **Android Build Support** (for Quest) and your desktop module
  (Windows/Mac).
- (VR) a headset; (Quest) Developer Mode enabled + Meta Quest Developer Hub.
→ `DEPLOYMENT.md` §1.

## 1. Open the project
1. Unzip `Elementborn-scaffold.zip`.
2. Unity Hub → **Add** → select the `Elementborn` folder → open with Unity 6.
3. The first import compiles scripts and pulls the pinned packages from `Packages/manifest.json`. Let it
   settle; the Console should be error-free. (If not, see `DEPLOYMENT.md` §11 Troubleshooting.)
→ `DEPLOYMENT.md` §2.

## 2. Project settings (one time)
- Set the **URP** asset as the active Render Pipeline (Project Settings → Graphics).
- **Color Space = Linear** (Player settings).
- **Active Input Handling = Input System Package (New)** — the project uses the new Input System.
- Add `Elementborn/ToonLit`, `ToonSky`, `ToonWater`, and `ComfortVignette` to **Always Included Shaders**
  so `Shader.Find` resolves them in a build.
→ `DEPLOYMENT.md` §2.

## 3. The smallest scene that runs (flat mode)
You don't need the whole world wired to confirm the project works:
1. New scene; add an empty **Bootstrap** object.
2. Build a **player rig prefab**: a capsule with `CharacterController`, `Damageable` (DestroyOnDeath
   **off**), `FirstPersonRig`, `PlayerCombatController`, `RespawnController`, the **"Player"** tag, and a
   child Camera. (Exact component list: `DEPLOYMENT.md` §4.)
3. Add a persistent object with `SceneStyleController` (toon sky + lighting), `ScoreController`, `GameHud`,
   and `SaveController`.
4. Put the player rig in the scene (or let `GameBootstrap`, Mode = Flat, instantiate it).
5. Press **Play** → you should move with WASD + mouse, see the HUD, and throw fire by default.

## 4. Add the world (map → terrain → spawns)
1. Add a `WorldMapController` (seed / region count) and a `GameFlowController`; wire the flow's player,
   world-map, and terrain references.
2. Pick **one** terrain component:
   - **`MeshTerrainBuilder`** — the Wind-Waker low-poly mesh with cel bands + outline. Assign it to the
     flow's mesh-terrain slot and set `terrainSize` = your MapSize.
   - or **`TerrainBuilder`** — a standard Unity Terrain.
3. Add `WorldSpawnPlacer` and `StructurePlacer` with the enemy / creature / civilian / lure / weapon-cache
   prefabs (`DEPLOYMENT.md` §4 lists the components each prefab needs).
4. Press Play → character creation → world map → **Enter** → a world with terrain, structures, enemies,
   creatures, and weather.

## 5. Go VR
- Enable **OpenXR** (Single Pass Instanced; add the interaction profiles for your headset). Set
  `GameBootstrap` Mode = VR (or Auto).
- Build the **VR rig prefab** (XR Origin + controllers + the player components) and add the XRI UI module
  so the menus are clickable in-headset.
→ `DEPLOYMENT.md` §3 (XR) and §4 (VR rig + UI input).

## 6. Run the tests / CI
- In-editor: **Window → General → Test Runner** → run EditMode (and PlayMode).
- CI: push to GitHub; the GameCI workflow compiles and runs the tests. Add the
  `UNITY_LICENSE` / `UNITY_EMAIL` / `UNITY_PASSWORD` secrets.
→ `DEPLOYMENT.md` §9.

## Default controls (flat)
WASD move · mouse look · click to cast · **E** interact · **F** element-travel · **M** summon mount ·
**C** summon companions · **Esc** settings · **F5** save · **F9** load · **F8** save slots.

**Gamepad** works alongside keyboard/mouse: left stick move, right stick look, **RT/LT** cast, **A** dash,
**LB** defend, **X** interact, **Y** travel, **d-pad up/down** mount/companions, **Start** settings,
**Select** slots. Every button is **rebindable** in the controls menu (**F10** or **Settings → Controls…**),
and a read-only **Controls** legend with live glyphs opens with **F1**; changes persist. Full reference:
**`INPUT.md`**.

## Where to go next
- **Make it look right:** `ART_GUIDE.md` + `PALETTE.md` + `UI_SPRITES.md`.
- **Sound:** `AUDIO.md` (placeholder SFX + how to replace them).
- **Third-person & consoles:** `PORTING.md` (assign `thirdPersonRigPrefab`, set `preferredFlatMode =
  ThirdPerson`, and point `FlatInputProvider.aimCamera` at the rig camera).
- **Ship it:** `DEPLOYMENT.md` §5–§8.
- **The remaining roadmap:** `WHATS_LEFT.md`.
