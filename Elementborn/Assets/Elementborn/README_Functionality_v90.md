# v90 Functionality: Imported Meshy Model + Procedural Animation

v90 starts using the attached 3D model files, not just generated PNG boards.

## Uploaded example inspection

The uploaded GLB was inspected:

```text
Skinned GLB: yes
Approx joint count: 78
Embedded animation clips: 0
```

The uploaded FBX ZIP contained:
- one FBX
- texture map
- emission map
- metallic map
- normal map
- roughness map

## Imported into Unity project

The extracted FBX and textures are copied into:

```text
Assets/Elementborn/Art/Models/MeshyImported/PinkEyeAxolotl/
```

The raw GLB is copied into:

```text
Assets/Elementborn/Art/Models/MeshyImported/RawGlb/
```

## New runtime animation support

Added:

```text
ElementbornPrototypeImportedModelAnimator.cs
ElementbornPrototypeImportedModelTag.cs
```

The procedural animator supports modes:

```text
Idle
Swim
Hover
Combat
HitReact
```

It looks for common rig/bone names and gently animates them. If a model has no useful bone names, it falls back to root bob/tilt motion.

## Prototype scene changes

The builder now:
- attaches the imported Pink Eye Axolotl FBX visual to the Training Hostile
- hides the old hostile capsule renderer while keeping its collider/gameplay logic
- creates an imported model showcase pedestal near the Water area
- adds procedural swim/combat motion to the imported visual

## New editor menu

```text
Elementborn -> Assets -> Build Imported Meshy Example Prefabs
```

This creates example prefabs:

```text
Assets/Elementborn/Generated/Prefabs/ImportedModels/PinkEyeAxolotl_Showcase.prefab
Assets/Elementborn/Generated/Prefabs/ImportedModels/PinkEyeAxolotl_Hostile.prefab
```

## Important animation note

Yes, I can help animate rigged GLB/FBX assets. There are two levels:

1. **Procedural Unity animation**, like this patch:
   - idle breathing
   - swimming/wiggling
   - combat motion
   - hit reactions
   - works even when there are no authored animation clips

2. **Authored animation clips/controllers**, which works best when:
   - the skeleton has stable bone names
   - the model is humanoid or has a documented creature rig
   - you can provide T-pose/rest-pose screenshots or bone hierarchy
   - we decide target clips: idle, walk, run, attack, hit, death, cast, swim, fly

For Meshy creature rigs with generic `Bone_###` names, procedural animation is usually the fastest first step.
