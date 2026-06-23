# Bootstrap generator — one click to a playable scene

Unity scenes and prefabs are Editor-authored binary assets; they can't be written as source the way the rest of
this project is. So instead of shipping a `.unity` you can't diff, the project ships a **generator** that builds
one for you. It lives in `Assets/Elementborn/Editor/BootstrapSceneGenerator.cs` (an Editor-only assembly) and
adds an **Elementborn ▸ Bootstrap** menu.

## The menu

1. **Build Player Rig Prefabs** — creates three rigs in `Assets/Elementborn/Prefabs/`:
   - `PlayerRig_FirstPerson` — `CharacterController` + `FirstPersonRig` + a child `RigCamera`, plus
     `PlayerCombatController` (wired to a `FlatInputProvider` and a `WeaponHolder`), `PlayerInteractor`, and
     `PlantControlController`.
   - `PlayerRig_ThirdPerson` — the same, with `ThirdPersonRig` and the boom camera behind a visible body capsule.
   - `PlayerRig_VR` — a minimal head-camera rig with `VrInputProvider` + combat/interaction. **Starting point
     only:** VR needs the XR plugin enabled and controller poses bound (see below).
   Adding `PlayerInteractor` brings the `InteractionArbiter` onto the rig automatically, so all the world/NPC/
   sidekick/plant interactions route through one prompt.

2. **Build Playable Scene** — creates `Assets/Elementborn/Scenes/Bootstrap.unity`: a directional sun + ambient
   light, ground (the `MeshTerrainBuilder` if present — it builds itself on Start — otherwise a large plane), a
   spawn point, and one GameObject carrying **`GameBootstrap`** (the three rig prefabs assigned, flat mode set to
   third-person) and **`GameFlowController`**. Builds the rig prefabs first if they're missing. Press **Play**:
   `GameBootstrap` spawns the right rig for the active mode, and `GameFlowController` runs Boot → character
   creation → world.

3. **Build Everything + Add To Build Settings** — both of the above, and inserts the scene at index 0 of Build
   Settings so a player build launches straight into it.

## How it stays safe without a compiler

The generator never hard-references game types. It finds components by name through `TypeCache` and sets
serialized fields by string through `SerializedObject`. Anything it can't resolve (a renamed component, a field
that moved) is logged as a **"wire it manually"** warning instead of throwing — the rest of the scene still
builds. So if you refactor a rig later, re-running the menu still gets you 90% of the way and tells you exactly
what to finish in the Inspector.

## What you may still finish by hand

- **VR:** enable an XR plug-in under *Project Settings ▸ XR Plug-in Management*, then bind the controller poses
  and the gesture inputs the `VrInputProvider`/`VrGestureProvider` expect. The flat and third-person rigs need
  none of this and are playable immediately.
- **Materials:** handled automatically. The generator builds cel-shaded `.mat` assets from the toon shaders
  (Elementborn ▸ Materials ▸ Build Cel-Shaded Materials, or built on demand when you generate rigs) and applies a
  character material to the rig body and a vertex-coloured ground material to the fallback plane. Sky/sun/ambient/
  fog come from a `SceneStyleController` the generator drops in the scene. Swap or retint the materials in
  `Assets/Elementborn/Materials` to taste.
- **A real character model:** the rig body is a capsule placeholder. Drop a model under the rig and (for VR/3rd
  person) point the rig's `body` field at it.
- **Anything logged as not-found:** check the Console after running the menu; each warning names the component or
  field to wire.

This is the answer to the "no scenes/prefabs in the repo" limitation: the binaries are one menu click away, and
the wiring is reproducible instead of hand-built.
