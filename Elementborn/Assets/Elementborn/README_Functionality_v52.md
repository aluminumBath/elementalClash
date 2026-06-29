# v52 Functionality: Compile Fix Pass 3

This patch addresses the next Unity compile layer reported after v51.

## Reported errors targeted

```text
CS1503 WindCapitalRegistry cannot convert argument 6 from string to float
CS1501 SiteInteriorController no overload for Configure takes 4 arguments
CS0103 PlayerMapMarkerTracker BuildMarkerId does not exist
CS0117 InteractionArbiter does not contain SignalInteract
```

## Changes

### PlayerMapMarkerTracker compatibility

Added:

```text
BuildMarkerId(MapMarkerType markerType, Vector3 worldPosition, string label)
```

Also added a compatibility overload:

```text
ReportOrUpdateMarker(string markerId, MapMarkerType markerType, Vector3 worldPosition, string label, bool isPersistent, string notes)
```

This supports older generated registries that used the 6th positional argument as notes.

### WindCapitalRegistry fix

Changed the map-marker call to use:

```text
notes: hook.Summary
```

instead of passing the summary as the 6th positional argument.

### InteractionArbiter compatibility

Added older input bridge entry points:

```text
SignalInteract()
SignalInteract(GameObject interactor)
SignalInteract(Transform interactor)
SignalInteract(Vector3 playerPosition)
```

### Canonical stale-file replacements

Added canonical versions of stale local files:

```text
Assets/Elementborn/Game/World/SiteInteriorController.cs
Assets/Elementborn/Game/VrInteractInput.cs
```

These are intentionally lightweight and package-safe.

## Apply script

Use:

```text
FIX_V52_COMPILE_ERRORS.ps1
```

It deletes stale local copies of:

```text
Assets\Elementborn\Game\InteractionArbiter.cs
Assets\Elementborn\Game\World\SiteInteriorController.cs
Assets\Elementborn\Game\VrInteractInput.cs
```

then copies the canonical v52 versions.

## Recommended next step

```text
1. Close Unity.
2. Extract v52 to a temp folder.
3. Run FIX_V52_COMPILE_ERRORS.ps1 from the repo root.
4. Reopen Unity.
5. Send the next first 10 unique Console errors if any remain.
```
