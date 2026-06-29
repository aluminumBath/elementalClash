# v64 Functionality: PlayerAttunementHud.Instance Compatibility

This patch addresses:

```text
Assets\Elementborn\Game\PlayerInventory.cs(270,33): error CS0117: 'PlayerAttunementHud' does not contain a definition for 'Instance'
Assets\Elementborn\Game\PlayerInventory.cs(204,33): error CS0117: 'PlayerAttunementHud' does not contain a definition for 'Instance'
```

## Root cause

v63 replaced `PlayerAttunementHud` with a null-safe version, but the local `PlayerInventory.cs` already expects:

```text
PlayerAttunementHud.Instance
```

## Change

v64 restores the singleton-style compatibility property:

```text
public static PlayerAttunementHud Instance { get; }
```

The getter is defensive:

- returns the existing HUD if present
- searches the scene for an existing HUD
- creates a minimal safe HUD if older systems request it before setup has created one

The null-safe behavior from v63 is preserved.
