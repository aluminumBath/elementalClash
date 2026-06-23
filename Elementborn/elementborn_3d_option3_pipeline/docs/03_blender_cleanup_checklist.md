# 03 — Blender Cleanup Checklist

For every generated mesh:

1. Import model into Blender.
2. Apply scale/rotation transforms.
3. Center origin at feet/base.
4. Delete floating fragments.
5. Remove internal junk geometry.
6. Merge close vertices.
7. Fix normals: outside facing.
8. Decimate or retopologize to target triangle count.
9. Split moving parts into named objects.
10. Assign toon/cel materials.
11. Bake or repaint textures if the generated texture is muddy.
12. Add armature if animated.
13. Test deformation with idle/walk/attack poses.
14. Export `.glb` for simple textured models or `.fbx` for complex animation pipelines.

## Triangle targets

- Small item: 300–2,000 tris
- Inventory prop: 1,000–5,000 tris
- NPC / channeler: 5,000–20,000 tris after cleanup
- Common creature: 3,000–15,000 tris
- Boss: 20,000–60,000 tris depending platform

For Quest/VR, stay lower and prioritize silhouette over tiny texture details.
