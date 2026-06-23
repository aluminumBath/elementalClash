# Hidden signature moves

**Status:** in. Each element has one hidden "signature" move, triggered by a special gesture rather than the
normal kit. They run through their own small path: the `VrGestureProvider` recognises the gesture and emits a
`Signature` intent, `PlayerCombatController` routes it to `HiddenMoveController`, and the effect applies. The
mapping and tuning (`HiddenMoves`) and the gesture predicates (`HiddenGestures`) are pure and unit-tested.

| Element | Gesture | Effect |
| --- | --- | --- |
| **Water** | spin your wrist (underwater) | a forward dash burst in the direction you face |
| **Earth** | cross arms, hands at the shoulders | rock armor: a big damage shield + a self-slow, timed |
| **Air** | spin a full 360° with arms raised | a tornado — damages and launches everyone nearby, you included |
| **Fire** | hand at your mouth, palm up + a button | a cone of fire breath that burns |

## Details

- **Water spin-dash** — a wrist twist past an angular-speed threshold while submerged fires a short, capped
  forward burst (and nudges the comfort vignette). Gated to underwater, matching the fiction.
- **Earth rock armor** — crossed-arms pose applies `Damageable.Shield` (a large incoming-damage cut) plus a
  Slow, for a few seconds. The defense boost is real immediately; the movement-slow rides the same locomotion
  hook the underwater speed-scale uses (so the player rig honours it once that hook is wired).
- **Air tornado** — a near-complete body turn with both hands raised hits everyone in a radius with damage and
  an upward launch. The caster is launched too — a best-effort upward burst here; the full launch + fall
  control arrives with the mobility moves.
- **Fire breath** — a hand brought up to the mouth plus a trigger press burns everyone in a forward cone.

All of these obey the incapacitation rule: while stunned, grabbed (octopus), or frozen (ice trap), no cast —
signatures included — goes off.

## Setup (editor)

Add `HiddenMoveController` to the player rig alongside `PlayerCombatController` (it finds the camera, the
`CharacterController`, and the `ComfortVignette`). Detection thresholds live on `VrGestureProvider`
(`spinAngularSpeed`, `fullTurnDegrees`, `mouthDistance`, `signatureCooldown`); the effect numbers live in
`HiddenMoves`.

## Non-VR

On flat/gamepad the signature move is reached without a special gesture: hold the **power modifier** (Left Shift
/ Right Bumper) and press **Defend**. The same modifier turns Primary into Heavy and Secondary into Sweep. It's
rebindable through the normal controls menu, and routes through `PlayerCombatController` to `HiddenMoveController`
exactly as the VR gesture does. See `INPUT.md` for the full table.

## Boundaries

The gesture thresholds are inherently fuzzy and want an on-device tuning pass — especially the wrist-spin and
the full-turn detection. The tornado's self-launch is approximate until the dedicated launch/fall-control work
lands, VFX reuse the element placeholders, and signatures are VR-gesture moves for now (a flat/gamepad binding
is a small follow-up).

## Testing

`HiddenMoves` (element mapping) and `HiddenGestures` (turn / wrist-spin / crossed-arms / hand-at-mouth
predicates) are unit-tested. Gesture detection on real devices and the applied effects are runtime behaviour.
