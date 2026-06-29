# v61 Functionality: Unity 6 Built-in Font Compatibility

This patch addresses:

```text
ArgumentException: Arial.ttf is no longer a valid built in font. Please use LegacyRuntime.ttf
```

## Root cause

Unity 6 no longer accepts:

```text
Resources.GetBuiltinResource<Font>("Arial.ttf")
```

Some generated UGUI/editor prefab builders still used that older built-in font name.

## Changes

- Replaced `Arial.ttf` references under `Assets/Elementborn` with Unity 6-compatible font loading.
- Added `ElementbornBuiltinFontUtility.GetDefaultFont()`, which first tries:
  - `LegacyRuntime.ttf`
- It also falls back safely for older Unity versions.
- Patched `AdminWristUiSetupMenu.Label(...)`, which was blocking:
  - `Elementborn -> Playtest -> Run Test Readiness Setup`

## After applying

Run:

```text
Elementborn -> Playtest -> Run Test Readiness Setup
```
