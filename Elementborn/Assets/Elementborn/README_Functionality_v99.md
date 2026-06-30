# v99 Functionality: Reachability + Specific Treasure Chest Visuals

v99 fixes the current scene problem where the player cannot reach objects, and it adds a dedicated installer for the requested treasure chest model.

## Problems addressed

The prior clean fantasy hub pass added lots of decorative primitive shapes. Some of those still had colliders, so they could block the player.

The scene also still had blocky chest placeholders.

## Reachability fix

New menu:

```text
Elementborn -> Visuals -> Repair Prototype Reachability
```

This disables colliders on decorative-only visual objects while preserving:
- core floor colliders
- road colliders
- player controller
- interactable triggers

It specifically repairs decorative roots like:

```text
Elementborn Clean Fantasy Hub
Elementborn Art Direction Pass
Generated Asset Safe Decoration
Generated Asset Review Gallery
```

## Specific treasure chest visual

Requested visual:

```text
Meshy_AI_Treasure_Chest_0623200731_texture_fbx
```

New extractor:

```text
IMPORT_V99_TREASURE_CHEST_0623200731.ps1
```

It searches:

```text
Elementborn/Assets/generated_assets
```

and extracts the matching ZIP to:

```text
Elementborn/Assets/Elementborn/Art/Models/MeshyImported/AutoImported/TreasureChest_0623200731
```

New Unity menus:

```text
Elementborn -> Assets -> Build Specific Treasure Chest Prefab
Elementborn -> Assets -> Apply Specific Treasure Chest Visuals To All Chests
Elementborn -> Visuals -> Repair Reachability And Chest Visuals
```

The prefab is built at:

```text
Assets/Elementborn/Generated/Prefabs/ImportedModels/Approved/TreasureChest_0623200731.prefab
```

## What it does to chests

For every chest interactable:
- hides the old blocky renderer
- hides old blocky child/lid renderers
- keeps the parent object and interaction trigger
- attaches the specific Meshy treasure chest model
- disables colliders on the imported visual so it does not block movement

## Recommended Unity order

```text
Elementborn -> Visuals -> Repair Prototype Reachability
Elementborn -> Assets -> Build Specific Treasure Chest Prefab
Elementborn -> Assets -> Apply Specific Treasure Chest Visuals To All Chests
Elementborn -> Visuals -> Rebuild Clean Fantasy Hub Look
File -> Save
File -> Save Project
```

If you rebuild the prototype scene later, the clean hub pass now also tries to repair reachability and reapply chest visuals.
