# VR motion combat (gestures)

**Status:** phase 1 — a motion gesture recognizer and a per-element style table that **augment the existing VR
combat**. It plugs into the same pipeline as the flat/gamepad input, so nothing downstream changes. Stance
recognition and a dedicated mode are planned next (see Roadmap).

## How it fits

`VrGestureProvider` implements `IPlayerInputProvider` exactly like `FlatInputProvider`/`VrInputProvider`: it
samples the controllers, recognizes a motion, maps it to a `ChannelingIntent`, and raises `IntentProduced`.
`PlayerCombatController` resolves that intent through the existing `AbilitySystem`. So the gesture provider is a
drop-in: put `VrGestureProvider` on the VR rig prefab and point combat's input reference at it (it supersedes
the simpler thrust-only `VrInputProvider`, which stays for reference).

The recognizer (`GestureRecognizer`) and the table (`GestureProfile`) live in **Core** — pure, engine-agnostic,
and unit-tested with recorded motion, so they're verifiable without a headset.

## One style per element

Each element has its own motion vocabulary; a gesture that isn't in the active element's table does nothing,
which keeps the styles distinct.

| Element | Feel | Gesture → ability |
| --- | --- | --- |
| **Fire** | aggressive, linear | straight **Thrust** → blast · **Uppercut** → lightning · **Guard** → barrier |
| **Water** | flowing, circular | **Whip** (arc) → water jet · **Slam** (downward) → ice / sanguine grip · **Guard** → barrier |
| **Earth** | grounded, heavy | **Slam** → rock hurl · **Uppercut** → boulder · **Guard** → barrier |
| **Air** | light, evasive | two-hand **Push** → gust · **Whip** → air scythe · **DashStep** / A button → dash · **Guard** → barrier |

The provider also keeps **button fallbacks** (A = dash, B = defend) so play is reliable while you tune motion
thresholds. The active element is read from the player's loadout on enable, or set with `SetElement(...)`.

## Recognizer heuristics (motion-first)

`GestureRecognizer.Recognize(window, forward, up)` finds the peak-velocity sample, projects its direction into
the head frame, and picks the dominant axis: forward = **Thrust**, lateral = **Whip**, up = **Uppercut**, down
= **Slam**, and anything under the speed floor (or a backward retract) = **None**. Charge scales with peak
speed. Two-hand **Push** (both hands thrusting) and the **Guard** stance (both hands raised and still) are
derived in `VrGestureProvider`. This is deliberately simple and robust; richer pose recognition can replace it
behind the same interface.

## Stance layer

On top of the per-motion recognizer, a pure `StanceResolver` reads a per-hand `HandInput` snapshot (pose,
height, hold time, and the recognized motion) and resolves **two-handed stances and stance-modified combos**
before any single-hand strike:

- **Hold-to-charge** — holding a hand still in a pose (a clenched fist or open palm on controllers = grip +
  trigger) builds charge over time. The next strike fires with the greater of its motion-charge and the
  held-charge, so you can wind up a heavy hit instead of only swinging hard.
- **Block / guard pose** — both hands raised and still is a held `Defend`. It's *sustained*: the barrier
  re-applies on a channel tick while you hold the pose, so blocking is a stance you maintain, not a tap.
- **Ice flow (Water)** — the showcase stance combo. Hold **one hand in a fist** to form the ice, then make a
  **forward-and-down paddle stroke** with the other hand (a full-body "canoe-paddle" wave) to send it. While
  both persist it channels ice, charged by how long the fist has been held, and each paddle stroke pushes the
  flow — so it pulses with your paddling rhythm rather than firing once.

The resolver is engine-agnostic and unit-tested; `VrGestureProvider` supplies the snapshots (pose from
grip/trigger, height from controller position vs. the HMD, hold timers) and owns the channel timing
(`channelInterval`) and one-shot cooldown. Combos are per element, so the table grows as new elements get
signature stances (e.g., an earth "form-and-hurl," an air "sustained gust").



1. On the VR rig prefab, add `VrGestureProvider` and assign **head** (the HMD camera transform).
2. Set `PlayerCombatController.inputProviderBehaviour` to the `VrGestureProvider`.
3. Leave element on its default or let it auto-adopt the loadout; tune `minSpeed`, `castCooldown`, and
   `windowSeconds` on-device.
4. The XR rig, controllers, and locomotion are unchanged — this only changes how casts are triggered.

## Comfort & safety

Casts are rate-limited by `castCooldown` so combat isn't constant flailing, and big motions aren't *required*
(button fallbacks exist). Keep encounters short or add rest beats to avoid "gorilla arm," and respect room-scale
boundaries — the dedicated mode (below) is where pacing/stamina would be designed around motion properly.

## Testing without a headset

The recognizer and table are pure classes; `GestureTests` feeds synthetic motion windows and asserts the
classification and the per-element mappings. On-device threshold/comfort tuning is the part that needs you in a
headset.

## Roadmap (next steps)

- ✅ **Stance layer** — hold-to-charge, a held block pose, and stance-modified combos (Water's fist+paddle
  ice flow), layered on the motion recognizer via `StanceResolver`.
- ✅ **Dedicated mode** — a sparring arena tuned for motion: escalating waves, telegraphed/dodgeable
  enemies, combo scoring, and stamina pacing. `AbilitySystem` now gives each style two more moves (Heavy +
  Sweep). See **`ARENA.md`**.
- **Deeper per-gesture abilities** — today the table routes through the existing intents (≈3–4 moves/element).
  Adding *new* moves (e.g., a distinct fire hook) means extending `AbilitySystem` with extra outcomes; the
  `GestureProfile` is structured to grow into that.
- **Confluence switching** — a gesture/menu to change the active element (and thus style) for four-element
  players; the provider already exposes `SetElement`.
- **Hand tracking** — controller-free Quest hand tracking could feed the same recognizer with joint velocities.
