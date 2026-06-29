# v56 Functionality: WorldSpawnPlacer Configure Compatibility

This patch addresses the last reported compiler error:

```text
Assets\Elementborn\Game\World\WorldSpawnPlacer.cs(136,45): error CS1501: No overload for method 'Configure' takes 2 arguments
```

The local project has a `WorldSpawnPlacer.cs` file that is not part of the current combined ZIP, so v56 fixes the target site bridge classes instead of replacing `WorldSpawnPlacer`.

## Changes

Added 2-argument compatibility overloads:

### SiteEntrance

```text
Configure(string id, SiteInteriorController target)
Configure(object siteDefinition, SiteInteriorController target)
Configure(object siteDefinition, Vector3 fallbackExit)
```

### SiteExit

```text
Configure(object siteDefinition, SiteInteriorController target)
```

### SiteInteriorController

```text
Configure(object siteDefinition, Vector3 exitPosition)
Configure(object siteDefinition, Transform exit)
```

These let older local world/site setup scripts pass `SiteInfo`-style objects without compile errors.
