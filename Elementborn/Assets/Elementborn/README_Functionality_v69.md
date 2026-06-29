# v69 Functionality: Pink Material / Render Pipeline Shader Fix

This patch addresses the manual Play result where the scene appears neon pink / magenta.

## Likely cause

Unity renders materials as bright magenta when their shader is missing or unsupported by the active render pipeline.

The generated Elementborn scene builders were still creating some materials with the Built-in Render Pipeline `Standard` shader:

```text
Shader.Find("Standard")
```

In URP/HDRP projects this commonly produces pink materials.

## Changes

### New editor utility

```text
Assets/Elementborn/Editor/ElementbornRenderPipelineMaterialUtility.cs
```

Adds menus:

```text
Elementborn -> Visuals -> Fix Pink Materials Everywhere
Elementborn -> Visuals -> Fix Generated Material Assets
Elementborn -> Visuals -> Fix Open Scene Renderer Materials
```

The utility:

- detects the active render pipeline
- prefers URP Lit / Simple Lit when URP is active
- supports HDRP Lit fallback when HDRP is active
- falls back safely to Standard/Unlit/Sprites shaders
- repairs generated Elementborn material assets under `Assets/Elementborn`
- repairs renderer material slots in the open scene

### Scene builder fixes

Updated generated-material creation in:

```text
ElementbornPlayableSceneBuilder.cs
CapitalLandmarkPrefabGenerator.cs
PlayableSceneProductionPolishBuilder.cs
```

The rounded playable scene builder now repairs materials before saving the generated scene.

## After applying

In Unity run:

```text
Elementborn -> Visuals -> Fix Pink Materials Everywhere
```

Then save the scene/project and press Play again.

If you rebuild the scene with:

```text
Elementborn -> Playable Setup -> Build Rounded Playable Scene
```

the regenerated materials should also be render-pipeline-safe.
