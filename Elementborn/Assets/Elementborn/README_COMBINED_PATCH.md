

## v46 additions

### Production-like playable scene and Fire Capital deepening
Added:
- production-style scene polish builder with roads, signs, guide canvas, lava channels, and clearer playable layout
- Fire Capital-specific registry, hooks, interactables, volcano hazard controller, and admin bridge
- Fire Capital citizen roster and five deeper Fire Capital quests
- gameplay loop state machine, spawn registry, spawn points, spawn wave definitions, and main gameplay loop director
- dashboard buttons for loop, waves, Fire Capital hooks, and volcano pressure
- PlayMode smoke tests for gameplay loop, spawn waves, Fire Capital, and dashboard actions


## v47 additions

### Left wrist admin / cheat code UI
Added:
- in-game left-wrist admin panel
- category dropdowns and action dropdowns
- dynamic form fields for admin changes
- cheat-code dropdown section
- raw command input section for legacy admin commands
- admin action catalog and executor
- reflection-based admin command router that calls existing `ExecuteCommand(string)` bridges
- Unity menu to generate/install the wrist UI
- automatic wrist UI installation in the rounded playable scene builder
- EditMode tests for admin catalog, action executor, cheat code, and command router


## v48 additions

### Unity test readiness pass
Added:
- one-click test readiness setup
- scene health/readiness scanner
- Markdown readiness reports
- in-game playtest harness panel
- runtime reset tools
- save-delete tools
- playtest presets/favorites
- first-run onboarding quest
- readiness setup menus
- EditMode tests for scanner/reset/presets/harness teleport


## v49 additions

### Unity import + compile triage kit
Added:
- Unity error intake checklist
- compile issue preflight report
- missing-reference repair/regenerate menu
- expected scene object verifier
- Unity Test Runner guide
- emergency safe mode enable/disable tools
- triage report folder under `Assets/Elementborn/Generated/Reports/Triage`


## v51 additions

### Compile fix pass 2
Added/changed:
- canonical `InteractionArbiter` that does not redefine `IInteractable`
- compatibility `Interaction` struct for old scaffold interactables
- removed stale `Elementborn.Core` using from `ElementbornPolishDebugReportMenu`
- stronger `FIX_V51_COMPILE_ERRORS.ps1` that deletes stale local duplicates before copying
- verified `GearStatModifier` and `GearStatType` presence


## v52 additions

### Compile fix pass 3
Added/changed:
- `PlayerMapMarkerTracker.BuildMarkerId`
- `PlayerMapMarkerTracker.ReportOrUpdateMarker(..., bool isPersistent, string notes)` compatibility overload
- `WindCapitalRegistry` named `notes:` argument fix
- `InteractionArbiter.SignalInteract` compatibility overloads
- canonical `SiteInteriorController`
- canonical `VrInteractInput`
- stronger `FIX_V52_COMPILE_ERRORS.ps1` cleanup/apply script


## v53 additions

### Compile fix pass 4
Added/changed:
- `FIX_V53_COMPILE_ERRORS.ps1` deletes stale local Elementborn `.asmdef` and `.asmref` files.
- `FIX_V53_COMPILE_ERRORS.ps1` normalizes file timestamps to avoid Unity/Bee "timestamp is in the future" rebuild warnings.
- `SiteInteriorController.cs` now imports `Elementborn.Core` for `MapMarkerType` compatibility.


## v54 additions

### Compile fix pass 5
Added/changed:
- `ProjectileCombatEmitter.Emit()`
- `NpcDialogueHookInteractable.SetNpc(...)`
- `NpcWorldIntegrationManifest.Npcs` alias
- `SiteInteriorController.Instance` and enter/exit compatibility overloads
- `FIX_V54_COMPILE_ERRORS.ps1` moves old `Assets\Tests` outside `Assets` to prevent obsolete prototype tests from blocking runtime compilation


## v55 additions

### Site/interior compile fix
Added/changed:
- canonical `SiteInteriorController`
- canonical `SiteEntrance`
- canonical `SiteExit`
- `FIX_V55_COMPILE_ERRORS.ps1` cleanup/apply script
- cleanup for stranded `Assets\Tests.meta`


## v56 additions

### WorldSpawnPlacer compatibility
Added 2-argument `Configure(...)` compatibility overloads to:
- `SiteEntrance`
- `SiteExit`
- `SiteInteriorController`


## v57 additions

### SiteKind Configure compatibility
Added `Configure(SiteKind, string)` overloads to:
- `SiteEntrance`
- `SiteExit`
- `SiteInteriorController`


## v58 additions

### Fully-qualified SiteKind compatibility
Changed site bridge overloads to use `Elementborn.Core.SiteKind` explicitly:
- `SiteEntrance.Configure(Elementborn.Core.SiteKind, string)`
- `SiteExit.Configure(Elementborn.Core.SiteKind, string)`
- `SiteInteriorController.Configure(Elementborn.Core.SiteKind, string)`


## v59 additions

