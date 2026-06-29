# v62 Functionality: Story Debug Dashboard Null-Safety

This patch addresses:

```text
NullReferenceException
Elementborn.Game.EditorTools.StorySystemsDebugDashboardSetupMenu.InstallDashboard()
Elementborn.Game.EditorTools.ElementbornPlayableSceneBuilder.InstallGeneratedSystems()
Elementborn.Game.EditorTools.ElementbornPlayableSceneBuilder.BuildRoundedPlayableScene()
```

## Likely cause

The generated Story Systems Debug Dashboard was still fragile during editor scene generation. The most likely immediate null source was font/text creation after the Unity 6 font migration, but the menu also assumed generated RectTransforms and dashboard UI references always existed.

## Changes

- Hardened `ElementbornBuiltinFontUtility.GetDefaultFont()`:
  - tries `LegacyRuntime.ttf`
  - tries legacy `Arial.ttf` only as a safe fallback
  - tries OS fonts such as Segoe UI / Arial / Liberation Sans
- Added `ElementbornBuiltinFontUtility.ApplyDefaultFont(Text)`.
- Patched generated UI text assignments to use `ApplyDefaultFont(...)`.
- Hardened `StorySystemsDebugDashboardSetupMenu`:
  - creates UI objects with `RectTransform`
  - verifies/creates missing `RectTransform`s
  - guards text creation
  - guards dashboard refresh
  - avoids hard-crashing the playable scene builder when dashboard UI setup is incomplete

## After applying

Run:

```text
Elementborn -> Playable Setup -> Build Rounded Playable Scene
```
