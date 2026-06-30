# v98 Functionality: Clean Fantasy Hub Rebuild

v98 is a stronger response to the scene still looking wrong/blocky/debug-like.

## What it fixes

The screenshot showed:
- giant colored flat test quadrants
- road labels/text painted into the world
- too many floating labels
- capsule/cylinder characters
- leftover preview/generated objects
- generated asset experiments visible in the hierarchy
- test-arena rather than game-hub feeling

## New menu

```text
Elementborn -> Visuals -> Remove Debug Text And Preview Objects
Elementborn -> Visuals -> Rebuild Clean Fantasy Hub Look
```

## Builder behavior

The prototype scene builder now calls:

```text
ElementbornPrototypeCleanFantasyHubBuilder.RebuildCleanFantasyHubLook(false)
```

after building the scene.

This replaces the v93 art-direction post-pass.

## What the clean hub pass does

```text
Clears generated preview/review/showcase clutter
Removes most world TextMesh labels
Keeps only small quest markers
Softens the harsh colored quadrant floors
Restyles roads as plain stone paths
Restyles the central convergence platform
Adds painterly/rounded terrain patches
Adds central plaza stone and gold inlay
Adds pebble paths
Adds fire brazier/crystals
Adds water pool/reeds
Adds earth trees/boulders
Adds air wind rings/clouds
Adds village stalls/banners/lanterns
Restyles player/NPC capsule colors
Adds small accessory shapes to characters
Keeps generated FBX models opt-in
```

## Recovery steps

After applying v98:

```text
Elementborn -> Visuals -> Remove Debug Text And Preview Objects
Elementborn -> Prototype -> Build Prototype Gameplay Loop Scene
Elementborn -> Visuals -> Rebuild Clean Fantasy Hub Look
File -> Save
File -> Save Project
```

Then play.

## Important

Do not run generated model presets yet. The base scene needs to look good first. Once the base scene is readable, we can selectively add one approved generated model at a time.
