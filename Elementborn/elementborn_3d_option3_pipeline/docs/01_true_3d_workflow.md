# 01 — True 3D Workflow

## Why turnarounds come first

The v4 PNGs are excellent concept art, but a single image only shows one face of the object. Image-to-3D systems invent the hidden back and side surfaces. For production use, especially for NPCs and creatures, create a consistent model sheet first:

- Front view
- Left side view
- Right side view if asymmetrical
- Back view
- 3/4 beauty view
- Optional expression/pose reference

For humanoids, generate a neutral A-pose/T-pose version. The expressive portrait is still useful for personality, but the mesh generator and rigger need clean limbs and visible clothing structure.

## Tool route

### Open-source/local route

1. Generate model sheet views using your preferred image generator or paint-over process.
2. Run Hunyuan3D 2.x or TripoSR on the best reference.
3. Clean up in Blender.
4. Export GLB/FBX.
5. Import into Unity.

### Artist-assisted route

1. Use the v4 image + turnaround prompt as a brief.
2. A modeler builds a low-poly/cel-shaded mesh in Blender.
3. Texture by hand or bake from the concept art.
4. Rig and animate.

This is slower but gives much better results for player characters and bosses.

## Asset rules

- Characters: humanoid A-pose, separate hair/cloak if possible, clear face loops.
- Creatures: simplified but believable anatomy, separate tentacles/fins/horns where rigged.
- Plants: separate moving parts: jaws, vines, petals, gate curtain.
- Items: single static mesh unless animated.
- UI: do not convert to 3D unless it is an in-world prop.
