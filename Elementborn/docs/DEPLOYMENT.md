# Elementborn — Build, Run & Deployment Guide

This is the step-by-step for getting Elementborn from source onto a screen — flat PC first (fastest
to preview), then PCVR, then Meta Quest standalone. One codebase serves all three; `GameBootstrap`
detects at runtime whether an XR display is active and picks VR or flat input automatically.

> **Important:** the repo ships **code, shaders, tests, and CI** — not scenes, prefabs, materials, or
> terrain art. Nothing renders until you create a bootstrap scene and a few prefabs in the editor
> (covered in §4). That's the one-time setup that turns the scaffold into something you can press Play on.

---

## 1. Requirements

### Editor
- **Unity 6 LTS (6000.0.x).** Install through **Unity Hub**; pick the latest `6000.0` LTS patch.
- Add these **modules** during install (Hub → Installs → gear → Add Modules):
  - **Android Build Support** → check **Android SDK & NDK Tools** and **OpenJDK** (required for Quest).
  - **Windows Build Support (IL2CPP)** — for PCVR and flat Windows builds.
  - **Mac** or **Linux Build Support** — only if you want a flat build on those platforms.

### Packages (already pinned in `Packages/manifest.json`)
```
com.unity.render-pipelines.universal  17.0.3
com.unity.inputsystem                 1.11.2
com.unity.xr.management               4.5.0
com.unity.xr.interaction.toolkit      3.0.7
com.unity.xr.openxr                   1.13.0
com.unity.ugui                        2.0.0
com.unity.test-framework              1.4.5
```
Unity resolves these on first open. If Package Manager flags a mismatch for your editor version, let it
update to the closest compatible patch.

### Hardware / accounts
- **Quest build:** a Meta Quest 2 / 3 / Pro, a USB-C cable, and a **Meta developer account** (free) with
  an organization, so you can enable Developer Mode.
- **PCVR:** a headset plus its runtime (SteamVR or the Meta/Oculus PC app).
- ~10 GB free for the editor + Android tooling; 16 GB RAM comfortable.

### Tooling
- **Git** and **Git LFS** (`git lfs install`) — LFS matters once you add art (textures, models, audio).

---

## 2. First-time project setup

1. Clone and open the folder in the matching Unity version (Hub → Open → select the project root).
2. Let the first import finish (it compiles the assemblies and pulls packages).
3. **Render pipeline:** Project Settings → Graphics should reference a **URP asset**. If none exists yet:
   - Right-click in Project → Create → Rendering → **URP Asset (with Universal Renderer)**. Make one for
     **PC** and a lighter one for **Mobile/Quest** (lower shadow distance, MSAA 2×, no soft shadows).
   - Assign them under Project Settings → **Quality** (per level) and Graphics → Default Render Pipeline.
4. **Color space:** Project Settings → Player → Other Settings → **Color Space = Linear**.
5. **Input:** Project Settings → Player → Active Input Handling = **Input System Package (New)** (or **Both**).
6. **Run the tests** to confirm a clean compile: Window → General → **Test Runner** → EditMode → Run All.

---

## 3. XR / OpenXR configuration

1. Project Settings → **XR Plug-in Management** → **Install XR Plugin Management** if prompted.
2. **Android tab:** enable **OpenXR**, then enable the **Meta Quest** feature group.
3. **PC (Windows) tab:** enable **OpenXR**.
4. Click **OpenXR** under XR Plug-in Management and configure **Interaction Profiles**:
   - Android: **Oculus Touch Controller Profile**.
   - PC: add the profiles for the headsets you target (Oculus Touch, Valve Index, HTC Vive, Windows MR).
5. **Render Mode:** set OpenXR → **Single Pass Instanced** (big perf win on Quest).
6. On Android, make sure the **Meta Quest Support** OpenXR feature is checked (it provides the loader).

---

## 4. Project-specific scene setup (the one-time editor work)

Elementborn builds its menus, map, HUD, terrain, structures, and spawns from code — but those scripts
need a scene to live in and a handful of prefabs to instantiate.

