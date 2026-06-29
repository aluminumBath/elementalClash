# v47 Functionality: Left Wrist Admin / Cheat Code UI

This patch converts the admin/cheat workflow from mostly command-line style tools into an in-game left-wrist UI with dropdowns and form fields.

## Added runtime systems

```text
AdminActionCategory
AdminFieldType
AdminActionFieldDefinition
AdminActionDefinition
AdminActionRequest
AdminActionResult
AdminRuntimeCommandRouter
AdminActionCatalog
AdminActionExecutor
AdminWristFieldRow
AdminWristPanelView
LeftWristAdminPanelController
```

## New editor menu

```text
Elementborn → Admin UI → Generate Left Wrist Admin UI Prefab
Elementborn → Admin UI → Install Left Wrist Admin UI In Open Scene
```

## What the wrist panel provides

```text
Category dropdown
Action dropdown
Dynamic form fields
Apply Change button
Cheat Code dropdown
Apply Cheat button
Raw admin command input
Run Raw button
```

## Supported action categories

```text
GameplayLoop
SaveLoad
CapitalWorldState
PoliticalEvents
QuestChains
FireCapital
SocialGroups
CreatureOrphanage
WolfPack
StoryEncounters
CheatCodes
RawCommand
```

## Cheat codes included

```text
stabilize_fire_capital
chaos_fire_capital
start_fire_intro
spawn_wave
save_slot_zero
load_slot_zero
admit_demo_creature
resolve_wolf_pack
pulse_volcano
calm_volcano
```

## Controls

```text
F8 toggles the wrist panel in non-VR/editor playtests.
Assign LeftWristAdminPanelController.leftWristAnchor to the player's left wrist/hand transform for VR.
If no wrist anchor is assigned, the panel follows the main camera with a left-side offset.
```

## Integration

The rounded playable scene builder now installs the left wrist admin panel automatically:

```text
Elementborn → Playable Setup → Build Rounded Playable Scene
```

The runtime bootstrap also ensures:

```text
AdminRuntimeCommandRouter
AdminActionCatalog
AdminActionExecutor
```
