# v24 Static Bug Check

Static checks did not find obvious generated-code hazards.

Checks performed:

- C# brace balance
- C# parenthesis balance
- duplicate generated type names
- accidental Python boolean literals
- explicit compile-error marker check
- obsolete FindObjectOfType / FindObjectsOfType scan

Files patched for object-finding API wrappers: 16

## Patched files

- `Elementborn/Assets/Elementborn/Editor/ElementbornUnityProjectSetupWizard.cs`
- `Elementborn/Assets/Elementborn/Game/Admin/AdminGatheringCommandBridge.cs`
- `Elementborn/Assets/Elementborn/Game/Boats/BoatWindHud.cs`
- `Elementborn/Assets/Elementborn/Game/Bootstrap/BootstrapUiFactory.cs`
- `Elementborn/Assets/Elementborn/Game/Bootstrap/ElementbornRuntimeBootstrap.cs`
- `Elementborn/Assets/Elementborn/Game/Bootstrap/PlayableSceneExampleSeeder.cs`
- `Elementborn/Assets/Elementborn/Game/Bootstrap/UnitySetupRuntimeValidator.cs`
- `Elementborn/Assets/Elementborn/Game/Diagnostics/ElementbornIntegrationDiagnostics.cs`
- `Elementborn/Assets/Elementborn/Game/Interaction/CampInteractable.cs`
- `Elementborn/Assets/Elementborn/Game/Save/BossSaveBridge.cs`
- `Elementborn/Assets/Elementborn/Game/Save/CreatureBondingSaveBridge.cs`
- `Elementborn/Assets/Elementborn/Game/Save/EnemyAiSaveBridge.cs`
- `Elementborn/Assets/Elementborn/Game/Save/GatheringSaveBridge.cs`
- `Elementborn/Assets/Elementborn/Game/Save/InventorySaveBridge.cs`
- `Elementborn/Assets/Elementborn/Game/Save/ShopSaveBridge.cs`
- `Elementborn/Assets/Elementborn/Game/World/WorldMapView.cs`

## Remaining obsolete object-finding references

- `Elementborn/Assets/Elementborn/Game/Setup/ElementbornFindUtility.cs`

## Limitations

- I cannot run the Unity compiler in this environment.
- Unity may still report project-specific integration issues after import.