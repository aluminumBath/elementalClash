# v66 Functionality: HUD Save Compatibility + Boat API Safety

This patch addresses the current compile errors from the attached Unity log:

```text
Assets\Elementborn\Game\PlayerInventory.cs(204,42): error CS1061: 'PlayerAttunementHud' does not contain a definition for 'CaptureInto'
Assets\Elementborn\Game\PlayerInventory.cs(270,42): error CS1061: 'PlayerAttunementHud' does not contain a definition for 'RestoreFrom'
```

## Changes

### PlayerAttunementHud

v66 restores compatibility methods expected by local `PlayerInventory.cs`:

```text
PlayerAttunementHud.CaptureInto(...)
PlayerAttunementHud.RestoreFrom(...)
```

They are implemented as broad `params object[]` methods so they compile against legacy call shapes with zero, one, or multiple arguments.

The methods attempt to read/write attunement-like fields or properties when a save container is supplied, while remaining safe if the object shape is unknown.

### BoatController

The apply script also patches stale local references in-place:

```text
_rb.velocity -> _rb.linearVelocity
_rb.drag     -> _rb.linearDamping
```

This is included because the attached log still shows Unity API-updater warnings for `BoatController.cs`.
