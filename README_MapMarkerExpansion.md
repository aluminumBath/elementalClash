# Elementborn Map Marker Expansion Patch

This patch extends the map marker system beyond boats and last-ridden creatures.

## Included marker categories

- Boat
- Last-ridden creature
- Active companion
- Camp
- Storage chest
- Rare enemy sighting
- Quest item
- Vendor NPC
- Guide NPC
- Custom pin

## Included icon PNGs

Located in:

```text
Elementborn/Assets/Elementborn/Art/UI/MapIcons/
```

Files:

```text
map_icon_boat.png
map_icon_land_paw.png
map_icon_flying_wing.png
map_icon_swimming_fin.png
map_icon_amphibious_paw_wave.png
map_icon_burrowing_claw.png
map_icon_companion_star.png
map_icon_camp_tent.png
map_icon_storage_chest.png
map_icon_rare_enemy_skull.png
map_icon_quest_scroll.png
map_icon_vendor_bag.png
map_icon_guide_compass.png
map_icon_custom_pin.png
map_icon_unknown_question.png
map_icons_preview.png
```

## Unity import settings for icons

Select the PNGs and set:

- Texture Type = `Sprite (2D and UI)`
- Sprite Mode = `Single`
- Alpha Is Transparency = checked
- Compression = `None` or `Low Quality`

Then click **Apply**.

## New scripts

```text
Assets/Elementborn/Core/CreatureTraversalType.cs
Assets/Elementborn/Core/CreatureTraversalCatalog.cs
Assets/Elementborn/Core/MapMarkerType.cs

Assets/Elementborn/Game/World/TrackedMapMarkerRecord.cs
Assets/Elementborn/Game/World/PlayerMapMarkerTracker.cs

Assets/Elementborn/Game/UI/MapMarkerIconLibrary.cs
Assets/Elementborn/Game/UI/PlayerMapMarkerRefreshExample.cs
```

## Basic usage examples

### Boat

When the player boards / disembarks or the boat moves:

```csharp
PlayerMapMarkerTracker.ReportBoat(transform.position, currentlyRidden: false);
```

### Last-ridden creature

When the player dismounts a creature:

```csharp
PlayerMapMarkerTracker.ReportLastRiddenCreature(creature.transform.position, creature.name);
```

### Active companion

```csharp
PlayerMapMarkerTracker.ReportActiveCompanion(companion.transform.position, companion.name);
```

### Camp

```csharp
PlayerMapMarkerTracker.ReportCamp(campTransform.position, "Field Camp");
```

### Storage chest

```csharp
PlayerMapMarkerTracker.ReportStorageChest(chestTransform.position, "Dock Chest");
```

### Rare enemy sighting

```csharp
PlayerMapMarkerTracker.ReportRareEnemySighting(enemyTransform.position, "Storm Wyvern Sighting", 600f);
```

### Quest item

```csharp
PlayerMapMarkerTracker.ReportQuestItem(itemTransform.position, "Pearl Compass");
```

### Vendor

```csharp
PlayerMapMarkerTracker.ReportVendorNpc(vendorTransform.position, "Tide Merchant");
```

### Guide

```csharp
PlayerMapMarkerTracker.ReportGuideNpc(guideTransform.position, "Kram");
```

### Custom pin

```csharp
PlayerMapMarkerTracker.ReportCustomPin("pin_reef", someWorldPos, "Reef Entrance", "Come back with water gear");
```

## Wiring icons into your map UI

Add `MapMarkerIconLibrary` to your map UI controller or a shared UI object and assign:

### Generic icons
- Boat Icon = `map_icon_boat`
- Companion Icon = `map_icon_companion_star`
- Camp Icon = `map_icon_camp_tent`
- Storage Chest Icon = `map_icon_storage_chest`
- Rare Enemy Icon = `map_icon_rare_enemy_skull`
- Quest Item Icon = `map_icon_quest_scroll`
- Vendor Icon = `map_icon_vendor_bag`
- Guide Icon = `map_icon_guide_compass`
- Custom Pin Icon = `map_icon_custom_pin`
- Unknown Icon = `map_icon_unknown_question`

### Creature traversal icons
- Land Creature Icon = `map_icon_land_paw`
- Flying Creature Icon = `map_icon_flying_wing`
- Swimming Creature Icon = `map_icon_swimming_fin`
- Amphibious Creature Icon = `map_icon_amphibious_paw_wave`
- Burrowing Creature Icon = `map_icon_burrowing_claw`

Then when you refresh the map, resolve a sprite for each `TrackedMapMarkerRecord` via `MapMarkerIconLibrary.Resolve(marker)`.

## Notes

- This patch is intentionally additive and generic.
- `PlayerMapMarkerRefreshExample.cs` is only a guide for integrating with your real map system.
- The tracker currently persists for the runtime session via `DontDestroyOnLoad`.
- The next step after this patch is per-save-slot persistence for the marker list.
