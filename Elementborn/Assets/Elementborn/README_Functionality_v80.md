# v80 Functionality: Reliable Prototype Guide Interaction

The v79 prototype scene was visible and playable, and the shard could be collected, but pressing `E` near Ember Guide could fail.

## Root cause

The player interaction scan was too dependent on physics overlap/collider range. That made interaction fragile when the player was visually close to the guide but not close enough to the exact collider/scan sphere.

## Changes

### More forgiving player scan

`ElementbornPrototypePlayerController` now:
- uses a larger default interaction range: `5.5`
- searches all active `ElementbornPrototypeInteractable` objects by distance
- still keeps physics overlap as a fallback
- shows the nearest available `Press E` prompt
- gives a clearer message if no interactable is close enough

### Interaction radius on every interactable

`ElementbornPrototypeInteractable` now has:

```text
activationRadius
createTriggerRadius
EnsureInteractionRadius()
```

and creates a trigger sphere child named:

```text
Prototype Interaction Radius
```

### Existing scene repair menu

New menus:

```text
Elementborn -> Prototype -> Repair Prototype Interactions In Open Scene
Elementborn -> Prototype -> Report Prototype Interactions
```

Recommended radii:
- Ember Guide: `5`
- Glowing Shard: `3.25`
- Return Pedestal: `4`

## How to test

1. Press Play.
2. Click Start / Resume.
3. Walk near Ember Guide.
4. Confirm HUD says `Press E: Talk to Ember Guide`.
5. Press `E`.
6. Quest should advance to `Collect the glowing shard`.
