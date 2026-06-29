# v85 Functionality: Elemental Gates + Status Effects

v85 extends the prototype from a combat sandbox into a clearer game-zone progression.

## New quest progression

After returning the shard to the pedestal, the next objective is:

```text
Attune and open the matching elemental gate.
```

The gate must match your selected element:

```text
1 = Fire -> Fire Gate
2 = Water -> Water Gate
3 = Earth -> Earth Gate
4 = Air -> Air Gate
```

Interact with the matching gate using `E`.

After opening a gate, the next objective is:

```text
Defeat the hostile.
```

Defeating the hostile completes the zone test.

## Elemental status effects

Elemental bolts now behave differently:

```text
Fire = burn damage over time
Water = slow
Earth = stun + knockback
Air = stronger pushback
```

The dummy and hostile health labels show short status labels like:

```text
Burning
Slowed
Stunned
Pushed
```

## Interactive gates

The four elemental gates are now actual interactables, not just scenery. When opened, their pillars spread and the top beam rises.

## Reset behavior

The editor reset menu now also closes elemental gates:

```text
Elementborn -> Prototype -> Clear Prototype Save And Reset Open Scene
```

## Test flow

1. Build prototype scene.
2. Press Play.
3. Choose New Prototype.
4. Talk to Ember Guide.
5. Choose Unifier or Dominion.
6. Collect shard.
7. Return shard.
8. Switch to an element with 1-4.
9. Press E at matching gate.
10. Defeat hostile.
11. Confirm objective becomes zone complete.
