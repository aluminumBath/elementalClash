# v81 Functionality: Make the Prototype Look Like Elementborn

## Compressed project state

Current known-good milestone:
- Unity compiles with no red project errors.
- Smoke tests passed earlier.
- EventSystem duplication fixed.
- Missing `Interactable` tag spam fixed.
- Playable prototype loop completed:
  `menu -> talk to guide -> collect shard -> return to pedestal -> complete/save`.
- v80 made guide interaction reliable.
- Remaining warnings are mostly Unity/package/editor noise, not gameplay blockers.

## What v81 adds

v81 starts replacing the graybox feel with Elementborn-specific gameplay and visuals.

### Element choice

The prototype menu now lets you choose:

```text
Fire
Water
Earth
Air
```

You can also switch during play:

```text
1 = Fire
2 = Water
3 = Earth
4 = Air
```

### First elemental ability

```text
Q = cast elemental bolt
```

The bolt color/shape changes with your attunement.

### Training dummy

The arena now includes a dummy enemy:
- takes damage from elemental bolts
- reports HP in the HUD message feed
- disappears when defeated
- respawns after a short delay

### Styled player proxy

The player is now a simple stylized channeler proxy:
- body
- head
- elemental sash

This is still primitive geometry, but no longer just the plain rescue capsule.

### Elementborn test arena

The prototype area is now divided into four elemental quadrants:

```text
Fire Quarter
Water Quarter
Earth Quarter
Air Quarter
Central Convergence Platform
```

This starts matching the larger Elementborn structure: elemental regions around a central convergence.

### Better HUD/dialogue

The HUD now shows:
- current objective
- selected attunement
- ability key
- control hints

NPC dialogue now appears in a bottom dialogue box instead of only the transient debug message area.
