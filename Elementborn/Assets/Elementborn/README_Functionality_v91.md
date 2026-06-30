# v91 Functionality: Generated Asset Library + Scene Decorator

v91 adds a reusable pipeline for the generated FBX ZIPs already in your local project.

## Source list

The uploaded generated-assets list had:

```text
Parsed entries: 256
ZIP entries: 256
```

The starter catalog maps `34` appropriate assets into roles like:
- player/channeler visuals
- elemental envoys
- hostile/creature showcases
- gates
- chests
- shrines
- resource nodes
- lore/map props
- weapons/gear showcases

## New files

```text
Assets/Elementborn/Data/GeneratedAssets/ElementbornGeneratedAssetCatalog_v91.json
Assets/Elementborn/Data/GeneratedAssets/ElementbornGeneratedAssetCatalog_v91.md
Assets/Elementborn/Game/Prototype/ElementbornPrototypeGeneratedAssetSlot.cs
Assets/Elementborn/Editor/ElementbornGeneratedAssetLibraryBuilder.cs
Assets/Elementborn/Editor/ElementbornGeneratedAssetSceneDecorator.cs
IMPORT_V91_GENERATED_ASSETS.ps1
```

## New Unity menus

```text
Elementborn -> Assets -> Report Generated Asset Library Matches
Elementborn -> Assets -> Build Generated Asset Library From Extracted FBXs
Elementborn -> Assets -> Decorate Open Prototype Scene With Generated Assets
```

The apply script scans:

```text
Elementborn/Assets/generated_assets
```

and extracts a curated starter subset into:

```text
Elementborn/Assets/Elementborn/Art/Models/MeshyImported/AutoImported
```

## Why this approach

The list is large, and blindly extracting/importing every FBX can slow Unity down or bloat the project quickly. v91 gives you a curated first pass, repeatable extraction, prefab generation, and scene decoration.
