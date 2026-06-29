# v65 Functionality: BoatController Unity 6 API Updater Fix

This patch addresses the Unity prompt:

```text
Some of this project's source files refer to API that has changed.
Assets/Elementborn/Game/Boats/BoatController.cs
```

## Root cause

`BoatController.cs` still used old Rigidbody APIs that Unity 6 wants to update.

## Changes

Migrated:

```text
Rigidbody.velocity -> Rigidbody.linearVelocity
Rigidbody.drag     -> Rigidbody.linearDamping
```

This should stop Unity from asking to run the API updater for:

```text
Assets/Elementborn/Game/Boats/BoatController.cs
```

## Notes

This patch targets the Unity 6 / 6000.x project currently being used.
