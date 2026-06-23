#!/usr/bin/env python3
"""
Generate flat placeholder input glyphs (gamepad buttons, keyboard keycaps, mouse) for Elementborn.

Output: Assets/Elementborn/Art/UI/glyphs/*.png  (64x64 RGBA, transparent background)
A preview sheet is written to ../glyphs_preview.png.

These are intentionally simple placeholders in the game's flat style. ControlGlyphs.SpriteName() maps each
binding to one of these filenames; copy the folder into Resources/ElementbornUI/glyphs/ (and import as Sprite)
to use them as Image-based glyphs. The in-engine prompts work as text tokens without them.
"""
import os
from PIL import Image, ImageDraw, ImageFont

SS = 2          # supersample factor for crisp edges
S = 64          # final size
W = S * SS

OUT = os.path.join("Assets", "Elementborn", "Art", "UI", "glyphs")
os.makedirs(OUT, exist_ok=True)

FONT_PATH = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"

def font(px):
    try:
        return ImageFont.truetype(FONT_PATH, int(px * SS))
    except Exception:
        return ImageFont.load_default()

def new():
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)

def center_text(d, text, fnt, fill, cx=W // 2, cy=W // 2):
    l, t, r, b = d.textbbox((0, 0), text, font=fnt)
    d.text((cx - (r - l) / 2 - l, cy - (b - t) / 2 - t), text, font=fnt, fill=fill)

def finish(img, name):
    img = img.resize((S, S), Image.LANCZOS)
    img.save(os.path.join(OUT, name + ".png"))
    return img

def rrect(d, box, radius, fill=None, outline=None, width=1):
    d.rounded_rectangle(box, radius=radius, fill=fill, outline=outline, width=width * SS)

# ---- gamepad face buttons (filled circle + letter) -------------------------
FACE = {
    "gp_a": ("A", (87, 166, 57)),
    "gp_b": ("B", (194, 54, 46)),
    "gp_x": ("X", (45, 111, 203)),
    "gp_y": ("Y", (210, 160, 36)),
}
def face_button(name, letter, color):
    img, d = new()
    m = 6 * SS
    d.ellipse([m, m, W - m, W - m], fill=color, outline=(245, 247, 250), width=3 * SS)
    center_text(d, letter, font(30), (245, 247, 250))
    return finish(img, name)

# ---- shoulders / triggers / start / select (rounded rect + text) -----------
DARK = (58, 63, 75)
DARK_BORDER = (154, 163, 178)
def pill(name, text, fs=22):
    img, d = new()
    m = 5 * SS
    rrect(d, [m, m + 8 * SS, W - m, W - m - 8 * SS], radius=12 * SS,
          fill=DARK, outline=DARK_BORDER, width=2)
    center_text(d, text, font(fs), (240, 243, 248))
    return finish(img, name)

# ---- dpad directions (rounded square + white triangle) ---------------------
def dpad(name, direction):
    img, d = new()
    m = 8 * SS
    rrect(d, [m, m, W - m, W - m], radius=10 * SS, fill=DARK, outline=DARK_BORDER, width=2)
    cx = cy = W // 2
    a = 14 * SS
    if direction == "up":
        pts = [(cx, cy - a), (cx - a, cy + a // 2), (cx + a, cy + a // 2)]
    elif direction == "down":
        pts = [(cx, cy + a), (cx - a, cy - a // 2), (cx + a, cy - a // 2)]
    elif direction == "left":
        pts = [(cx - a, cy), (cx + a // 2, cy - a), (cx + a // 2, cy + a)]
    else:
        pts = [(cx + a, cy), (cx - a // 2, cy - a), (cx - a // 2, cy + a)]
    d.polygon(pts, fill=(240, 243, 248))
    return finish(img, name)

# ---- keyboard keycaps (light rounded square + dark letter) -----------------
KEYCAP = (232, 234, 240)
KEYCAP_BORDER = (174, 182, 196)
KEYCAP_TEXT = (42, 46, 55)
def keycap(name, text, fs=26):
    img, d = new()
    m = 6 * SS
    # base + a subtle bottom lip
    rrect(d, [m, m, W - m, W - m], radius=11 * SS, fill=(196, 201, 212), outline=KEYCAP_BORDER, width=2)
    rrect(d, [m + 2 * SS, m, W - m - 2 * SS, W - m - 5 * SS], radius=9 * SS, fill=KEYCAP)
    center_text(d, text, font(fs), KEYCAP_TEXT, cy=W // 2 - 2 * SS)
    return finish(img, name)

# ---- mouse (outline body, highlight a button) ------------------------------
MOUSE_OUTLINE = (201, 208, 220)
HILITE = (45, 111, 203)
def mouse(name, side):
    img, d = new()
    bx0, by0, bx1, by1 = 16 * SS, 8 * SS, W - 16 * SS, W - 8 * SS
    midx = (bx0 + bx1) // 2
    midy = by0 + (by1 - by0) // 3
    # highlight half
    if side == "left":
        d.pieslice([bx0, by0, bx1, by0 + (bx1 - bx0)], 180, 270, fill=HILITE)
        d.rectangle([bx0, midy - 1, midx, midy], fill=HILITE)
    else:
        d.pieslice([bx0, by0, bx1, by0 + (bx1 - bx0)], 270, 360, fill=HILITE)
    # body outline
    d.rounded_rectangle([bx0, by0, bx1, by1], radius=18 * SS, outline=MOUSE_OUTLINE, width=3 * SS)
    d.line([midx, by0 + 4 * SS, midx, midy], fill=MOUSE_OUTLINE, width=3 * SS)
    d.line([bx0, midy, bx1, midy], fill=MOUSE_OUTLINE, width=3 * SS)
    return finish(img, name)

# ---- PlayStation faces (dark button + white shape symbol) ------------------
PS_BODY = (40, 42, 52)
PS_RING = (235, 238, 242)
def ps_face(name, shape):
    img, d = new()
    m = 6 * SS
    d.ellipse([m, m, W - m, W - m], fill=PS_BODY, outline=PS_RING, width=3 * SS)
    c, r, col, lw = W // 2, 13 * SS, (245, 247, 250), 4 * SS
    if shape == "cross":
        d.line([c - r, c - r, c + r, c + r], fill=col, width=lw)
        d.line([c - r, c + r, c + r, c - r], fill=col, width=lw)
    elif shape == "circle":
        d.ellipse([c - r, c - r, c + r, c + r], outline=col, width=lw)
    elif shape == "square":
        d.rectangle([c - r, c - r, c + r, c + r], outline=col, width=lw)
    elif shape == "triangle":
        pts = [(c, c - r - 2 * SS), (c - r, c + r), (c + r, c + r)]
        d.line(pts + [pts[0]], fill=col, width=lw, joint="curve")
    return finish(img, name)

# ---- Switch faces (dark button + white letter, per physical position) ------
def sw_face(name, letter):
    img, d = new()
    m = 6 * SS
    d.ellipse([m, m, W - m, W - m], fill=PS_BODY, outline=PS_RING, width=3 * SS)
    center_text(d, letter, font(28), (245, 247, 250))
    return finish(img, name)

def main():
    made = []
    for n, (letter, color) in FACE.items():
        face_button(n, letter, color); made.append(n)
    for n, t in [("gp_lb", "LB"), ("gp_rb", "RB"), ("gp_lt", "LT"), ("gp_rt", "RT")]:
        pill(n, t, 22); made.append(n)
    for n, t in [("gp_start", "START"), ("gp_select", "SELECT")]:
        pill(n, t, 13); made.append(n)
    for n, dirn in [("gp_dup", "up"), ("gp_ddown", "down"), ("gp_dleft", "left"), ("gp_dright", "right")]:
        dpad(n, dirn); made.append(n)
    for n, t, fs in [("key_e", "E", 28), ("key_f", "F", 28), ("key_m", "M", 26), ("key_c", "C", 28),
                     ("key_esc", "Esc", 18), ("key_f8", "F8", 22), ("key_f10", "F10", 18)]:
        keycap(n, t, fs); made.append(n)
    keycap("key_space", "SPACE", 13); made.append("key_space")
    for n, side in [("mouse_left", "left"), ("mouse_right", "right")]:
        mouse(n, side); made.append(n)
    for n, shape in [("gp_ps_cross", "cross"), ("gp_ps_circle", "circle"),
                     ("gp_ps_square", "square"), ("gp_ps_triangle", "triangle")]:
        ps_face(n, shape); made.append(n)
    for n, letter in [("gp_sw_a", "A"), ("gp_sw_b", "B"), ("gp_sw_x", "X"), ("gp_sw_y", "Y")]:
        sw_face(n, letter); made.append(n)
    # PlayStation shoulders / triggers / start-select (pills)
    for n, t, fs in [("gp_ps_l1", "L1", 22), ("gp_ps_r1", "R1", 22), ("gp_ps_l2", "L2", 22),
                     ("gp_ps_r2", "R2", 22), ("gp_ps_options", "OPTIONS", 12), ("gp_ps_share", "SHARE", 13)]:
        pill(n, t, fs); made.append(n)
    # Switch shoulders / triggers / start-select (pills)
    for n, t, fs in [("gp_sw_l", "L", 24), ("gp_sw_r", "R", 24), ("gp_sw_zl", "ZL", 22),
                     ("gp_sw_zr", "ZR", 22), ("gp_sw_plus", "+", 30), ("gp_sw_minus", "−", 30)]:
        pill(n, t, fs); made.append(n)

    # preview sheet
    cols = 8
    rows = (len(made) + cols - 1) // cols
    pad, cell = 10, S + 26
    sheet = Image.new("RGBA", (cols * cell + pad, rows * cell + pad), (32, 35, 42, 255))
    sd = ImageDraw.Draw(sheet)
    pf = ImageFont.truetype(FONT_PATH, 11) if os.path.exists(FONT_PATH) else ImageFont.load_default()
    for i, n in enumerate(made):
        cx, cy = pad + (i % cols) * cell, pad + (i // cols) * cell
        g = Image.open(os.path.join(OUT, n + ".png"))
        sheet.alpha_composite(g, (cx + (cell - S) // 2 - pad // 2, cy))
        l, t, r, b = sd.textbbox((0, 0), n, font=pf)
        sd.text((cx + (cell - (r - l)) // 2 - pad // 2, cy + S + 4), n, font=pf, fill=(200, 205, 215, 255))
    sheet.save(os.path.join("..", "glyphs_preview.png"))
    print(f"generated {len(made)} glyphs -> {OUT}")
    print("preview -> glyphs_preview.png")

if __name__ == "__main__":
    main()
