# Elementborn Map Boat + Last-Ridden Creature Markers With Icons

This patch adds code plus temporary transparent PNG icons for showing the player's boat and last-ridden creature on the map.

## Added icon files

```text
Elementborn/Assets/Elementborn/Art/UI/MapIcons/map_icon_boat.png
Elementborn/Assets/Elementborn/Art/UI/MapIcons/map_icon_land_paw.png
Elementborn/Assets/Elementborn/Art/UI/MapIcons/map_icon_flying_wing.png
Elementborn/Assets/Elementborn/Art/UI/MapIcons/map_icon_swimming_fin.png
Elementborn/Assets/Elementborn/Art/UI/MapIcons/map_icon_amphibious_paw_wave.png
Elementborn/Assets/Elementborn/Art/UI/MapIcons/map_icon_burrowing_claw.png
Elementborn/Assets/Elementborn/Art/UI/MapIcons/map_icon_unknown_question.png
```

## Unity import settings for icons

After copying the patch and opening Unity:

1. Select the PNG icons in:
   `Assets/Elementborn/Art/UI/MapIcons`
2. In the Inspector, set:
   - Texture Type: `Sprite (2D and UI)`
   - Sprite Mode: `Single`
   - Alpha Is Transparency: checked
   - Filter Mode: `Point` or `Bilinear`
   - Compression: `None` or `Low Quality`
3. Click `Apply`.

## Marker prefab setup

For the last-ridden creature marker prefab:

1. Add an `Image` component if it does not already have one.
2. Add `CreatureMapMarkerIconSet`.
3. Assign:
   - Icon Image = marker Image
   - Land Icon = `map_icon_land_paw`
   - Flying Icon = `map_icon_flying_wing`
   - Swimming Icon = `map_icon_swimming_fin`
   - Amphibious Icon = `map_icon_amphibious_paw_wave`
   - Burrowing Icon = `map_icon_burrowing_claw`
   - Unknown Icon = `map_icon_unknown_question`

For the boat marker, use:

```text
map_icon_boat.png
```

## Hook into BoatController

When the player boards:

```csharp
PlayerMapAssetTracker.ReportBoatPosition(transform.position, true);
```

When the player disembarks:

```csharp
PlayerMapAssetTracker.ReportBoatPosition(transform.position, false);
```

While the boat is actively piloted:

```csharp
if (currentPilot != null)
{
    PlayerMapAssetTracker.ReportBoatPosition(transform.position, true);
}
```

Replace `currentPilot` with your actual boat pilot field/property.

## Hook into creature riding

When the player mounts a creature:

```csharp
PlayerMapAssetTracker.ReportCreaturePosition(
    creature.transform.position,
    true,
    creature.name,
    creatureElement
);
```

When the player dismounts:

```csharp
PlayerMapAssetTracker.ReportCreaturePosition(
    creature.transform.position,
    false,
    creature.name,
    creatureElement
);
```

## Hook into map UI

When creating/updating the last-ridden creature marker:

```csharp
var iconSet = marker.GetComponent<CreatureMapMarkerIconSet>();
if (iconSet != null)
{
    iconSet.SetCreatureType(tracker.LastRiddenCreatureTraversalType);
}
```

The included `PlayerMapMarkerRefreshExample.cs` shows the intended logic but logs markers instead of creating real UI markers.
