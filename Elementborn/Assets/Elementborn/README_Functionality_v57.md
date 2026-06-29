# v57 Functionality: SiteKind Configure Compatibility

This patch addresses:

```text
Assets\Elementborn\Game\World\WorldSpawnPlacer.cs(136,55): error CS1503: Argument 1: cannot convert from 'Elementborn.Core.SiteKind' to 'string'
Assets\Elementborn\Game\World\WorldSpawnPlacer.cs(136,61): error CS1503: Argument 2: cannot convert from 'string' to 'Elementborn.Game.SiteInteriorController'
```

The local `WorldSpawnPlacer.cs` is calling a 2-argument site bridge method shaped like:

```text
Configure(SiteKind, string)
```

## Changes

Added exact overloads:

```text
SiteEntrance.Configure(SiteKind kind, string idOrTitle)
SiteExit.Configure(SiteKind kind, string id)
SiteInteriorController.Configure(SiteKind kind, string idOrTitle)
```

Also added broad compatibility overloads:

```text
Configure(object kindOrDefinition, string idOrTitle)
```

for future local scaffold calls.
