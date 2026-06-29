# v89 Functionality: Asset-Backed Visuals

v89 starts using the generated PNG assets inside the Unity prototype.

## What this is

This is the first bridge from procedural placeholder geometry to actual generated art assets.

The generated PNGs are copied into:

```text
Assets/Elementborn/Art/Concept/WindwakerReplacement/
```

## Included generated assets

```text
- volcanic_fortress_in_fiery_colors.png
- social_npcs_character_lineup_illustration.png
- fantasy_ui_design_reference_poster.png
- fantasy_map_icons_reference_sheet.png
- fantasy_spell_and_combat_icons_chart.png
- fantasy_quest_and_equipment_icons_ui.png
- stylized_game_map_marker_assets_sheet.png
- stylized_game_asset_design_sheet.png
- fantasy_boss_icon_reference_sheet.png
- fantasy_game_ui_style_guide.png
- fantasy_map_icons_on_parchment_sheet.png
- fantasy_spell_and_combat_icon_sheet.png
- fantasy_quest_and_equipment_icon_sheet.png
```

## New in-scene art boards

The prototype builder now creates in-world textured boards using those PNGs:

```text
Fire Capital Vista Board
Social NPC Roster Board
Spell Training Icon Board
Quest Gear Reward Board
Map Marker Reference Board
Game Asset Design Board
UI Style Board
Boss Icon Board
```

These appear around the central hub as readable visual anchors/reference boards.

## New code

```text
ElementbornPrototypeBillboardFacing.cs
```

This supports camera-facing art boards for future NPC portraits, signboards, quest boards, and 2D-to-3D placeholder conversions.

## Important note

These are still PNG assets, not final rigged 3D prefabs or FBX models. This patch uses them as textured in-world boards and visual guides. The next step is to start creating Unity prefab replacements for specific objects:

```text
player
hostile
envoys
gates
shrines
resource nodes
chests
lore stones
```

## Suggested next milestone

v90 should convert the visual boards into actual prefab stand-ins:
- Fire vista becomes Fire Gate/Fire Quarter backdrop
- NPC lineup informs envoy silhouettes
- icon sheets become UI icons/markers
- asset sheet informs shrine/chest/node meshes
- spell chart informs ability HUD icons
