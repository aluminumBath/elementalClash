# v83 Functionality: Dummy Reset NullReference Fix

## Problem fixed

Running:

```text
Elementborn -> Prototype -> Clear Prototype Save And Reset Open Scene
```

could throw:

```text
NullReferenceException
ElementbornPrototypeDummyEnemy.SetVisibleAndCollidable(...)
ElementbornPrototypeDummyEnemy.ResetDummy(...)
```

## Root cause

The editor menu called `ResetDummy()` outside Play Mode before Unity had called the dummy's `Awake()`. That meant cached arrays like `renderers` and `colliders` were still null.

## Changes

`ElementbornPrototypeDummyEnemy` now:

- caches renderers/colliders lazily before every reset/visibility operation
- safely creates/updates the health label outside Play Mode
- avoids null array access
- avoids repeatedly recreating hit-flash materials every frame
- keeps the health label visible when the dummy body is hidden
- resets the dummy safely from both editor menus and Play Mode

`ElementbornPrototypeStateRepairMenu` now wraps dummy reset in a defensive try/catch so one bad scene object cannot break the full reset operation.
