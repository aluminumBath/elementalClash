# v82 Functionality: Prototype Reset + Bolt Hit Feedback

## Problem fixed

After completing the prototype loop once, old `PlayerPrefs` save data could auto-load the scene in `Completed` state. That made talking to the guide or collecting the shard report that the objective was already complete.

Water bolts also appeared not to hit the training dummy, or the feedback was too subtle.

## Changes

### No accidental completed-state load

`ElementbornPrototypeGameManager` now defaults:

```text
loadSavedStateOnAwake = false
```

The main menu now has clear choices:

```text
New Prototype
Resume Saved
Save Current
Clear Save
Start Without Loading
```

Use **New Prototype** to replay the quest from the beginning.

### Scene reset

New editor menu:

```text
Elementborn -> Prototype -> Clear Prototype Save And Reset Open Scene
```

This clears PlayerPrefs, resets the manager to `NotStarted`, reactivates the shard, resets the player position, and restores dummy health.

### More reliable elemental bolt hits

`ElementbornPrototypeProjectile` now uses:
- trigger collider
- kinematic Rigidbody
- sphere cast between frames
- overlap sphere at current position
- impact visual on hit

This avoids fast projectile tunneling and makes water bolts much easier to verify.

### Dummy feedback

The training dummy now has:
- visible HP label
- hit flash
- HUD message on hit
- reset/respawn behavior
