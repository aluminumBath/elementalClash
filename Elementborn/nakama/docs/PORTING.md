# Elementborn — Third-person & Console Porting

Two questions answered here: **adding a third-person mode** (done in code) and **what it takes to run on Xbox
and PlayStation** (mostly access/SDK/cert work that can't live in this repo).

---

## Third-person perspective (implemented)

A flat third-person camera is now in the codebase next to the first-person and VR rigs. It deliberately
reuses the existing combat pipeline, so no ability code changed.

**Pieces**
- **`ThirdPersonRig`** — `CharacterController` + an orbit camera on a boom (distance/height/pitch), with
  camera-relative WASD, mouse orbit, gravity, a body that turns toward movement, and a short spring that
  keeps the camera out of walls/terrain. It reads mouse sensitivity, invert-Y, and FOV from `SettingsStore`.
- **`GameBootstrap.Mode`** now has `ThirdPerson`, a `thirdPersonRigPrefab` slot, and a `preferredFlatMode`
  (Flat or ThirdPerson). With no XR runtime it spawns the preferred non-VR rig; with XR it still boots VR.
- **`GameFlowController`** finds either `FirstPersonRig` or `ThirdPersonRig` as the movement behaviour to
  gate during menus, so the flow works unchanged.

**Wiring it (Editor)**
1. Build a **third-person rig prefab**: a capsule with `CharacterController`, `Damageable`,
   `PlayerCombatController`, `RespawnController`, the **Player** tag — plus **`ThirdPersonRig`**, a child
   **Camera**, and a visible body mesh (assign it to the rig's `body` field so it turns to face movement).
2. Add **`FlatInputProvider`** and set its **`aimCamera` to the rig camera** — casts go where the camera
   looks. Add **`AbilityVfxBinder`** for visuals/SFX (same as flat/VR).
3. On `GameBootstrap`: assign `thirdPersonRigPrefab` and set `preferredFlatMode = ThirdPerson`.
4. Press Play (no headset) → orbit camera, WASD relative to view, click to cast.

**Notes**
- The comfort vignette is a VR-motion aid; it's harmless in flat third-person and the settings toggle hides
  it anyway.
- Want shoulder-swap, aim-zoom, or lock-on? Those are small additions on top of `ThirdPersonRig`.

---

## Running on Xbox and PlayStation

Short version: the **code ports well** (input is abstracted, URP runs on console, the save is plain JSON), but
the **platform pieces are gated** behind licensed programs, NDA'd SDKs, dev-kit hardware, and certification —
none of which can be shipped in a public repo. The VR build does **not** map to consoles; you'd ship the
**flat / third-person** build (PSVR2 is its own separate target).

### 1. Developer access (the real gate)
- **Xbox** — join **ID@Xbox** (indie) or a full publishing deal, and get a **Partner Center** account.
- **PlayStation** — join **PlayStation Partners** and get approved for PS5 dev.
- Both require agreeing to NDAs before you can even download the tools.

### 2. Tools & hardware (NDA, not downloadable here)
- **Xbox** — the **Microsoft GDK** plus Unity's **Xbox platform module**; an **Xbox dev kit** (or a retail
  console in dev mode for some testing).
- **PlayStation** — Unity's **PS5 platform module** plus Sony's SDK; a **PS5 DevKit/Testkit**.
- These Unity modules are delivered through the platform holders, not via the normal Hub/registry — so they
  can't be added to this project here.

### 3. Code changes (small, because the engine abstracts most of it)
- **Input** — ✅ **done.** A code-built `InputBindings` action map drives keyboard/mouse **and** gamepad
  together (left stick move, right stick look, triggers/buttons/d-pad for cast/defend/dash/interact/etc.), and
  every button is **rebindable** at runtime (`RebindController`). Console controllers map cleanly to the
  gamepad bindings. Full reference: **`INPUT.md`**. Any console-specific glyphs/prompts are an art pass.
- **Saving** — `SaveSystem` writes JSON to `Application.persistentDataPath`. On console you route saves
  through the platform's **save-data API** (provided by the platform module) and obey cert rules (per-user
  save, save icons, no saving during suspend). The slot model already in place maps onto that.
- **Lifecycle** — handle **suspend/resume**, **controller disconnect**, and **multiple users / profiles**.
- **Quality** — fixed hardware means a hard performance target; URP/toon shaders generally need a per-platform
  validation pass.

### 4. Platform services & certification
- **Accounts/profiles, achievements (Xbox) / trophies (PlayStation), store entitlements/DLC, age ratings.**
- Pass **Xbox Requirements (XR)** / Sony **TRC** certification before release.

### 5. VR caveat
- **Xbox has no VR.** **PS5 has PSVR2**, but that's a separate SDK and target — the OpenXR/Quest build won't
  carry over. Plan consoles as the **flat or third-person** experience.

### Bottom line
What I can do in this repo: keep the code portable (done — input abstraction, a full keyboard/mouse **+
gamepad** rebindable scheme, URP, JSON save, a third-person rig). What I **can't** do here: provide the console
SDKs/modules, dev-kit builds, or certification — those need your licensed developer accounts and hardware.
