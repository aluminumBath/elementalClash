# v88 Functionality: Visual Identity, Markers, Compass

v88 starts turning the procedural prototype from a plain systems test into a more readable game slice.

## New world readability

Added floating markers:

```text
! = main talk/objective
? = envoy/talk
◆ = shard objective
⬢ = pedestal objective
⇧ = elemental gate
⚔ = hostile
◎ = training dummy
+ = essence/resource node
✚ = healing shrine
$ = loot chest
i = lore stone
```

Markers bob and face the camera.

## Objective compass

A new objective compass appears at the top of the screen:

```text
C = toggle compass
```

It points to the current objective target and shows distance/direction such as:

```text
ahead
left
ahead-right
behind-left
```

## Visual identity improvements

The builder now adds more stylized silhouettes:
- player hood/shoulder cloak
- envoy shoulder cloaks
- hostile cloak
- spinning/bobbing shard and essence nodes
- chest lid
- more marker-driven readability

## First relic reward

The Convergence Reward Chest now grants:

```text
Prototype Convergence Relic
```

The HUD and journal show whether it is equipped.

## Suggested next milestone

v89 should add a real equipment/reward loop:
- relic affects ability behavior
- simple inventory screen
- loot comparison panel
- one optional side quest
- hostile drops loot
- resource nodes can craft/upgrade a relic