### UI prefab factory compile fix
Added `ElementbornUiPrefabFactory.CreateThemeAsset()` as a compatibility wrapper around `LoadOrCreateTheme()`.


## v60 additions

### Editor-safe playable scene builder
`ElementbornRuntimeBootstrap.EnsureSingleton<T>()` now gates `DontDestroyOnLoad` behind `Application.isPlaying`, allowing editor setup menus such as `Build Rounded Playable Scene` to create runtime system placeholders without throwing edit-mode exceptions.


## v61 additions

### Unity 6 built-in font compatibility
Replaced legacy `Arial.ttf` generated UI font calls with a Unity 6-safe default font helper using `LegacyRuntime.ttf`.


## v62 additions

### Story debug dashboard null-safety
Hardened generated UI font assignment and dashboard setup so `Build Rounded Playable Scene` can continue without null-reference crashes from the Story Systems Debug Dashboard installer.


## v63 additions

### PlayMode UI null-safety
Added a null-safe `PlayerAttunementHud` replacement and `ElementbornTmpFontUtility` to avoid `TMP_Settings.defaultFontAsset` crashes during PlayMode tests.


## v64 additions

### PlayerAttunementHud singleton compatibility
Restored `PlayerAttunementHud.Instance` for older local callers such as `PlayerInventory`, while keeping the v63 null-safe HUD implementation.


## v65 additions

### BoatController Unity 6 API migration
Migrated `BoatController.cs` away from old Rigidbody APIs:
- `velocity` -> `linearVelocity`
- `drag` -> `linearDamping`

This should remove the Unity API updater prompt for `Assets/Elementborn/Game/Boats/BoatController.cs`.


## v66 additions

### HUD save compatibility
Restored `PlayerAttunementHud.CaptureInto(...)` and `PlayerAttunementHud.RestoreFrom(...)` for older local `PlayerInventory` save/restore calls.

### BoatController local stale API safety
The v66 apply script patches any remaining local `_rb.velocity` / `_rb.drag` references in `BoatController.cs` to Unity 6 APIs.


## v67 additions

### Smoke-test restoration
Added minimal EditMode and PlayMode smoke tests under `Assets/Tests/ElementbornSmoke/` so Unity Test Runner discovers real tests again after the older broken tests were removed/quarantined.


## v68 additions

### Smoke-test asmdef duplicate-reference fix
Corrected the v67 smoke-test assembly definitions by removing explicit Test Runner references and relying on `optionalUnityReferences: ["TestAssemblies"]`, which prevents Unity Safe Mode duplicate-reference errors.


## v69 additions

### Pink material / render pipeline shader repair
Added `ElementbornRenderPipelineMaterialUtility` and patched generated scene/material builders so Elementborn no longer creates Built-in `Standard` materials in a URP/HDRP project. Use `Elementborn -> Visuals -> Fix Pink Materials Everywhere` on the currently open scene.


## v70 additions

### Timestamp normalization and TMP warning cleanup
Added a timestamp-normalization apply script for future-dated source/test files and updated `ElementbornTmpFontUtility` to avoid creating TMP font assets from `LegacyRuntime`, which logged repeated font-face warnings.


## v71 additions

### Single EventSystem enforcement
Added `ElementbornEventSystemUtility`, a runtime guard, and an editor repair menu to prevent duplicate EventSystems in generated/playable scenes. Patched known EventSystem creators to route through the centralized utility.


## v72 additions

### EventSystem.current safety
Removed direct `EventSystem.current = ...` assignment from `ElementbornEventSystemUtility`. The utility now repairs/creates a single valid EventSystem with an input module and lets Unity select the current EventSystem naturally.


## v73 additions

### Console / warning cleanup
Added a narrow `Assets/csc.rsp` warning suppression file, removed TMP runtime font-asset creation warnings, disabled routine EventSystem repair logs, and stopped the story dashboard from auto-printing a large report to the Console.


## v74 additions

### Interactable tag repair
Added a direct TagManager repair script, a Unity editor tag repair/report menu, and a safer `NpcWorldIntegrationManager.TryAssignTag(...)` implementation so missing tags do not create dozens of runtime errors.


## v75 additions

### Tag repair menu compile fix
Replaced the broken v74 `ElementbornTagRepairMenu.cs` log line with a compiler-safe implementation while preserving required-tag repair/report functionality.


## v76 additions

### Playable camera/movement rescue
Added a runtime/editor safety layer that guarantees a visible ground plane, camera, capsule player, and WASD movement when the generated prototype scene lacks a usable player controller.


## v77 additions

### Rescue capsule grounding + temporary menu
Fixed the rescue capsule sinking by aligning `CharacterController.center` with the visible capsule mesh and added a temporary prototype menu overlay with start/resume, recenter, and repair actions.


## v78 additions

### 3D text orientation fix
Fixed mirrored/backwards `TextMesh` labels on generated scene signs and added a menu command to repair existing open-scene sign labels.


## v79 additions

