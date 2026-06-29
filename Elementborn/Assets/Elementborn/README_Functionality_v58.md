# v58 Functionality: Fully Qualified SiteKind Configure Compatibility

This patch addresses the persistent local `WorldSpawnPlacer` error:

```text
WorldSpawnPlacer.cs(136,55): Argument 1 cannot convert from Elementborn.Core.SiteKind to string
WorldSpawnPlacer.cs(136,61): Argument 2 cannot convert from string to SiteInteriorController
```

The local call is:

```text
go.AddComponent<SiteEntrance>().Configure(kind, regionId + "_" + kind);
```

v57 added `Configure(SiteKind, string)`, but v58 makes the overload explicit:

```text
SiteEntrance.Configure(Elementborn.Core.SiteKind kind, string idOrTitle)
SiteExit.Configure(Elementborn.Core.SiteKind kind, string id)
SiteInteriorController.Configure(Elementborn.Core.SiteKind kind, string idOrTitle)
```

Using the fully-qualified enum type avoids namespace ambiguity or stale using/import issues.
