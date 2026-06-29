# v46 Functionality: Production-Like Playable Scene + Fire Capital Deepening

This pass focuses on making the generated test scene feel more like a playable vertical slice.

## Major additions

```text
Cleaner production-like playable scene polish
Fire Capital deepening: NPCs, quests, hooks, interactables, volcano pressure
Simple main gameplay loop/spawn flow
Gameplay loop dashboard buttons
PlayMode smoke tests
```

## New runtime systems

```text
ElementbornGameplayLoopState
ElementbornSpawnRole
ElementbornSpawnPoint
ElementbornSpawnRegistry
ElementbornSpawnWaveEntry
ElementbornSpawnWaveDefinition
ElementbornMainGameplayLoopDirector

FireCapitalDistrict
FireCapitalHookType
FireCapitalCourtHookDefinition
FireCapitalRuntimeRecord
FireCapitalRegistry
FireCapitalCourtInteractable
FireCapitalVolcanoHazardController
FireCapitalAdminCommandBridge
```

## New editor menus

```text
Elementborn → Playable Setup → Build Rounded Playable Scene
Elementborn → Playable Setup → Generate Content For Playable Scene
Elementborn → Playable Setup → Install Generated Game Systems In Open Scene
Elementborn → Playable Setup → Apply Production Scene Polish

Elementborn → Gameplay Loop → Generate Gameplay Loop Assets
Elementborn → Gameplay Loop → Install Gameplay Loop In Open Scene
Elementborn → Gameplay Loop → Install Spawn Points In Open Scene

Elementborn → Fire Capital → Generate Fire Capital Assets
Elementborn → Fire Capital → Install Fire Capital Systems In Open Scene
```

## New Fire Capital quests

```text
Audience at the Caldera Throne
The Caldera Evacuation Drill
The Furnace Guard Trial
Lyra's Glassfire Mediation
Maelis and the Ash Memory
```

## New Fire Capital NPCs

```text
Captain Vaela Furnaceguard
Master Ventwright Boros
Talia Embermarket
Sister Calda Glassfire
Nix Ashrunner
```

## Dashboard actions

The story debug dashboard now has buttons for:

```text
Start Loop
Spawn Wave
Fire Hook
Fire Volcano
Refresh
Save Slot 0
Load Slot 0
Evaluate Political
Sync Capitals
Trigger Social Event
Admit Demo Creature
Respawn Wolf Pack
```

## PlayMode smoke tests

Added PlayMode tests for:

```text
gameplay loop start/spawn flow
spawn waves
Fire Capital hook start/resolve
volcano pressure pulse/calm
dashboard action buttons
```

## Recommended use

```text
1. Import v46.
2. Let Unity compile.
3. Run Elementborn → Playable Setup → Build Rounded Playable Scene.
4. Open the generated scene.
5. Press Play.
6. Use dashboard buttons to test main loop, political systems, Fire Capital, orphanage, social events, and wolf-pack respawn.
7. Run Unity Test Runner → PlayMode.
```
