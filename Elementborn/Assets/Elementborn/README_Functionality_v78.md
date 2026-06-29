# v78 Functionality: 3D Text Orientation Fix

The generated 3D sign objects had valid geometry and bounds, but their `TextMesh` labels could appear backwards.

## Root cause

`PlayableSceneProductionPolishBuilder.CreateSign(...)` created labels with:

```text
localRotation = Quaternion.Euler(0, 180, 0)
```

That mirrored the text.

## Changes

Future generated sign labels now use:

```text
localRotation = Quaternion.identity
localScale = Vector3.one
```

v78 also adds:

```text
Elementborn -> Visuals -> Fix Backwards 3D Text Labels
Elementborn -> Visuals -> Report 3D Text Labels
```

The repair menu searches existing scene `TextMesh` sign labels and resets their local rotation/scale so the text is readable.