### Prototype gameplay loop
Added a self-contained prototype gameplay scene builder with main menu, HUD, player movement, camera follow, one NPC, one resource objective, one turn-in point, and save/load.


## v80 additions

### Reliable prototype interaction
Made prototype interaction distance-based and forgiving, added automatic trigger-radius helpers to interactables, and added repair/report menu commands for existing generated scenes.


## v81 additions

### Elementborn style + elemental ability prototype
Added element choice, Fire/Water/Earth/Air attunement switching, Q-cast elemental bolt, a damageable training dummy, a styled channeler proxy player, an elemental quadrant test arena, and improved dialogue/HUD.


## v82 additions

### Prototype reset + bolt hit feedback
Prevented accidental old completed-save auto-load, added explicit New Prototype/Resume Saved/Clear Save paths, added an editor reset menu, and made elemental bolts reliably hit the training dummy with visible HP/impact feedback.


## v83 additions

### Dummy reset null-reference fix
Made `ElementbornPrototypeDummyEnemy.ResetDummy()` safe in Edit Mode before `Awake()` runs by lazily caching renderers/colliders and defensively resetting dummy state from the editor repair menu.


## v84 additions

### Game-zone prototype upgrade
Added player health/stamina/death/respawn, hostile enemy AI/damage, branch choice with Ember Guide, HUD health/stamina/cooldown indicators, and visible elemental gates around the central hub.


## v85 additions

### Elemental gates + status effects
Added interactive elemental gates, post-shard gate progression, element-specific projectile effects, gate reset handling, and hostile-defeat zone completion.


## v86 additions

### Filled-out prototype zone content
Added envoys, lore stones, resource nodes, healing shrines, loot chests, hub dressing, inventory/progression counters, and a longer zone-completion quest chain.


## v87 additions

### Readable messages + journal
Increased HUD/dialogue message durations, enlarged message/dialogue UI, added a recent message log, added a toggleable quest journal, and added Enter-to-dismiss dialogue.


## v88 additions

### Visual identity + markers + compass
Added floating quest markers, objective compass, better primitive silhouettes, spinning/bobbing pickups, first relic reward state, and more readable world-object guidance.


## v89 additions

### Asset-backed visuals
Copied generated PNG concept/reference assets into the Unity project and created textured in-world art boards around the prototype hub using those assets.


## v90 additions

### Imported Meshy model + procedural animation
Imported the attached Meshy FBX/textures and raw GLB, added procedural bone/root animation support, created imported model prefab builder menu, and replaced the visible hostile capsule with an imported model visual while preserving gameplay collision.


## v91 additions

### Generated asset library + scene decorator
Added a curated generated-FBX extraction pipeline, catalog, prefab builder, scene decorator, and starter mappings to replace prototype placeholders with generated assets when available.


## v92 additions

### Safe generated asset pipeline
Stopped automatic generated-model decoration, added generated-decoration cleanup, added safe opt-in decoration, and added corrupt generated image quarantine/sanitization.


## v93 additions

### Art direction / non-blocky hub pass
Removed oversized debug-like boards/signs from scene build, shrank quest markers, added a clean scene-polisher menu, and added stylized non-cube elemental dressing for the hub.


## v94 additions

### Fuzzy generated asset matching + assignment window
Added renamed-file support for generated Meshy assets, fuzzy extractor/matcher, folder repair menu, and an in-Unity generated asset assignment window for placing or attaching one model at a time.


## v95 additions

### Assignment window compile fix
Added missing `using UnityEditor.SceneManagement;` to `ElementbornGeneratedAssetAssignmentWindow.cs`.


## v96 additions

### Generated visual presets
Added preset-based generated asset placement, a visual presets editor window, report/dry-run style availability checks, and safer cleanup markers for preset-applied models.


## v97 additions

### Generated asset review + approval workflow
Added asset review window, review gallery, approval/rejection database, and approval-only visual preset mode so generated models must be reviewed before presets use them.


## v98 additions

### Clean fantasy hub rebuild
Added a stronger cleanup/rebuild pass that removes debug text and preview clutter, softens the colored test arena, restyles paths/actors/gates, and builds a cleaner procedural fantasy hub as the base visual layer.


## v99 additions

### Reachability + specific treasure chest visuals
Added decorative-collider cleanup so the player can reach objects again, plus a specific TreasureChest_0623200731 prefab builder and menu to apply that visual to every chest while preserving interactions.


## v100 additions

Exact chest/channeler player visual installer with safe fallback behavior.


## v101 additions

### Stable player + chest visual recovery
Added guaranteed visible procedural chests, exact model diagnostics, channeler/axolotl player import attempts, and guaranteed procedural robed player fallback.


## v102 additions

### Safe visual recovery from repo analysis
Overwrote risky V99/V100 model installer menus with safe wrappers, added V102 stable recovery menus, forced visible procedural chest recovery, and guaranteed channeler/robed player fallback.


## v103 additions

### PowerShell label-colon hotfix
Fixed `$Label:` parser errors in exact chest/channeler extraction scripts by changing to `${Label}:`.
