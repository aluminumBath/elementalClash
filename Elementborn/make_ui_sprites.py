#!/usr/bin/env python3
"""make_ui_sprites.py — regenerate the flat 2D UI sprite set into Assets/Elementborn/Art/UI/.

Parity with make_sfx.py / make_glyphs.py: the committed UI sprites are placeholders, and this is the source that
reproduces them, so the 2D look can be restyled by editing this script and re-running. Sizes / 9-slice borders /
colors follow docs/UI_SPRITES.md and docs/GENERATED_ART.md. PNGs are straight-alpha; author size is kept modest
(the borders are what matter for 9-slicing). Run: python3 make_ui_sprites.py

NOTE: these are 2D placeholders only — not the 3D meshes/VFX. And Unity still needs the one-time in-editor import
(Texture Type = Sprite, Border = the listed inset) before the UI uses them; see docs/GENERATED_ART.md.
"""
import os
from PIL import Image, ImageDraw

OUT = os.path.join("Assets", "Elementborn", "Art", "UI")

# palette (UI_SPRITES.md): dark surface, accent blue, trims, gems
SURFACE      = (30, 33, 40, 240)      # #1E2128
SURFACE_EDGE = (78, 86, 102, 255)
BLUE         = (64, 153, 242, 255)    # ~ rgba(0.25,0.6,0.95)
BLUE_HI      = (104, 183, 255, 255)
BLUE_LO      = (40, 104, 176, 255)
ROW_BLUE     = (56, 128, 216, 235)    # ~ rgba(0.22,0.5,0.85)
GRAY         = (96, 104, 118, 220)
WARM         = (255, 242, 179, 255)   # #FFF2B3
GREEN        = (184, 255, 204, 255)   # #B8FFCC
WHITE        = (255, 255, 255, 255)
GEMS = {
    "gem_diamond":  (191, 233, 245, 255),
    "gem_sapphire": (60, 150, 200, 255),
    "gem_emerald":  (70, 160, 80, 255),
    "gem_ruby":     (200, 60, 60, 255),
    "gem_silver":   (160, 168, 178, 255),
}


def _canvas(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def frame(name, border, fill, edge, scale=2):
    """A rounded 9-slice frame: corners fit inside the border so slicing stays clean. Authored at `scale`x."""
    b = border * scale
    center = 16 * scale
    size = b * 2 + center
    img = _canvas(size, size)
    d = ImageDraw.Draw(img)
    radius = max(2, b - 2 * scale)
    margin = 2 * scale  # transparent margin so the border isn't clipped
    d.rounded_rectangle([margin, margin, size - 1 - margin, size - 1 - margin],
                        radius=radius, fill=fill, outline=edge, width=max(2, scale))
    img.save(os.path.join(OUT, name + ".png"))


def solid_round(name, color, d_px=56):
    img = _canvas(d_px, d_px)
    dr = ImageDraw.Draw(img)
    dr.ellipse([2, 2, d_px - 3, d_px - 3], fill=color)
    img.save(os.path.join(OUT, name + ".png"))


def gem(name, color, d_px=56):
    """A simple faceted gem (diamond silhouette) in the currency hue."""
    img = _canvas(d_px, d_px)
    dr = ImageDraw.Draw(img)
    cx = d_px / 2
    pts = [(cx, 4), (d_px - 6, d_px * 0.42), (cx, d_px - 4), (6, d_px * 0.42)]
    dr.polygon(pts, fill=color, outline=(255, 255, 255, 160))
    # a light top facet for a touch of dimension
    dr.polygon([(cx, 4), (d_px - 6, d_px * 0.42), (cx, d_px * 0.42)],
               fill=(255, 255, 255, 70))
    img.save(os.path.join(OUT, name + ".png"))


def main():
    os.makedirs(OUT, exist_ok=True)
    # 9-slice frames (name, border) — borders match GENERATED_ART.md
    frame("panel", 24, SURFACE, SURFACE_EDGE)
    frame("map_frame", 24, (26, 31, 38, 255), SURFACE_EDGE)
    frame("map_bg", 18, (18, 20, 26, 255), (40, 46, 56, 255))
    frame("hud_bar", 16, SURFACE, SURFACE_EDGE)
    frame("hud_chip", 12, SURFACE, SURFACE_EDGE)
    frame("hud_prompt", 16, (24, 26, 32, 235), WARM)
    frame("hud_toast", 16, (24, 26, 32, 235), GREEN)
    frame("btn_row", 14, ROW_BLUE, (90, 140, 200, 255))
    frame("btn_normal", 16, BLUE, (120, 180, 240, 255))
    frame("btn_highlighted", 16, BLUE_HI, (170, 210, 255, 255))
    frame("btn_pressed", 16, BLUE_LO, (90, 140, 200, 255))
    frame("btn_disabled", 16, GRAY, (120, 128, 140, 200))
    frame("input", 12, (22, 24, 30, 255), SURFACE_EDGE)

    # simple / tintable
    solid_round("map_node", WHITE, 36)
    Image.new("RGBA", (8, 8), WHITE).save(os.path.join(OUT, "map_link.png"))
    Image.new("RGBA", (16, 16), (255, 255, 255, 255)).save(os.path.join(OUT, "overlay_dim.png"))

    for n, c in GEMS.items():
        gem(n, c)

    print("UI sprites written to", OUT)
    print("count:", len([f for f in os.listdir(OUT) if f.endswith('.png')]), "png")


if __name__ == "__main__":
    main()
