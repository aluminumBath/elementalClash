# v77 Functionality: Rescue Capsule Grounding + Temporary Main Menu

v76 made the generated scene manually playable, but the capsule could appear halfway sunk into the ground.

## Root cause

The Unity capsule mesh is centered on the GameObject, but v76 set:

```text
CharacterController.center = (0, 1, 0)
```

That made the collision capsule bottom sit one unit above the visible mesh bottom. Gravity then moved the transform down until the controller touched the ground, leaving the visible capsule half underground.

## Fix

v77 sets:

```text
CharacterController.center = Vector3.zero
```

and clamps the rescue player's transform to:

```text
groundY + 1.0
```

so the capsule bottom stays on the plane.

## Temporary menu

v77 also adds a simple in-game prototype menu in the rescue layer:

```text
Start / Resume Prototype
Recenter Player
Repair Scene Again
Esc toggles menu
```

This is not the final main menu. It is a playable-test menu so the game has an obvious start/resume path while the final title screen/menu flow is still being built.
