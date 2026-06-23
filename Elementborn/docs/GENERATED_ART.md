# Elementborn — Generated Art (starter UI & particle sprites)

A ready-made set of flat 2D sprites generated to the `UI_SPRITES.md` spec, so the code-built UI and the
weather particles have art without opening an image editor. They're intentionally clean and simple (the
game's style); restyle or replace any of them freely.

**Locations (in the project):**
- UI sprites → `Assets/Elementborn/Art/UI/`
- Particle sprites → `Assets/Elementborn/Art/Particles/`
- Input glyphs → `Assets/Elementborn/Art/UI/glyphs/` (gamepad/keyboard/mouse; see `INPUT.md`)

These are **not** a substitute for the 3D Blender work (meshes for creatures, buildings, weapons, etc.) —
those still need modeling. These are the 2D layer, which was never a Blender task.

## Import settings (one time, per sprite)
Unity imports PNGs as textures by default. For the UI sprites:
- Texture Type = **Sprite (2D and UI)**; Mesh Type = Full Rect; Wrap = Clamp; Filter = Bilinear; sRGB on.
- For the **9-slice frames**, open the Sprite Editor and set **Border L/R/T/B** to the value in the table
  below, then on the `Image` set **Image Type = Sliced** and **Fill Center = on**.
- White sprites (`map_node`, `map_link`, the particles) are meant to be **tinted** — set the `Image` color
  (or the particle Start Color, which the weather code already sets).

For the particle sprites, use them as the texture on a particle material (URP/Particles/Unlit); the
`WeatherController` tints them per weather.

## UI sprites → slots and borders

| File | Type | 9-slice border | Fills (UI_SPRITES.md slot) |
| --- | --- | --- | --- |
| `panel` | Sliced | 24 | Shop panel; world-map detail panel |
| `map_frame` | Sliced | 24 | World-map area frame |
| `map_bg` | Sliced | 18 | World-map background |
| `hud_bar` | Sliced | 16 | HUD currency bar |
| `hud_prompt` | Sliced | 16 | HUD interaction prompt (warm trim) |
| `hud_toast` | Sliced | 16 | HUD toast (green trim) |
| `hud_chip` | Sliced | 12 | Score chip (optional) |
| `btn_row` | Sliced | 14 | Shop item rows |
| `btn_normal` | Sliced | 16 | Buttons — normal state |
| `btn_highlighted` | Sliced | 16 | Buttons — hover/highlighted |
| `btn_pressed` | Sliced | 16 | Buttons — pressed |
| `btn_disabled` | Sliced | 16 | Buttons — disabled |
| `map_node` | Simple | — | World-map region node (tint by biome) |
| `map_link` | Sliced/Simple | — | World-map link line (tint + stretch) |
| `overlay_dim` | Simple | — | Full-screen dim (death overlay; tintable) |
| `gem_diamond` | Simple | — | Currency icon — Diamond |
| `gem_sapphire` | Simple | — | Currency icon — Sapphire |
| `gem_emerald` | Simple | — | Currency icon — Emerald |
| `gem_ruby` | Simple | — | Currency icon — Ruby |
| `gem_silver` | Simple | — | Currency icon — Silver |

## Particle sprites

| File | Use |
| --- | --- |
| `particle_soft` | generic soft round particle |
| `particle_snow` | blizzard snow |
| `particle_rain` | rain (vertical streak) |
| `particle_sand` | sandstorm fleck |

## Wiring them in
The UI is code-built, so assign these the same way `UI_SPRITES.md` describes: add serialized `Sprite`
fields to the UI controllers (or `Resources.Load`) and set `image.sprite` + `image.type = Sliced`. (I can
wire those fields for you on request.) For particles, drop the sprite onto the weather particle material.

Colors match `palette/` and the toon shaders; gem colors are the five currency hues.

## Regenerating

The UI sprites are reproducible: `python3 make_ui_sprites.py` rewrites this set to the `UI_SPRITES.md` spec
(sizes, 9-slice borders, palette), the same way `make_sfx.py` and `make_glyphs.py` regenerate the audio and the
input glyphs. Restyle by editing the script and re-running — the layout is fixed, so nothing else moves. (These
stay 2D placeholders; the in-editor import + the 3D/VFX work below are separate.)

## Input glyphs (gamepad / keyboard / mouse)
`make_glyphs.py` generates 44 flat 64×64 glyphs into `Assets/Elementborn/Art/UI/glyphs/`: Xbox face buttons
(A/B/X/Y), bumpers/triggers (LB/RB/LT/RT), d-pad, Start/Select, common keycaps (E/F/M/C/Esc/F8/F10/Space), the
mouse buttons, **PlayStation** faces (✕/○/□/△) plus L1/R1/L2/R2 and Options/Share, and **Switch** faces plus L/R, ZL/ZR and +/− (faces per physical position). The
active gamepad's brand is detected at runtime, so the right faces show automatically. The HUD prompts and the controls menu use **text tokens** by default and don't
need these. To show them as image glyphs, import as **Sprite (2D and UI)** and copy the folder into
`Resources/ElementbornUI/glyphs/`; `ControlGlyphs.Sprite(action)` then loads the one matching the active
binding. `ControlGlyphs.SpriteName(path)` is the filename map if you swap in a different glyph pack.
