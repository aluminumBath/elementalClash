# v71 Functionality: Single EventSystem Enforcement

The latest editor log showed repeated runtime warnings:

```text
There are 2 event systems in the scene. Please ensure there is always exactly one event system in the scene
UnityEngine.EventSystems.EventSystem:Update()
```

## Why this matters

Duplicate EventSystems can cause UI selection/input bugs, duplicated update warnings, and unpredictable behavior with generated canvases, world-map UI, and admin/debug panels.

## Changes

v71 adds:

```text
Assets/Elementborn/Game/UI/ElementbornEventSystemUtility.cs
Assets/Elementborn/Editor/ElementbornEventSystemRepairMenu.cs
```

## Runtime guard

`ElementbornEventSystemGuard` installs automatically after scene load and deduplicates EventSystems during the first few frames of Play Mode. This catches late-created UI canvases.

## Editor repair menu

New menu:

```text
Elementborn -> UI -> Fix Duplicate EventSystems In Open Scene
```

It keeps one EventSystem, removes/disables duplicates, marks the scene dirty, and saves assets.

## Patched creators

The following files now use the central utility instead of creating EventSystems directly:

```text
BootstrapUiFactory.cs
RuntimeSceneSafetyNet.cs
WorldMapView.cs
ElementbornUnityProjectSetupWizard.cs
ElementbornUiPrefabFactory.cs
ElementbornPlayableSceneBuilder.cs
```

## Recommended use

After applying v71:

1. Reopen Unity and wait for compile.
2. Run `Elementborn -> UI -> Fix Duplicate EventSystems In Open Scene`.
3. Save scene/project.
4. Press Play again.
