# Compile Fix Notes - Pass 3

This pass fixes edit-mode test references after the runtime `WorldMap` / static `WorldMapLayout` split.

## Changes

- Updated `Assets/Tests/EditMode/CheckpointTests.cs`
  - `WorldMap.Checkpoints` -> `WorldMapLayout.Checkpoints`
  - `WorldMap.Rifts` -> `WorldMapLayout.Rifts`

- Updated `Assets/Tests/EditMode/MapNavigationTests.cs`
  - `WorldMap.Rifts` -> `WorldMapLayout.Rifts`
  - `WorldMap.BuildNetwork()` -> `WorldMapLayout.BuildNetwork()`
  - `WorldMap.BoundsMin` / `WorldMap.BoundsMax` -> `WorldMapLayout.BoundsMin` / `WorldMapLayout.BoundsMax`

- Updated `Assets/Tests/EditMode/ShardBurstTests.cs`
  - Added `using UnityEngine;` so `Mathf.Sqrt(...)` resolves.

## Note

The Unity console message `Failed to perform selection on text` is an Editor UI stack trace from clicking/selecting console text, not a C# compiler error in the game code.
