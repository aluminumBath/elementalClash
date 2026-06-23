# Input & rebinding

The flat/desktop control scheme supports **keyboard + mouse and gamepad at the same time**, and the discrete
actions are **rebindable at runtime**. It's built entirely in code (`InputBindings.cs`) — no `.inputactions`
asset to wire — and overrides persist to JSON. VR (Quest) uses XRI controller bindings and is unaffected; see
the note at the bottom.

## Default scheme

| Action | Keyboard / Mouse | Gamepad |
| --- | --- | --- |
| Move | `WASD` | Left stick |
| Look | Mouse | Right stick |
| Primary cast (hold to charge) | Left mouse | Right trigger |
| Secondary cast (hold to charge) | Right mouse | Left trigger |
| Defend | `F` | Left bumper |
| Dash | `Space` | A / Cross |
| Interact | `E` | X / Square |
| Element travel | `F` | Y / Triangle |
| Summon mount | `M` | D-pad up |
| Summon companions | `C` | D-pad down |
| Settings | `Esc` | Start |
| Save slots | `F8` | Select / Back |
| Controls menu | `F10` | — (open from Settings → Controls…) |
| Quick save / load | `F5` / `F9` | — |

Mounted movement also reads the left stick (and the triggers for fly up/down) through the mount's
keyboard-fallback path.

> The default for **Defend** and **Element travel** is the same key (`F`) — that overlap predates this scheme
> and is harmless (element travel only fires near water). The rebinding menu is the place to split them if you
> want distinct keys.

## Rebinding

Open the controls menu with **F10**, or from **Settings (Esc) → Controls…**. Each action shows two buttons:
its **keyboard/mouse** binding (left) and its **gamepad** binding (right). Tap either one and the menu listens
for the next matching control, then reassigns it; **Esc cancels** a listen. **Reset to defaults** clears every
override.

Under the hood this uses the Input System's interactive rebinding, constrained so a keyboard/mouse slot only
accepts keyboard/mouse controls and a gamepad slot only accepts gamepad controls (and only button-like
controls — sticks and mouse motion are excluded). Overrides are written to
`Application.persistentDataPath/elementborn_bindings.json` and reloaded on launch.

Movement and camera look are intentionally **not** rebindable — they're the locomotion sticks/mouse and are
handled directly by `FirstPersonRig`/`ThirdPersonRig`. Stick look speed is the serialized `stickLookSpeed`
(degrees/second) on each rig; mouse look still uses `lookSensitivity`, and both are scaled by the in-game
mouse-sensitivity / invert-Y settings.

## On-screen glyphs & prompts

The HUD interaction prompt is a **glyph + verb** row: when the glyph sprites are imported it shows the control
**image** next to the verb (e.g. a face-button icon then "Ride"); otherwise it falls back to a device- and
brand-aware **text token** ("[X] Ride", "[□] Ride", "[E] Ride"). It updates live — standing at a merchant reads
`E`/Shop on keyboard and flips to the gamepad face button the instant you touch a controller, and it follows
any rebind. `ControlGlyphs` resolves an action to its token/sprite from the binding's effective path,
`InputDeviceMonitor` latches which device you used last, and the controls menu shows the same tokens on its
binding buttons.

