# VR setup — getting into the headset

The game already has VR combat (gesture casting via `VrInputProvider` / `VrGestureProvider`) and now comfort
locomotion (`VrComfortLocomotion`). What VR needs that can't be done from a code-only environment is the **XR
plug-in configuration** (an Editor/project step) and **on-headset tuning** (needs a device). This is the
checklist.

## 1. Enable an XR plug-in

Project Settings ▸ **XR Plug-in Management** → Install, then tick a provider per platform:

- **Meta Quest (Android):** Oculus (or OpenXR with the Meta feature group).
- **PCVR (Windows):** OpenXR, with the interaction profiles for your controllers (Oculus Touch, Index, etc.).

Set the **Tracking Origin Mode to Floor** (XR Origin) so the player's real height maps to the world — that's what
makes standing/roomscale correct, and it's why height "calibration" is a reset rather than a necessity.

## 2. Use the generated VR rig

`Elementborn ▸ Bootstrap ▸ Build Player Rig Prefabs` builds `PlayerRig_VR` in the XR-origin shape this expects:

```
PlayerRig_VR            ← XR origin: CharacterController + VrComfortLocomotion + VrInputProvider + combat/interaction
  └─ Camera Offset      ← the camera offset transform (height calibration nudges this)
       └─ Head (Camera) ← the HMD camera: Camera + AudioListener + ComfortVignette
```

Add the XR package's tracking components to match your setup: an XR Origin / Tracked Pose Driver on the camera so
the HMD drives `Head`, and tracked controller objects under the origin (the gesture casting reads controller
velocity/buttons via `InputDevices`, so it needs the controllers tracked, not an Input Actions asset).

## 3. Comfort locomotion (`VrComfortLocomotion`)

On the rig root, configured with sensible defaults:

- **Turning** — snap by default (`snapAngle` 45°, re-arms when the stick re-centres) or switch `turnStyle` to
  smooth. Right thumbstick. A comfort-vignette pulse plays during the turn.
- **Moving** — left thumbstick walks in the direction you're facing, with a gentle speed-scaled vignette.
- **Recenter** — right primary button yaws the rig so the headset faces forward again.
- **Height reset** — left primary button offsets the Camera Offset so your head reads as the standing reference.

All vignetting honors the **comfort-vignette** setting (Settings overlay), through the shared `ComfortVignette` on
the camera. Surfacing snap-vs-smooth and the turn angle in the Settings UI is a small follow-up.

## 4. What still needs a real headset

Code gets you a working rig; comfort is per-person and per-device:

- Tune `snapAngle`, `moveSpeed`, and the vignette strengths on-device — what reads as comfortable varies a lot.
- Verify controller mappings against the actual interaction profile (button/stick features can differ by device).
- Check framerate headroom on Quest standalone (the cel-shaded look is cheap, but draw calls and the vignette
  overlay still cost something) and confirm no judder.

These are the genuinely hardware-bound parts; everything up to them is in the project.
