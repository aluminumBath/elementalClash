# v76 Functionality: Playable Camera + Movement Rescue

The project now compiles without errors, but pressing Play can still show only the horizon if the generated scene has no usable player/camera/controller stack.

## What v76 adds

```text
Assets/Elementborn/Game/Bootstrap/ElementbornPlayableRescueController.cs
Assets/Elementborn/Editor/ElementbornPlayableRescueMenu.cs
```

## Runtime behavior

On Play Mode scene load, the rescue controller ensures:

```text
visible directional light
visible ground plane if no large ground renderer exists
capsule player if no player exists
CharacterController movement
Main Camera following the player
small controls overlay
```

Controls:

```text
WASD / Arrow Keys = move
Shift = sprint
Space = jump
```

## Editor menus

```text
Elementborn -> Playable Setup -> Open Generated Playable Scene
Elementborn -> Playable Setup -> Install Playable Camera And Movement Rescue
```

## Expected result

After applying v76, pressing Play should show at least a green ground plane, a blue capsule player, a camera view, and a controls overlay.

This is intentionally a prototype rescue layer. It is not the final player controller, but it lets the project become manually testable while the real gameplay prefabs are wired up.
