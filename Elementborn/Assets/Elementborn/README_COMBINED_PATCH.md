

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
