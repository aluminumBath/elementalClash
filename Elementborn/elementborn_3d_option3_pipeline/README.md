# Elementborn Option 3 — True 3D Asset Pipeline

This package prepares the v4 polished 2D assets for a true 3D workflow in Blender and Unity.

It does **not** pretend that a single front-facing PNG is enough for game-ready 3D. The correct pipeline is:

1. Pick a high-priority asset from `prompts/hero_asset_priority_manifest.csv`.
2. Generate a front / side / back / 3/4 turnaround using `prompts/turnaround_prompts.md`.
3. Run the best available image-to-3D tool on the turnaround or reference image.
4. Clean the resulting mesh in Blender with `blender/scripts/cleanup_generated_model_for_unity.py`.
5. Export a Unity-ready `.glb` or `.fbx`.
6. Import into Unity using the scripts in `unity/`.

## Recommended scope

Make real 3D models for:

- Player / channeler archetypes
- Named NPCs
- Common enemies
- Bosses
- Tameable creatures
- High-value interactive plants
- Important items/weapons

Keep UI, quest panels, currency icons, and most small inventory art as 2D sprites.

## Included here

- `source_pngs/hero_assets/` — the first 25-ish assets to convert.
- `prompts/turnaround_prompts.md` — prompts for generating model sheets.
- `docs/` — workflow and quality checklist.
- `blender/scripts/` — cleanup/export scripts for generated meshes.
- `unity/` — import helpers and runtime model setup scripts.
- `model_outputs/blockout_glb/` — a few simple low-poly blockout `.glb` files to test your Unity import flow immediately.

## Important art note

Non-channeler characters should be medieval civilian / robe / tunic / cloak designs, not armor. Channelers can have reinforced robes, belts, bracers, charms, and elemental gear, but still should read as magical adventurers rather than plate-armored knights.
