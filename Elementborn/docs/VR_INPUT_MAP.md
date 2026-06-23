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

## Interaction — mapped in VR

**Interact** is bound to the **right-hand grip**. `VrInteractInput` reads the grip (legacy XR
`CommonUsages.gripButton`) and signals the shared `InteractionArbiter`, which fires the current best interaction —
the *same* selection/prompt path the desktop Interact key uses. So talking to NPCs, picking up / activating,
mounting, taming, and the leyline-rift / checkpoint prompts all work from the headset. This was the highest-impact
gap. (The on-screen prompt is still a screen-space HUD — visible in flat play; in the headset it needs the
world-space UI work noted under the menus below.)

## Menu overlays — mapped in VR

A single **`VrOverlayHub`** is the headset entry point to the panels. The **left-hand menu button**
(`CommonUsages.menuButton`, read like the grip) opens a panel with a button per overlay — **Quests, Inventory,
Grimoire, Map, Social, Character, Settings** — each opening it through the overlay's public `Open()`, so none of
them need the keyboard in VR. (Desktop keeps its per-panel keys L/I/G/M/J/C/Esc; the hub is also on **Tab** for
testing.) The panels switch to **World Space** in the headset via `VrCanvasAdapter` (attached by the overlay
builders) and are placed in front of the player on open. The one remaining piece is the in-editor XRI raycaster
(see the gap list) that lets the controller ray click them.

## ⚠️ Not mapped to VR (the gap list)

Interact, the menu overlays, and the summon/travel verbs are now reachable in VR (above + the hub). What remains
is one in-editor step — and, optionally, dedicated bindings for the verbs:

1. **Save / Load, Element travel, Summon Mount, Summon Companion** — now entries on the `VrOverlayHub` (an
   **Actions** group), each calling the system's existing public method, so a headset player can trigger them
   without the keyboard. They still have **no dedicated controller binding/gesture** — a radial or button mapping
   off the menu would be nicer than a menu entry, but the access gap is closed.
2. **XRI UI raycaster (in-editor)** — overlay/hub canvases go World Space in VR (`VrCanvasAdapter`), but the
   controller-ray *click* needs an XRI `TrackedDeviceGraphicRaycaster` + `XRUIInputModule` on the rig, added in the
   editor — the same step `CharacterCreationUI` documents. Until then the panels render in-headset but the ray
   can't click them. **This is the one true remaining VR gap.**

In short: **combat, comfort locomotion, interaction, the menu overlays, and the summon/travel verbs are all
reachable in VR now; the one true remaining gap is the in-editor XRI UI raycaster (so the ray can click the
panels).** Dedicated controller bindings/gestures for the verbs (vs. the menu entries) would be the next polish.

## VR binding conflict — resolved

Right **A** previously drove both Dash (combat) and Recenter (locomotion), so a press fired both. **Fixed**:
Recenter now lives on the **right thumbstick-click** (`primary2DAxisClick`), leaving A as Dash-only. The left
thumbstick-click and the controller menu buttons are still free for the menu/interact wiring above.

## Suggested approach for the remaining gaps

- **Interact**: ✅ done — `VrInteractInput` reads the right grip and signals `InteractionArbiter`.
- **Overlays — built (canvas + opener); only the in-editor XRI raycaster remains.** A `VrOverlayHub` (left menu
  button / Tab) opens a panel that opens each overlay via its public `Open()`, and each overlay canvas switches to
  **World Space** in VR via `VrCanvasAdapter`, positioned in front of the HMD on open. What's deliberately *not*
  wired in code is the controller-ray click stack the legacy rig lacks: a `TrackedDeviceGraphicRaycaster` on the
  canvases, an `XRUIInputModule` on the event system, and **left/right controller objects with poses + an
  `XRRayInteractor`** on the rig. That's an in-editor XRI step (the same one `CharacterCreationUI` notes), best
  validated on a headset — so it's not wired blind. Once present, the hub and panels are clickable in-headset.
- **Element travel / Mount / Companion**: radial or hub entries off that same menu button.
- The **A-button conflict** is already resolved (Recenter moved to the right thumbstick-click).

(Generated as an audit of `InputBindings`, `VrInputProvider`, `VrGestureProvider`, `VrComfortLocomotion`, and the
overlay controllers. Update as bindings are added.)
