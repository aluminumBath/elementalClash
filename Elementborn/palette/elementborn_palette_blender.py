"""
Elementborn starter palette -> Blender.

HOW TO USE
  1. Open Blender, switch to the Scripting workspace (top tabs).
  2. Text > Open this file (or paste it into a new text block).
  3. Press Run Script (Alt+P).
  4. Select your mesh, enter Vertex Paint mode, open the brush's Color Palette
     panel, and pick "Elementborn" from the palette dropdown. Click a swatch to
     load it, then flood selected faces with Shift+K.

Colours are the project's authoritative sRGB values (matching TerrainColors and
the toon shaders). Brush/palette colours in Blender are scene-linear, so this
script converts sRGB -> linear; painting onto a Byte-Color attribute then lands
back at the intended sRGB. If a swatch ever looks off, eyedrop from the
reference PNG, which is the source of truth.
"""
import bpy

PALETTE_NAME = "Elementborn"

# (name, (r, g, b)) in 0-255 sRGB
COLORS = [
    ("Plains", (110, 165, 75)),
    ("Capital City", (132, 150, 112)),
    ("Mountains", (130, 120, 110)),
    ("Volcano", (72, 62, 62)),
    ("Desert", (210, 190, 130)),
    ("Forest Temple", (70, 130, 70)),
    ("Swamp", (90, 110, 80)),
    ("Marsh", (110, 120, 85)),
    ("Beach", (225, 210, 160)),
    ("Island", (100, 160, 90)),
    ("Cloud Temple", (180, 190, 205)),
    ("Seabed", (190, 180, 150)),
    ("Sky Top", (51, 115, 217)),
    ("Sky Horizon", (184, 219, 242)),
    ("Sky Ground", (115, 128, 122)),
    ("Sun", (255, 245, 209)),
    ("Cloud / Foam", (255, 255, 255)),
    ("Water Shallow", (77, 179, 189)),
    ("Water Deep", (15, 71, 115)),
    ("Shadow Tint", (128, 140, 179)),
    ("Fire", (220, 90, 45)),
    ("Water", (60, 150, 200)),
    ("Earth", (150, 110, 70)),
    ("Air", (200, 225, 240)),
    ("Ice", (150, 220, 235)),
    ("Lightning", (240, 215, 90)),
    ("Wood", (140, 95, 55)),
    ("Metal", (160, 168, 178)),
    ("Stone", (150, 150, 150)),
    ("Outline", (18, 18, 22)),
    ("Skin", (222, 178, 140)),
    ("Cloth", (90, 100, 120)),
]


def srgb_to_linear(v):
    v = v / 255.0
    return v / 12.92 if v <= 0.04045 else ((v + 0.055) / 1.055) ** 2.4


pal = bpy.data.palettes.get(PALETTE_NAME) or bpy.data.palettes.new(PALETTE_NAME)
pal.colors.clear()
for _name, (r, g, b) in COLORS:
    c = pal.colors.new()
    c.color = (srgb_to_linear(r), srgb_to_linear(g), srgb_to_linear(b))

print(f"[Elementborn] Palette '{PALETTE_NAME}' built with {len(COLORS)} colours.")
