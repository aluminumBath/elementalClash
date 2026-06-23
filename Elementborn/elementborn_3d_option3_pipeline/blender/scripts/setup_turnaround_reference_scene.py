# setup_turnaround_reference_scene.py
# Run inside Blender after placing front/side/back PNGs in a folder:
# blender --python setup_turnaround_reference_scene.py -- /path/to/turnaround_pngs
import bpy, sys
from pathlib import Path

folder = Path(sys.argv[sys.argv.index('--')+1]) if '--' in sys.argv else Path('.')
front = folder/'front.png'
side = folder/'side.png'
back = folder/'back.png'

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

# Enable image planes add-on if available
try:
    bpy.ops.preferences.addon_enable(module='io_import_images_as_planes')
except Exception:
    pass

def add_plane(path, loc, rot=(0,0,0), name='ref'):
    if not path.exists():
        print(f'Missing: {path}')
        return
    try:
        bpy.ops.import_image.to_plane(files=[{'name': path.name}], directory=str(path.parent), relative=False)
        obj = bpy.context.object
        obj.name = name
        obj.location = loc
        obj.rotation_euler = rot
    except Exception:
        # fallback empty image reference
        img = bpy.data.images.load(str(path))
        empty = bpy.data.objects.new(name, None)
        empty.empty_display_type = 'IMAGE'
        empty.data = img
        empty.location = loc
        bpy.context.collection.objects.link(empty)

add_plane(front, (0, 2.5, 1.5), (1.5708,0,0), 'REF_front')
add_plane(side, (2.5, 0, 1.5), (1.5708,0,1.5708), 'REF_side')
add_plane(back, (0, -2.5, 1.5), (1.5708,0,3.14159), 'REF_back')

# Add guide grid / unit cube
bpy.ops.mesh.primitive_cube_add(size=2, location=(0,0,1))
cube = bpy.context.object
cube.name = 'scale_reference_2m_cube'
cube.display_type = 'WIRE'

bpy.ops.wm.save_as_mainfile(filepath=str(folder/'turnaround_reference_scene.blend'))
