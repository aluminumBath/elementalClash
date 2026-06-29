# v55 Functionality: Site/Interior Compile Fix

This patch addresses the next compile layer after v54.

## Reported errors targeted

```text
SiteInteriorController.cs CS0121 ambiguous Enter(null)
SiteEntrance.cs CS1503 cannot convert SiteInfo to string
SiteEntrance.cs CS1503 cannot convert Vector3 to GameObject
```

## Changes

- Replaced `SiteInteriorController` with a canonical compatibility version.
- Fixed internal overload calls by casting `null` explicitly to `GameObject`.
- Added broad compatibility overloads:
  - `Enter(object request, Vector3 fallbackExit)`
  - `Enter(string id, Vector3 fallbackExit)`
  - `Exit(object request, Vector3 fallbackExit)`
- Added canonical `SiteEntrance`.
- Added canonical `SiteExit`.
- Updated the apply script to delete stale local `SiteEntrance` / `SiteExit` files before copying.
- Updated the apply script to delete stranded `Assets\Tests.meta`, which caused Unity to recreate an empty `Assets\Tests` folder warning.
