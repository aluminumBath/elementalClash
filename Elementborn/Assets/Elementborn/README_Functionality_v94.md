# v94 Functionality: Fuzzy Generated Asset Matching + Assignment Window

v94 accounts for the filename cleanup script you ran.

Your rename script changed generated file names by:
- removing `Meshy_AI_`
- removing timestamps after `3D`
- shortening `image-to-3d-texture` to `texture`
- removing the long quarantine-like path substring
- collapsing underscores

v94 now supports both original and renamed file names.

## New fuzzy extractor

```text
IMPORT_V94_GENERATED_ASSETS_FUZZY.ps1
```

This matches both:

```text
Meshy_AI_Skyotter_3D_0624183855_image-to-3d-texture_fbx.zip
Skyotter_3D_texture_fbx.zip
Skyotter_texture_fbx.zip
```

## Updated Unity matching

The generated asset library builder now normalizes names before matching, so renamed folders/files can still match catalog entries.

New menu:

```text
Elementborn -> Assets -> Repair Generated Asset Folder Names
```

This tries to rename extracted folders back into stable catalog folders like:

```text
AutoImported/Skyotter
AutoImported/FirePhoenix
AutoImported/TreasureChest
```

## New assignment window

```text
Elementborn -> Assets -> Generated Asset Assignment Window
```

Use it to:
- search generated assets
- see whether FBX and prefab are found
- build prefabs
- sanitize imports
- repair folder names
- place one preview in the scene
- assign one generated asset to the selected GameObject
- hide the selected object's blocky renderer if desired
- choose local offset and target height

This replaces the risky “decorate everything” workflow.

## Recommended workflow

After applying v94:

```text
Elementborn -> Assets -> Sanitize Generated Asset Imports
Elementborn -> Assets -> Repair Generated Asset Folder Names
Elementborn -> Assets -> Report Generated Asset Library Matches
Elementborn -> Assets -> Build Generated Asset Library From Extracted FBXs
Elementborn -> Assets -> Generated Asset Assignment Window
```

Then use the window to place/assign one model at a time.

## Why this is safer

v91 placed too many generated assets at once. Some were oversized, some were white due import/material issues, and at least one texture was corrupt.

v94 keeps everything opt-in:
- one asset
- one preview
- one selected object
- adjustable height/offset
- easy cleanup
