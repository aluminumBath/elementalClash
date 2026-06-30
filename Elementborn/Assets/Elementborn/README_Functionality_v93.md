# v93 Functionality: Art Direction / Non-Blocky Hub Pass

v93 responds to the prototype still feeling too blocky and debug-like.

## Problems addressed

The scene had:
- huge ART markers
- large reference boards
- oversized text signs
- square market stalls
- blocky landmarks
- cluttered generated model decoration
- markers that dominated the view

## What v93 changes

```text
Stops asset boards from being created during prototype scene build
Stops huge instruction/control signs from being created during scene build
Adds a clean art-direction scene polisher
Shrinks quest markers
Shrinks/normalizes TextMesh labels
Removes visual clutter from earlier generated-asset passes
Adds non-cube elemental dressing
Adds central city silhouette dressing
Adds fire lava/basalt/crystals
Adds water pools/reeds/crystals
Adds earth trees/rocks/crystals
Adds air cloud puffs/wind rings/crystals
Adds rounded stalls, shrine frames, chest pedestals
Adds NPC accessories and hostile details
```

## New Unity menus

```text
Elementborn -> Visuals -> Apply Prototype Art Direction Pass
Elementborn -> Visuals -> Clean Prototype Visual Clutter
```

## Recommended recovery flow

After applying v93:

```text
Elementborn -> Assets -> Sanitize Generated Asset Imports
Elementborn -> Assets -> Clear Generated Asset Decorations In Open Scene
Elementborn -> Prototype -> Build Prototype Gameplay Loop Scene
File -> Save
File -> Save Project
```

The prototype builder now automatically applies the art-direction pass after building the scene.

## Design direction

This is still not final art and not a full replacement for real modeled prefabs, but it should feel much less like a pure blockout. It moves the scene toward:

```text
stylized elemental fantasy hub
readable game slice
smaller markers
less debug text
more organic silhouettes
clearer elemental regions
```

## Next milestone

v94 should start replacing specific hero/NPC/creature visuals one at a time with curated generated FBX prefabs after each model is manually verified for:
- scale
- orientation
- material import
- texture validity
- bone hierarchy
- performance
