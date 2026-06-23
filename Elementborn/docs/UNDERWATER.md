# Underwater layer

**Status:** the underwater *systems* and *swim locomotion* are in — submerged state, breathing/drowning, the
per-element rules, the freeze/bubble interactions, and comfort-vignetted 3D swimming. The *creatures* (fish,
coral, octopus, the dragon boss) are the remaining phase; see the boundaries at the end.

## Submerged state

`WaterVolume` marks a body of water: put it on an object with a trigger `Collider` — the collider box is the
water and its top is the surface. Any point inside is "submerged". It's registered statically, so an actor's
`UnderwaterController` just asks `WaterVolume.Submerged(position)` with no wiring.

## Breath & drowning

`OxygenModel` (pure, tested) is a breath meter: it drains while you can't breathe and refills while you can.
`UnderwaterController` runs one per actor and applies drowning damage once it empties. You're breathing at the
surface, inside an air bubble, or if you're a **water user** (water users breathe water). A non-water user
caught in an ice trap can't breathe at all — the ice suffocates them.

## Per-element rules below the surface

`UnderwaterAbilityRules` (pure, tested) gates casting underwater, enforced by a hook in
`PlayerCombatController` (a fizzled cast costs no stamina):

- **Water** — moves normally and is the strongest down here, but only its **ice** offense works. Ice barriers
  and dashes also pass.
- **Fire** — its offense fizzles underwater; its real job is to **thaw ice** — a fire cast melts nearby
  `IceTrap`s faster (`IceTrap.ThawNear`).
- **Air** — offense fizzles; its tool is the **bubble** (below).
- **Earth** — offense fizzles underwater for now (easy to revisit if you want underwater earth moves).

## Freeze-trap and air bubble

- **`IceTrap`** — the Water user's frozen patch. It **traps** whoever's inside (a Slow plus an immobilize via
  the status system; water users are exempt), suffocates non-water users, melts on a timer, and Fire thaws it
  faster. Deploy with `IceTrap.Freeze(pos, radius, life)`.
- **`BubbleVolume`** — the Air user's pocket. Inside it you breathe and you're safe from drowning, but you move
  slowly. It can follow the caster and/or expire. Deploy with `BubbleVolume.Deploy(pos, radius, life, follow)`.

**Freezing a bubble:** if a Water user freezes the water around an Air user's submerged bubble, the bubble
freezes solid and pops (`BubbleVolume.FreezeNear`, called when an ice cast lands). Its occupants are left in the
ice trap — pinned and, if they aren't water users, suffocating — until it melts or a Fire user thaws it.

## Movement speed & atmosphere

`SwimLocomotion` takes over while submerged: full-3D swimming (you move where you look, with explicit
ascend/descend and gentle buoyancy), speed scaled by `UnderwaterController.SpeedScale` (water users full
speed, others slower, bubble/ice slower still), capped and fed into the shared `ComfortVignette` so motion
stays comfortable in a headset. It disables the ground rig while under and hands movement back on the surface.
On the player rig, `controlsAtmosphere` toggles thicker URP fog and a blue tint while submerged, capturing and
restoring the scene's fog so surfacing looks right.

## Setup (editor)

1. Add a `WaterVolume` on a box trigger covering the water body.
2. Put `UnderwaterController` and `SwimLocomotion` on the player rig (`fromPlayerLoadout = true`,
   `controlsAtmosphere = true`); `SwimLocomotion` auto-finds the camera, ground rig, and vignette. Add
   `UnderwaterController` to enemies you want to drown or trap.
3. Casting wires the rest: underwater, a Water **ice** cast freezes a trap ahead of you, an Air **primary**
   deploys a following bubble, and a **Fire** cast thaws nearby ice. Tune ranges/lifetimes in `UnderwaterTuning`.

## What's next / boundaries

- ✅ **Systems-adjacent (now in):** buoyant 3D swim locomotion with the comfort vignette; enemies trapped
  by ice via the status system; and freeze / bubble / thaw triggered from element casts.
- **Content (needs an artist / can't be produced here):** the organic models — small **fish** and **coral**
  decoration, the **octopus** enemies (grab + interrupt + drown), and the **water-dragon/serpent** boss
  (ice + water, guarding a reward). I can ship their AI and **blocky placeholder** stand-ins, but not the
  detailed organic meshes; **real water rendering** (surface, caustics, refraction — only tinted fog + a flat
  plane here) and the **scene/prefab** wiring are yours.

## Testing

`OxygenModel` and `UnderwaterAbilityRules` are unit-tested. The volumes, drowning damage, and atmosphere are
runtime behaviour.