**Brand detection.** The active gamepad's family is detected at runtime from its layout/name (`ControlGlyphs.
DetectBrand`) and drives the face labels and glyphs: **Xbox** A/B/X/Y, **PlayStation** ✕/○/□/△ with L1/R1/L2/R2
and Options/Share, and **Switch** using the physical button positions (south = B, east = A …) with L/R, ZL/ZR
and +/−. Unknown pads fall back to the universal Xbox-style letters. You can also force it with
`ControlGlyphs.SetBrand(...)`.

**Controls legend.** A read-only **Controls** page (F1, or the **Legend** button in the rebind menu) lists
every action with its live glyph — the imported sprite when present, otherwise the token — and refreshes when
you switch device or pad brand. `ControlsLegendController` builds it on `UiTheme`.

A matching set of **placeholder glyph sprites** is generated to `Assets/Elementborn/Art/UI/glyphs/`
(`make_glyphs.py`): Xbox face buttons, bumpers/triggers, the d-pad, Start/Select, common keycaps, the mouse,
**plus PlayStation and Switch faces, shoulders, triggers, and Start/Select**. They're optional — prompts work as text tokens without them.
To use them as image glyphs, import as **Sprite (2D and UI)** and copy the folder into
`Resources/ElementbornUI/glyphs/`; `ControlGlyphs.Sprite(action)` then returns the one matching the active
binding and brand. The set is **brand-complete** (faces, shoulders, triggers, and Start/Select for Xbox,
PlayStation, and Switch), so a full controls legend looks right on every brand. For glyphs
*inline inside* TMP text, build a TMP sprite asset from the set (an editor step). `ControlGlyphs.SpriteName(path)`
is the filename map, so swapping in a purchased glyph pack is a one-method change.

## Interaction routing
A single `InteractionArbiter` on the player rig owns the Interact button and the on-screen interaction prompt.
World objects, NPCs, sidekicks, plant control, and the frogs implement `IInteractable` and *offer* an
interaction each frame; the arbiter shows one prompt and routes one press to the best offer (highest priority,
ties to nearest) — so overlapping interactables never double-fire or fight over the HUD.

## Power modifier (Heavy / Sweep / Signature)

The advanced moves are reachable on flat/gamepad through a held **modifier**, so no extra buttons are needed.
While the modifier is held, the three offensive casts become their advanced versions:

| Hold modifier + | Becomes |
| --- | --- |
| Primary cast | **Heavy** (charge still applies) |
| Secondary cast | **Sweep** |
| Defend | **Signature** (your element's hidden move) |

Dash and everything else are unchanged, and releasing the modifier returns the buttons to normal — note it
borrows Defend while held, so let go to guard. Default bind is **Left Shift** (keyboard) / **Right Bumper**
(gamepad); Right Bumper is used because Left Bumper is Defend, and the modifier must differ from the buttons it
remaps. Like every action here it's **rebindable** (it shows in the F1 legend and the F10 menu as
"Heavy/Sweep/Signature (hold)"). The mapping itself is the pure, unit-tested `ExtendedCast.Remap` in Core, which
`FlatInputProvider` calls at the moment each cast fires. VR reaches the same moves through gestures instead
(see `VR_COMBAT.md` / `HIDDEN_MOVES.md`).

## Where it lives

- **`InputBindings.cs`** — the code-built `Gameplay` action map. Each action carries binding index 0
  (keyboard/mouse) and index 1 (gamepad). Public `InputAction` properties (`PrimaryCast`, `Interact`, …), the
  `Rebindable` list used by the menu, `StartRebind`, `ResetAll`, and JSON save/load. Includes the `ExtendedCast` power modifier.
- **`RebindController.cs`** — the controls menu (built on `UiTheme`). Toggles with F10, suspends the player
  while open, and refreshes after each rebind.
- **`ControlsLegendController.cs`** — the read-only Controls legend (F1) showing each action's live glyph.
- **`ControlGlyphs.cs` / `InputDeviceMonitor.cs`** — active-device tracking and the action→token/sprite
  mapping behind the prompts.
- Consumers: `FlatInputProvider` (cast/defend/dash), `PlayerInteractor`, `ElementTravelController`,
  `MountSummoner`, `CompanionSummoner`, `SettingsController` (Menu), `SaveSlotController` (Slots).

## VR / Oculus

VR doesn't use this scheme. On Quest/PCVR the player casts and moves through the XRI controller bindings set up
with the XR rig in the editor (triggers, grips, thumbsticks, gestures), so changing the gamepad/keyboard
bindings here has no effect in-headset. Rebinding XR controller actions is a separate XRI concern and is out of
scope for this menu — which is why the rebinding UI is flat/gamepad only. A gamepad plugged into a PCVR
machine still works for the flat fallback rig, just not inside the headset session.

For VR **motion combat** (punch/whip gestures, per-element fighting styles) see **`VR_COMBAT.md`** — a
separate system from this flat/gamepad scheme.
