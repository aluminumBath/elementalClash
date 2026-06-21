# Elementborn — UI Sprite Specs

Exact sizes, anchors, 9-slice borders, and colors for the game's UI, taken straight from the code-built
canvases (`GameHud`, `ScoreController`, `RespawnController`, `CharacterCreationUI`, `ShopController`,
`WorldMapView`). Make sprites to these specs and they drop into the existing layout without moving anything.

## Read first — how this UI works

All UI is **built in code at runtime** (no prefabs or scenes). Each controller creates its own `Canvas`
(`ScreenSpaceOverlay`) with a `CanvasScaler` set to **Scale With Screen Size**. Today it draws solid-color
`Image`s and `UnityEngine.UI.Text` using the built-in `LegacyRuntime.ttf`.

To use your art you assign a `Sprite` to each `Image` (Image Type = **Sliced** for 9-slice frames) and swap
`Text` → **TextMeshPro**. Because the UI is code-built, you wire sprites either by adding
`[SerializeField] private Sprite …` fields to each controller and assigning `image.sprite` in its builder,
or by `Resources.Load<Sprite>("UI/…")`. The specs below are everything the art needs; the wiring is a small
code change (I can add the serialized fields and the TMP swap on request).

## Global conventions

- **Reference resolution:** every canvas uses **1280×800**, *except* `CharacterCreationUI`, which uses
  **1920×1080** (its VR canvas is World-Space, 800×600 at scale 0.0025). Sizes below are in the canvas's
  reference pixels.
- **Author at 2×** the listed size for crispness on high-DPI and Quest, export **transparent PNG** (straight
  alpha), provide `name.png` and `name@2x.png`.
- **9-slice:** author resizable frames with uniform corners; in the Sprite Editor set **Border** L/R/T/B to
  the listed inset (px at 1×); on the `Image` set **Type = Sliced**, **Fill Center = on**.
- **Palette:** dark UI surfaces are around `#1E2128`; the accent blue is the button color. Pull gem and
  accent colors from `palette/` so the UI matches the world.
- Leave a 2–4 px transparent margin around any glow so slicing doesn't clip it.

---

## 1. HUD — `GameHud` (1280×800)

Currently text-only. Add a bar behind the currency, gem icons, and frames behind the prompt/toast.

| Element | Anchor / Pivot | Size (1×) | Anchored pos | Sprite | 9-slice border | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Currency bar (new) | (0,1) | 640×44 | (24, −20) | `hud_bar` | 12 | sits behind the five gem+count groups |
| Gem icon ×5 (new) | inline | 28×28 | — | `gem_*` (5) | — | one per currency, colors below |
| Prompt frame (new) | (0.5,0) | 720×48 | (0, 90) | `hud_prompt` | 16 | warm text drawn on top (current `#FFF2B3`) |
| Toast frame (new) | (0.5,0.5) | 820×56 | (0, 150) | `hud_toast` | 16 | transient; current text `#B8FFCC` |

Currency text format is `Dia n  Sap n  Eme n  Rub n  Sil n` (font 24). Gem icons at **28×28** (author 56×56):
Diamond `#BFE9F5`, Sapphire `#3C96C8`, Emerald `#46A050`, Ruby `#C83C3C`, Silver `#A0A8B2`.

## 2. Score HUD — `ScoreController` (1280×800)

| Element | Anchor / Pivot | Size (1×) | Anchored pos | Sprite | Border | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Score text | (1,1) | 360×40 | (−24, −20) | optional `hud_chip` behind | 10 | font 28, white |
| Combo text | (1,1) | — | (−24, −58) | — | — | font 22, gold `#FFD966` |

## 3. Death overlay — `RespawnController` (1280×800)

| Element | Anchor / Pivot | Size (1×) | Pos | Sprite | Notes |
| --- | --- | --- | --- | --- | --- |
| Dim | fullscreen stretch | — | — | tint or `overlay_dim` | current `rgba(0.40,0.05,0.05,0.55)` |
| Countdown text | (0.5,0.5) | 700×200 | center | — | font 40, white |

## 4. Character creation — `CharacterCreationUI` (1920×1080)

| Element | Anchor / Pivot | Size (1×) | Anchored pos | Sprite | Border | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Panel background | full stretch | — | — | `menu_bg` (tile/gradient) | — | current `rgba(0.08,0.09,0.12,0.92)` |
| Title | top | 800×120 | (0, 230) | — | — | font 54 |
| Body text (reveal) | center | 720×220 | (0, 60) | — | — | font 40 |
| Buttons | center column | 360×64 | various y | `btn` + states | 16 | current blue `rgba(0.25,0.6,0.95,1)` |

