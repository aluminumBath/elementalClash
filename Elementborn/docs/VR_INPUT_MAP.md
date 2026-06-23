# VR input map — what's bound, and what isn't (yet)

Elementborn has two input paths. **Desktop/flat** uses the rebindable Input System action map
(`InputBindings`) plus a few overlays that read the keyboard directly. **VR (Quest / OpenXR)** uses
controller-velocity gestures (`VrInputProvider` / `VrGestureProvider`) and thumbstick comfort locomotion
(`VrComfortLocomotion`). The two paths were built at different times, so several desktop actions have **no VR
binding yet**. This doc is the running checklist.

## Combat — fully mapped in VR

| Action | Desktop | VR |
| ------ | ------- | -- |
| Primary cast | Left mouse / RT | Right-hand thrust gesture |
| Secondary cast | Right mouse / LT | Left-hand thrust gesture |
| Defend | `Defend` action | Right controller **B** |
| Dash | `Dash` action | Right controller **A** |
| Heavy | hold `ExtendedCast` + Primary | gesture stance (motion layer) |
| Sweep | hold `ExtendedCast` + Secondary | gesture stance (motion layer) |
| Signature | hold `ExtendedCast` + Defend | per-element special gesture (VR-first) |

## Locomotion & comfort — mapped in VR

Thumbstick walk (face-relative), snap/smooth turn, **Recenter** (right **thumbstick-click**), **Height calibrate
/ seated reset** (left **X**). Desktop uses WASD + mouse-look on the rig (intentionally not rebindable).

## ⚠️ Not mapped to VR (the gap list)

These are reachable on desktop but currently have **no controller/gesture binding**, so a headset-only player
can't trigger them:

1. **Interact** — talk to NPCs, pick up / activate, mount via interact, feed a companion. Desktop: the
   `Interact` action. VR: nothing — `VrInputProvider` only reads A/B, and the "XRI grab" hook is still a
   "later" note. **Highest-impact gap** (NPCs, quests, pickups, taming all run through Interact).
2. **Quest Log** — desktop key **L** (`QuestLogController` reads the keyboard directly). No VR open.
3. **Inventory** — desktop key **I** (`InventoryController`, keyboard-direct). No VR open.
4. **Social menu** — desktop key **J** (`SocialMenuController`, keyboard-direct). No VR open.
5. **Grimoire** — desktop key **G** (`GrimoireController`, keyboard-direct). No VR open.
6. **Character screen** — `CharacterScreenController`, keyboard-direct. No VR open.
7. **Settings menu** — the `Menu` action (keyboard/gamepad only, no XR binding). No VR open.
8. **Save / Load slots** — the `Slots` action (keyboard/gamepad only). No VR open.
9. **Element travel** — the `ElementTravel` action. No VR binding.
10. **Mount** (summon / ride) — the `Mount` action. No VR binding.
11. **Companion** (summon) — the `Companion` action. No VR binding.

In short: **all combat works in VR; almost none of the menus/overlays or the world-interaction verbs do.**

## VR binding conflict — resolved

Right **A** previously drove both Dash (combat) and Recenter (locomotion), so a press fired both. **Fixed**:
Recenter now lives on the **right thumbstick-click** (`primary2DAxisClick`), leaving A as Dash-only. The left
thumbstick-click and the controller menu buttons are still free for the menu/interact wiring above.

## Suggested approach when wiring these

- **Interact**: bind to right **grip** (or trigger) via `VrInputProvider`, feeding the existing
  `InteractionArbiter` — that single binding unlocks NPCs/quests/pickups/mounting.
- **Overlays (Quest/Inventory/Social/Grimoire/Character/Settings/Slots)**: a single **menu button** (left menu /
  hamburger) that opens a VR-friendly wrist or world-space hub, rather than one button per panel. The overlays
  already have `Show()/Hide()/Toggle()` — they just need a VR opener that doesn't read `Keyboard.current`.
- **Element travel / Mount / Companion**: radial or hub entries off the same menu button.
- The **A-button conflict** is already resolved (Recenter moved to the right thumbstick-click).

(Generated as an audit of `InputBindings`, `VrInputProvider`, `VrGestureProvider`, `VrComfortLocomotion`, and the
overlay controllers. Update as bindings are added.)
