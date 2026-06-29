# v84 Functionality: Game-Zone Prototype Upgrade

v84 turns the styled prototype into a more game-like test zone.

## New gameplay systems

### Player health / stamina

The player now has:

```text
Health
Stamina
Sprint stamina drain
Jump stamina cost
Stamina regeneration
Damage invulnerability window
Death and respawn
```

### Hostile enemy

The prototype scene now includes a hostile enemy that:
- detects the player
- moves toward the player
- attacks in melee range
- damages health
- can be hit by elemental bolts
- respawns after defeat

### Branching guide choice

Ember Guide now asks the player to choose a first stance:

```text
Unifier
Dominion
```

This is saved as prototype state and shown in the HUD. It is a first step toward Elementborn's faction/ideology theme.

### Better HUD

The HUD now shows:
- current objective
- selected element
- selected stance
- health
- stamina
- ability cooldown

### Elemental gates / hub feel

The prototype arena now adds four visible elemental gates:

```text
Fire Gate
Water Gate
Earth Gate
Air Gate
```

This makes the scene feel more like a central convergence hub with elemental paths.

## Test flow

1. Run `Elementborn -> Prototype -> Build Prototype Gameplay Loop Scene`.
2. Press Play.
3. Choose New Prototype.
4. Talk to Ember Guide.
5. Choose Unifier or Dominion.
6. Collect the shard.
7. Return it to the pedestal.
8. Cast Q at the dummy and hostile.
9. Let the hostile hit you.
10. Confirm death/respawn if health reaches zero.
