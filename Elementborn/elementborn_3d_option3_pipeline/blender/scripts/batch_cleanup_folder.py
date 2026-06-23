# batch_cleanup_folder.py
# Run inside Blender:
# blender --background --python batch_cleanup_folder.py -- /input_mesh_folder /output_glb_folder
import bpy, sys, subprocess
from pathlib import Path

args = sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
in_dir = Path(args[0]) if args else Path('generated_meshes_input')
out_dir = Path(args[1]) if len(args) > 1 else Path('model_outputs/blender_cleaned_glb')
script = Path(__file__).parent / 'cleanup_generated_model_for_unity.py'

for p in in_dir.iterdir():
    if p.suffix.lower() in ['.glb','.gltf','.fbx','.obj']:
        out = out_dir / f'{p.stem}_cleaned.glb'
        print(f'Clean: {p} -> {out}')
        # This script is designed for one model per Blender process.
        # From a terminal, prefer calling Blender in a loop. Kept here as a reference.
