# 02 — Tool Recommendations

## Best open-source candidates

### Hunyuan3D 2.1 / 2.x

Best candidate for higher-quality textured assets if your hardware can run it. Use for characters, creatures, props, and bosses. Expect cleanup.

### TripoSR

Fast, open-source single-image-to-3D reconstruction. Good for quick mesh experiments and blockouts. Expect less production polish and more cleanup.

## Suggested use by asset type

| Asset type | Best first method | Notes |
| --- | --- | --- |
| NPCs / Channelers | Turnaround + Hunyuan3D, then Blender cleanup | Need rig-friendly anatomy. |
| Creatures | Turnaround + Hunyuan3D; hand-clean appendages | Tentacles/fins/horns often need manual work. |
| Bosses | Artist-assisted Blender modeling recommended | AI mesh is best as concept/blockout. |
| Plants | Hunyuan3D or hand-model from concept | Moving gameplay parts should be separate objects. |
| Items / Stones | Direct 3D modeling in Blender or image-to-3D | Easier than characters. |
| UI sprites | Keep 2D | No need for true 3D. |

## Practical warning

AI-generated meshes are rarely game-ready. Common problems:

- Too many triangles
- Messy topology
- Non-manifold geometry
- Painted-on details that should be actual geometry
- Bad hands/fingers/faces
- Hidden/back side invented incorrectly
- Hard to rig without retopology

For Unity, treat raw output as **draft geometry**, not a final production mesh.
