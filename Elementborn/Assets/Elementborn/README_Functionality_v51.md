# v51 Functionality: Compile Fix Pass 2

This patch addresses the next Unity compile layer reported after v50.

## Reported errors targeted

```text
CS0246 GearStatModifier could not be found
CS0535 BaseInteractable does not implement IInteractable.TryGetInteraction(Vector3, out Interaction)
CS0101 duplicate IInteractable definition in InteractionArbiter
CS0234 Elementborn.Core does not exist in ElementbornPolishDebugReportMenu
```

## Changes

### Canonical InteractionArbiter

Added/replaced:

```text
Assets/Elementborn/Game/InteractionArbiter.cs
```

This canonical version defines:

```text
Interaction
InteractionArbiter
```

It intentionally does **not** define `IInteractable`.

### Single IInteractable definition

The only valid interface definition is:

```text
Assets/Elementborn/Game/Interaction/IInteractable.cs
```

It remains a marker interface for compatibility with both older scaffold interactables and newer `BaseInteractable` objects.

### BaseInteractable compatibility

`BaseInteractable` remains compatible with the marker-style `IInteractable`, so it should no longer be forced to implement old `TryGetInteraction` methods.

### GearStatModifier compatibility

Verified present:

```text
Assets/Elementborn/Game/Equipment/GearStatModifier.cs
Assets/Elementborn/Game/Equipment/GearStatType.cs
```

If Unity reports `GearStatModifier` missing, the local project is likely in a mixed state where this file was not copied. The v51 script verifies it after copy.

### Removed stale Core import

Removed an unnecessary `using Elementborn.Core;` from:

```text
Assets/Elementborn/Editor/ElementbornPolishDebugReportMenu.cs
```

## Apply script

Use:

```text
FIX_V51_COMPILE_ERRORS.ps1
```

It deletes stale local files before copying:

```text
Assets/Elementborn/Game/Interaction/PlayerInteractor.cs
Assets/Elementborn/Game/BossController.cs
Assets/Elementborn/Game/InteractionArbiter.cs
```

Then it copies the v51 patch into the project and verifies required compatibility files.

## Recommended next step

```text
1. Close Unity.
2. Extract v51 to a temp folder.
3. Run FIX_V51_COMPILE_ERRORS.ps1 from the repo root.
4. Reopen Unity.
5. Send the next first 10 unique Console errors if any remain.
```