1. **Bootstrap scene:** File → New Scene (Basic URP). Save as `Assets/Scenes/Bootstrap.unity`. Add it to
   File → Build Settings → **Scenes In Build**.
2. **Flow object:** create an empty GameObject `GameFlow`, add **`GameFlowController`**. Also add
   **`ScoreController`** and **`SceneStyleController`** (assign the sun light + your ToonSky material).
3. **World object:** create `World`, add **`WorldMapController`**, **`TerrainBuilder`** (assign a Unity
   Terrain or let it create one; assign the `WaterSurface` + ToonWater material), **`StructurePlacer`**,
   and **`WorldSpawnPlacer`**. Wire these into `GameFlowController`'s serialized fields.
4. **Player rig prefab:** an empty `Player` with **CharacterController**, **`Damageable`** (turn
   **DestroyOnDeath off**), **`FirstPersonRig`**, **`PlayerCombatController`**, **`WeaponHolder`**, and a
   **`RespawnController`**. Tag it **`Player`**. Parent the Main Camera (flat) / XR Origin (VR) under it.
5. **Enemy prefab:** a capsule with **CharacterController**, **`Damageable`**, and **`EnemyController`**.
   Assign it to `WorldSpawnPlacer.enemyPrefab` (the placer sets each one's archetype).
6. **Weapon pickup prefab:** a trigger collider + **`WeaponPickup`**; assign to the placer.
7. **Materials:** create materials from `Elementborn/ToonLit`, `Elementborn/ToonSky`, `Elementborn/ToonWater`
   and assign them (sky → SceneStyleController, water → WaterSurface, ToonLit is the default for
   `ToonPalette`).
8. Press **Play** — you should land in character creation (flat: mouse + WASD), pick an element, see the
   map, enter the world, and fight. **This is your preview.**

---

## 5. Building — flat PC (fastest preview)

1. File → Build Settings → **Windows/Mac/Linux**, Target = your OS, Architecture x86_64.
2. (Optional) Player → Other Settings → Scripting Backend **Mono** for faster iteration builds.
3. **Build** (or **Build And Run**). With no VR runtime active, `GameBootstrap` starts in flat mode.

---

## 6. Building — PCVR

1. Same Windows target as §5, but XR is already enabled (OpenXR PC from §3).
2. Set your **active OpenXR runtime** so the headset is driven by the right stack:
   - **SteamVR:** Settings → OpenXR → *Set SteamVR as the active OpenXR runtime*.
   - **Meta/Oculus PC app:** Settings → General → *OpenXR Runtime → Set Oculus as active*.
3. Build the `.exe`, start your runtime, then launch the build. `GameBootstrap` detects the HMD and uses
   VR input.

---

## 7. Building — Meta Quest (standalone Android)

### 7a. Player settings for Android
- Player → Other Settings:
  - **Scripting Backend:** IL2CPP. **Target Architectures:** **ARM64** only (uncheck ARMv7).
  - **Graphics APIs:** **Vulkan** (remove OpenGLES3 unless you need a fallback).
  - **Minimum API Level:** Android 10 (API 29) minimum; **target the highest installed**.
  - **Texture compression:** **ASTC**.
  - **Package Name:** `com.YourCompany.Elementborn`.
- Player → Resolution and Presentation: **Landscape Left**.
- Use the **Mobile URP asset** for the Android quality level.

### 7b. Enable the headset for development
1. In the **Meta Horizon** phone app: pair the headset → Headset Settings → **Developer Mode → On**.
2. Put the headset on, accept the **USB debugging** prompt when you plug into the PC.

### 7c. Build and install
- Easiest: File → Build Settings → **Android** → **Switch Platform** → connect headset → **Build And Run**.
- Or build an APK and sideload it:
  ```bash
  adb devices                         # confirm the headset shows as 'device'
  adb install -r Elementborn.apk      # -r reinstalls/updates
  # to remove:
  adb uninstall com.YourCompany.Elementborn
  # to watch logs while it runs:
  adb logcat -s Unity Elementborn
  ```
- In the headset, find it under **Apps → Unknown Sources**.

---

## 8. Signing (Android / Quest)

1. Player → Publishing Settings → **Keystore Manager → Create New**. Make a `.keystore`, add a key, set
   both passwords. Save the keystore **outside** the repo and back it up — **the same key is required for
   every future update**.
2. Or generate one with the JDK:
   ```bash
   keytool -genkeypair -v -keystore elementborn.keystore -alias elementborn \
     -keyalg RSA -keysize 2048 -validity 10000
   ```
3. For **sideloading / dev**, the debug key is fine. For the **Meta Horizon Store**, follow Meta's app
   submission (upload via the Meta Quest Developer Hub / Developer Dashboard, which manages store signing).
4. **Every Android update must increase `bundleVersionCode`** (Player → Other Settings) or the install
   will be rejected as a downgrade.

---

## 9. Continuous integration (GameCI)

`.github/workflows/tests.yml` already runs EditMode + PlayMode tests on push/PR via
`game-ci/unity-test-runner`, caching `Library/` between runs.

**Secrets to add** (repo → Settings → Secrets and variables → Actions):
- `UNITY_LICENSE` — your Personal license `.ulf` contents (activate once via the GameCI activation flow),
  plus `UNITY_EMAIL` and `UNITY_PASSWORD`.

**Optional build job** (add to a workflow) once you're ready to automate builds:
```yaml
  build:
    needs: test
    runs-on: ubuntu-latest
    strategy:
      matrix:
        targetPlatform: [StandaloneWindows64, Android]
    steps:
      - uses: actions/checkout@v4
        with: { lfs: true }
      - uses: actions/cache@v4
        with:
          path: Library
          key: Library-${{ matrix.targetPlatform }}-${{ hashFiles('Assets/**','Packages/**','ProjectSettings/**') }}
          restore-keys: Library-${{ matrix.targetPlatform }}-
      - uses: game-ci/unity-builder@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        with:
          targetPlatform: ${{ matrix.targetPlatform }}
      - uses: actions/upload-artifact@v4
        with:
          name: Build-${{ matrix.targetPlatform }}
          path: build
```
For **signed** Android in CI, also pass the keystore via secrets (`ANDROID_KEYSTORE_BASE64`,
`ANDROID_KEYSTORE_PASS`, `ANDROID_KEYALIAS_NAME`, `ANDROID_KEYALIAS_PASS`) — see the unity-builder docs.

---

## 10. Quest performance checklist
- **Single Pass Instanced** rendering (§3) and **Vulkan + ARM64 + ASTC** (§7a).
- Target **72 Hz** first; raise to 90/120 only with headroom.
- Keep real-time lights/shadows minimal — **bake** static lighting; the toon shaders are cheap, but watch
  **transparent overdraw** from the water.
- **Mesh-combining is already wired** (`StructurePlacer` → `MeshCombiner`), so towns are a few draw calls.
- Enable **Fixed Foveated Rendering** (Meta OpenXR settings / OVR) for extra GPU savings.
- Budget rough targets: keep it well under ~100 draw calls and ~250k tris visible per eye on Quest 2.

---

## 11. Troubleshooting
- **Everything's magenta/pink:** URP isn't the active pipeline, or shaders weren't compiled for the target.
  Confirm §2.3 and that the ToonLit/Sky/Water shaders are in the build.
- **Black screen on Quest:** Graphics API isn't Vulkan, the XR loader (Meta Quest) isn't enabled, or the
  min API level is too low. Re-check §3 and §7a.
- **Controllers don't track / no input:** missing **Interaction Profile** (§3.4), or Active Input Handling
  isn't set to New/Both (§2.5).
- **App not in the headset:** it's under **Unknown Sources**, and the APK must be signed (§8).
- **IL2CPP / NDK build errors:** the Android NDK module wasn't installed with the editor (§1).
- **Missing textures/models after clone:** run `git lfs pull`.

---

## 12. Release flow (summary)
1. Bump version (and `bundleVersionCode` for Android).
2. Run tests (CI does this automatically).
3. Build per target (§5–7); sign Android (§8).
4. Smoke-test on a real device.
5. Distribute: sideload (dev), Meta Horizon Store (Quest), or your channel of choice (PCVR/flat).
