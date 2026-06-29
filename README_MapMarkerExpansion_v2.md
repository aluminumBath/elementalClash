# Map Marker Expansion v2

This update adds more polished icons and broader code functionality for map markers.

## New map marker types

Core:
- Player
- Boat
- Last ridden creature
- Active companion

Player-owned:
- Camp
- Home base
- Storage chest
- Crafting station
- Stable

Items:
- Quest item
- Rare item
- Weapon
- Resource node
- Treasure

NPCs:
- Vendor
- Guide
- Trainer
- Healer
- Quest giver

Threats:
- Rare enemy sighting
- Boss lair
- Enemy camp
- Danger zone
- Sea monster sighting

Places:
- Dock
- Fast travel
- Shrine
- Dungeon
- Cave
- Puzzle
- Locked door
- Fishing spot
- Underwater ruin
- Coral reef
- Wind current

Player-authored:
- Custom pin

## New utility components

### MapDiscoveryTrigger
Attach to trigger volumes. When the player enters, it adds a map marker.

Good for:
- docks
- caves
- shrines
- fast travel points
- coral reef entrances
- underwater ruins
- wind currents

### ImportantItemMapMarker
Attach to quest items, rare items, weapons, treasure, or resource nodes.
It reports itself while active and can remove itself when disabled/picked up.

### NpcMapMarkerReporter
Attach to important NPCs like Kram, vendors, trainers, healers, and quest givers.

### CustomPinController
UI can call this for player-created pins.

### MapMarkerSeedTester
Optional scene helper to seed all marker types quickly so you can test the icon library.

## Icon location

`Assets/Elementborn/Art/UI/MapIcons`

Set all icons to `Sprite (2D and UI)` in Unity.
