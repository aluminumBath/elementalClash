# v92 Functionality: Safe Generated Asset Pipeline

v92 fixes the over-decoration and corrupt-texture import problems from v91.

## What went wrong

v91 proved the generated asset pipeline works, but it was too aggressive:

```text
The scene auto-decorated with too many generated models.
Some imported models were huge or visually white.
At least one extracted Meshy PNG was unreadable/corrupt.
Unity failed to import that unreadable PNG.
```

## What v92 changes

```text
Stops automatic generated-model decoration during prototype scene build
Stops automatic imported axolotl showcase/hostile replacement during build
Adds a cleanup menu for generated decorations
Adds a safe small generated-asset decoration menu
Adds a small showcase-gallery menu
Adds an import sanitizer for corrupt PNG/JPG files
Adds a PowerShell sanitizer that quarantines bad images before Unity opens
```

## New Unity menus

```text
Elementborn -> Assets -> Sanitize Generated Asset Imports
Elementborn -> Assets -> Clear Generated Asset Decorations In Open Scene
Elementborn -> Assets -> Decorate Open Prototype Scene With Safe Generated Assets
Elementborn -> Assets -> Create Small Generated Asset Showcase Gallery
```

The old menu is still present:

```text
Elementborn -> Assets -> Decorate Open Prototype Scene With Generated Assets
```

But it now calls the safe small decoration pass instead of the old aggressive decoration.

## Recommended recovery steps

1. Close Unity.
2. Apply v92.
3. Reopen Unity.
4. Run:

```text
Elementborn -> Assets -> Sanitize Generated Asset Imports
Elementborn -> Assets -> Clear Generated Asset Decorations In Open Scene
Elementborn -> Prototype -> Build Prototype Gameplay Loop Scene
File -> Save
File -> Save Project
```

5. Only after the scene looks clean, optionally run:

```text
Elementborn -> Assets -> Build Generated Asset Library From Extracted FBXs
Elementborn -> Assets -> Decorate Open Prototype Scene With Safe Generated Assets
```

## Corrupt image handling

Bad image files are moved out of `Assets` into:

```text
Elementborn/QuarantinedGeneratedAssetFiles
```

This prevents Unity from repeatedly trying to import broken image files.
