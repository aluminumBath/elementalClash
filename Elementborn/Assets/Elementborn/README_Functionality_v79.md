# v79 Functionality: Prototype Gameplay Loop

v79 moves Elementborn from "rescue playable" toward an actual prototype loop.

## New scene builder

```text
Elementborn -> Prototype -> Build Prototype Gameplay Loop Scene
Elementborn -> Prototype -> Open Prototype Gameplay Loop Scene
Elementborn -> Prototype -> Install Prototype Loop In Open Scene
```

The main scene is saved to:

```text
Assets/Elementborn/Generated/Scenes/Elementborn_Prototype_Gameplay.unity
```

## Gameplay loop

The prototype scene includes:

```text
Prototype main menu
HUD objective tracker
Capsule player with CharacterController
Third-person camera follow
Ember Guide NPC
Glowing Shard objective
Return Pedestal turn-in point
Basic save/load with PlayerPrefs
Readable 3D labels
Simple landmarks
```

## Controls

```text
WASD / Arrow Keys = move
Shift = sprint
Space = jump
E = interact
Esc = menu
F5 = save
F9 = load
F1 = toggle help
```

## Quest flow

```text
1. Talk to Ember Guide.
2. Collect the Glowing Shard.
3. Return it to the Return Pedestal.
4. Prototype loop completes and saves.
```

## Rescue layer relationship

The v76/v77 rescue layer remains available for broken/generated scenes, but v79 prevents it from auto-installing when a real `ElementbornPrototypeGameManager` is present.