Provide button states: **normal / highlighted / pressed / disabled** (either one sprite + Unity Button color
tint, or a 4-sprite SpriteSwap set). Button label font 28.

## 5. Shop — `ShopController` (1280×800)

| Element | Anchor / Pivot | Size (1×) | Anchored pos | Sprite | Border | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Panel | (0.5,0.5) | 620×640 | (0,0) | `panel` | 24 | current `rgba(0.08,0.09,0.12,0.95)` |
| Title | (0.5,1) of panel | 580×56 | (0, −24) | — | — | font 40 (shows the shop name) |
| Item row | vertical list | 540×56 (row) | auto (spacing 8) | `btn_row` | 14 horiz | current blue `rgba(0.22,0.5,0.85,1)`; label font 24 |
| Close button | (0.5,0) of panel | 240×56 | (0, 28) | `btn` | 16 | red `rgba(0.6,0.25,0.28,1)`; font 26 |

Rows are stacked by a `VerticalLayoutGroup` (spacing 8, content-fitted), so `btn_row` should 9-slice
horizontally and tolerate a fixed 56 px height.

## 6. World map — `WorldMapView` (1280×800)

| Element | Anchor / Pivot | Size (1×) | Anchored pos | Sprite | Border | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Background | full stretch | — | — | `map_bg` | — | current `rgba(0.07,0.08,0.10,1)` |
| Title "WORLD MAP" | (0,1) | 420×44 | (28, −20) | — | — | font 30 |
| Map area | (0,0.5) | 760×760 | (28, −10) | `map_frame` | 24 | current `rgba(0.10,0.12,0.15,1)` |
| Detail panel | (1,0.5) | 420×760 | (−28, −10) | `panel` (reuse) | 24 | current `rgba(0.12,0.14,0.18,0.95)`; text inset 18, font 18 |
| Region node | center in map | 18×18 (Capital 26×26) | per region | `map_node` (round) | — | tinted by biome (table below) |
| Link line | center in map | length×3 | midpoint | 1px white or `map_link` | — | thin connector |
| Region label | (0.5,0) | 160×18 | above node | — | — | font 13 |
| Enter button | (0.5,0) | 300×54 | (0, 22) | `btn` (reuse) | 16 | font 18 |

Map node tints (from code — distinct from the terrain palette): Capital `#F2D94D`, Cloud Temple `#CCE6FF`,
Volcano `#D94026`, Desert `#E6C773`, Island `#4DBFB3`, Beach `#F2E6A6`, Swamp `#59732A`, Marsh `#738C59`,
Forest Temple `#4099 59`→`#409959`, Mountains `#8C8C99`, Plains `#8CB366`.

---

## Fonts

Replace `LegacyRuntime.ttf` (`UnityEngine.UI.Text`) with **TextMeshPro** + an SDF font asset for crisp
scaling at any size. The point sizes above are the current `Text` sizes; TMP sizes will be close. A clean
humanist sans or a soft rounded face suits the Wind-Waker tone; include a bold weight for titles.

## Unity sprite import settings

- Texture Type = **Sprite (2D and UI)**; Mesh Type = Full Rect; Wrap = Clamp; Filter = Bilinear;
  sRGB = on; Alpha source = Input Texture Alpha. Tiny icons: Compression = None/High Quality.
- 9-slice: Sprite Editor → set Border L/R/T/B to the listed inset → on the `Image`, Type = Sliced, Fill
  Center on. Pixels-Per-Unit 100 is fine; keep it consistent across the set.

## Suggested sprite list (one tidy set)

`hud_bar`, `hud_prompt`, `hud_toast`, `hud_chip`, `gem_diamond`, `gem_sapphire`, `gem_emerald`, `gem_ruby`,
`gem_silver`, `panel`, `btn` (+ 4 states), `btn_row`, `map_bg`, `map_frame`, `map_node`, `map_link`,
`overlay_dim`. Plus a TMP font asset (regular + bold). That covers every screen above.

## Wiring sprites into the code-built UI

Two routes:

1. **Serialized fields (recommended).** Add e.g. `[SerializeField] private Sprite panelSprite;` to the
   controller and, in its builder, `img.sprite = panelSprite; img.type = Image.Type.Sliced;`. Swap `Text`
   creation to `TMP_Text` with a serialized `TMP_FontAsset`.
2. **Resources.** Put sprites in `Assets/Resources/UI/` and `Resources.Load<Sprite>("UI/panel")` in the
   builder.

Either way the layout/sizes don't change — that's why the specs above are fixed. Ask and I'll add the
serialized `Sprite`/`TMP_FontAsset` fields and the slicing/TMP swaps across the six controllers.
