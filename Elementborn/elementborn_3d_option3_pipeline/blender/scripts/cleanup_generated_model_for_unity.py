# cleanup_generated_model_for_unity.py
# Run inside Blender: blender --background --python cleanup_generated_model_for_unity.py -- input.glb output.glb
import bpy, sys, os, math
from pathlib import Path

argv = sys.argv
if '--' in argv:
    args = argv[argv.index('--')+1:]
else:
    args = []

in_path = Path(args[0]) if len(args) > 0 else None
out_path = Path(args[1]) if len(args) > 1 else Path('cleaned_model.glb')

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

if in_path is not None:
    ext = in_path.suffix.lower()
    if ext in ['.glb', '.gltf']:
        bpy.ops.import_scene.gltf(filepath=str(in_path))
    elif ext == '.fbx':
        bpy.ops.import_scene.fbx(filepath=str(in_path))
    elif ext == '.obj':
        bpy.ops.wm.obj_import(filepath=str(in_path))
    else:
        raise RuntimeError(f'Unsupported input: {in_path}')

# Collect mesh objects
meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
for obj in meshes:
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    # Shade flat for cel-shaded look; use shade_smooth manually if needed.
    try:
        bpy.ops.object.shade_flat()
    except Exception:
        pass
    # Add weighted normals for cleaner highlights.
    mod = obj.modifiers.new('WeightedNormals', 'WEIGHTED_NORMAL')
    mod.keep_sharp = True
    obj.select_set(False)

# Center model on origin and put lowest point at z=0
all_meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
if all_meshes:
    min_x=min_y=min_z=1e9
    max_x=max_y=max_z=-1e9
    for o in all_meshes:
        for v in o.bound_box:
            world = o.matrix_world @ bpy.mathutils.Vector(v) if hasattr(bpy, 'mathutils') else o.matrix_world @ __import__('mathutils').Vector(v)
            min_x=min(min_x, world.x); min_y=min(min_y, world.y); min_z=min(min_z, world.z)
            max_x=max(max_x, world.x); max_y=max(max_y, world.y); max_z=max(max_z, world.z)
    cx=(min_x+max_x)/2; cy=(min_y+max_y)/2
    for o in all_meshes:
        o.location.x -= cx
        o.location.y -= cy
        o.location.z -= min_z

# Create simple toon material if no material exists
mat = bpy.data.materials.new('M_Elementborn_Toon_Base')
mat.diffuse_color = (0.35, 0.55, 0.85, 1.0)
for o in all_meshes:
    if not o.data.materials:
        o.data.materials.append(mat)

out_path.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(out_path), export_format='GLB', export_apply=True)
print(f'Exported cleaned GLB: {out_path}')
