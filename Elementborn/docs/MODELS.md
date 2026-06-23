# Elementborn — Placeholder Models

Procedurally generated **medium-poly** placeholder meshes (19 models) for the geometric objects in the world, so you
have stand-ins better than primitives while the real art is modeled. They're faceted and flat-colored with
the project palette. They are **not** a replacement for the organic Blender work (creatures, characters) —
those still need modeling.

Each model ships in two formats, from the same geometry — pick the one for your tool:
- **Unity-ready:** `Assets/Elementborn/Art/Models/*.obj` (+ `*.mtl`) — imports with no plugins; each color
  is a material.
- **Blender (vertex colors):** `Assets/Elementborn/Art/Models/blender_ply/*.ply` — imports with per-vertex
  colors, matching the `ART_GUIDE.md` vertex-color workflow.

## The set

| Model | Tris | Placeholder for |
| --- | --- | --- |
| `crate` | 84 | weapon cache / props (WeaponCache POI, WeaponPickup) |
| `barrel` | 144 | props (villages, markets, camps) |
| `rock` | 192 | terrain scatter |
| `tree` | 360 | forests, plains |
| `bush` | 240 | ground cover |
| `fence` | 48 | camps, villages, farms |
| `house` | 54 | village building (`StructureKind.Village`) |
| `pedestal` | 36 | shrine / pickup plinth (WeaponPickup, LurePickup, Shrine) |
| `sword` | 48 | weapon (`WeaponType.Sword`) |
| `hammer` | 60 | weapon (`WeaponType.Hammer`) |
| `chest` | 60 | treasure / weapon cache (alt to crate) |
| `market_stall` | 144 | Market structures (striped canopy + counter) |
| `tent` | 7 | Camp structures |
| `dock` | 108 | Dock structures (sits over water) |
| `shield` | 240 | weapon (`WeaponType.Shield`) — lies flat, reorient in editor |
| `signpost` | 76 | landmarks / wayfinding |
| `rock_small` | 120 | terrain scatter (small) |
| `pine_tree` | 124 | forests / mountains (conifer variant) |
| `campfire` | 636 | Camp ambiance (stone ring + logs + flames) |

## Scale & orientation
- Units = **meters, Y-up**, resting on the ground plane (base near y = 0) — authored Unity-native. A few thin pieces (`shield`, weapons) sit flat and may need rotating to their held orientation in the editor.

## Importing
- **Unity (OBJ):** drop the `.obj` in; Unity creates a material per color from the `.mtl`. For the toon
  look, switch each material's shader to `Elementborn/ToonLit` (or assign your own). These use **materials**,
  not vertex colors.
- **Blender (PLY):** File ▸ Import ▸ Stanford (.ply); it comes in with **vertex colors**. Some Blender
  versions import PLY Z-up — if it lands on its side, rotate −90° on X, then apply and export FBX to Unity
  as usual (Unity reads the vertex colors; enable **Use Vertex Colors** on the `ToonLit` material).

## Notes
- Faceted, flat-colored, recognizable — **not final art**. Refine or replace them: the PLYs drop into
  Blender with their colors, or model fresh per `ART_GUIDE.md`.
- Pipeline choice: for the `ToonLit` vertex-color path use **PLY → Blender → FBX**; for a fast colored
  drop-in use the **OBJ**.
- Colors are palette hues (wood, metal, stone, leaf, wall, roof) — see `PALETTE.md`.
