# Elementborn — Color Palette

A starter palette of 32 flat colors for the vertex-color art workflow (see `ART_GUIDE.md`). Every
value is the authoritative **sRGB** used by the code: the biome colors come from `TerrainColors`
(Core/World), and the sky/water/shadow hues from the toon shaders. Painting your art from these keeps
the whole world coherent.

Files (in `palette/`):
- **`elementborn_palette_blender.py`** — run in Blender's Scripting tab to create a native
  **Elementborn** palette; it appears in the Vertex Paint brush's Color Palette dropdown
  (it converts sRGB→linear for you).
- **`elementborn_palette.png`** — a labeled swatch; open it in an Image Editor and use the brush
  eyedropper, or keep it on a second screen. This PNG is the visual source of truth.
- **`elementborn_palette.gpl`** — a GIMP palette for the 2D tools you'll use for UI/particle art
  (Krita, Aseprite, GIMP, and Inkscape all import `.gpl`).

## Importing
- **Blender (native palette):** Scripting workspace → Text ▸ Open `elementborn_palette_blender.py`
  → Run Script. Then Vertex Paint mode → brush → Color Palette → choose **Elementborn**.
- **Blender (eyedrop):** open `elementborn_palette.png` in an Image Editor; click the brush color
  swatch → eyedropper → sample from the image.
- **Krita / Aseprite / GIMP / Inkscape:** import `elementborn_palette.gpl` as a palette.

## A note on color management
Blender brush/palette colors are scene-linear, so the script converts these sRGB values to linear;
painting onto a **Byte Color** attribute then stores the intended sRGB, which is what `ToonLit`
expects. If a swatch ever looks off, trust the PNG and eyedrop from it.

## The colors

### Terrain / Biomes
| Name | RGB | Hex | Used by |
| --- | --- | --- | --- |
| Plains | 110, 165, 75 | `#6EA54B` | grassland ground |
| Capital City | 132, 150, 112 | `#849670` | city ground |
| Mountains | 130, 120, 110 | `#82786E` | rock |
| Volcano | 72, 62, 62 | `#483E3E` | basalt |
| Desert | 210, 190, 130 | `#D2BE82` | sand / dunes |
| Forest Temple | 70, 130, 70 | `#468246` | forest floor |
| Swamp | 90, 110, 80 | `#5A6E50` | swamp ground |
| Marsh | 110, 120, 85 | `#6E7855` | marsh ground |
| Beach | 225, 210, 160 | `#E1D2A0` | beach sand |
| Island | 100, 160, 90 | `#64A05A` | island grass |
| Cloud Temple | 180, 190, 205 | `#B4BECD` | high plateau stone |
| Seabed | 190, 180, 150 | `#BEB496` | underwater ground |

### Sky & Water
| Name | RGB | Hex | Used by |
| --- | --- | --- | --- |
| Sky Top | 51, 115, 217 | `#3373D9` | ToonSky zenith |
| Sky Horizon | 184, 219, 242 | `#B8DBF2` | ToonSky horizon |
| Sky Ground | 115, 128, 122 | `#73807A` | ToonSky below-horizon |
| Sun | 255, 245, 209 | `#FFF5D1` | sun disc / warm light |
| Cloud / Foam | 255, 255, 255 | `#FFFFFF` | clouds & water foam |
| Water Shallow | 77, 179, 189 | `#4DB3BD` | ToonWater shallow |
| Water Deep | 15, 71, 115 | `#0F4773` | ToonWater deep |
| Shadow Tint | 128, 140, 179 | `#808CB3` | ToonLit shadow |

### Elements & Abilities
| Name | RGB | Hex | Used by |
| --- | --- | --- | --- |
| Fire | 220, 90, 45 | `#DC5A2D` | fire element + fireball |
| Water | 60, 150, 200 | `#3C96C8` | water element + jet |
| Earth | 150, 110, 70 | `#966E46` | earth element + shard |
| Air | 200, 225, 240 | `#C8E1F0` | air element + gust |
| Ice | 150, 220, 235 | `#96DCEB` | ice element + spike |
| Lightning | 240, 215, 90 | `#F0D75A` | lightning element + bolt |

### Materials & Neutrals
| Name | RGB | Hex | Used by |
| --- | --- | --- | --- |
| Wood | 140, 95, 55 | `#8C5F37` | wooden weapons |
| Metal | 160, 168, 178 | `#A0A8B2` | metal weapons |
| Stone | 150, 150, 150 | `#969696` | generic structures |
| Outline | 18, 18, 22 | `#121216` | ToonLit outline |
| Skin | 222, 178, 140 | `#DEB28C` | character skin (starter) |
| Cloth | 90, 100, 120 | `#5A6478` | character cloth (starter) |
