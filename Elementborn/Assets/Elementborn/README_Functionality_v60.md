# v60 Functionality: Editor-Safe Build Rounded Playable Scene

This patch addresses the editor menu error:

```text
InvalidOperationException: The following game object is invoking the DontDestroyOnLoad method: PlayerInventoryTracker.
Notice that DontDestroyOnLoad can only be used in play mode and, as such, cannot be part of an editor script.
```

## Root cause

`ElementbornPlayableSceneBuilder.BuildRoundedPlayableScene()` calls:

```text
ElementbornRuntimeBootstrap.EnsureRuntimeSystems()
```

from an editor menu. That eventually calls:

```text
ElementbornRuntimeBootstrap.EnsureSingleton<T>()
```

which created a GameObject and immediately called:

```text
DontDestroyOnLoad(go)
```

Unity does not allow `DontDestroyOnLoad` during edit-mode editor scripts.

## Change

`ElementbornRuntimeBootstrap.EnsureSingleton<T>()` now only calls `DontDestroyOnLoad` while the game is actually in Play Mode:

```text
if (Application.isPlaying)
{
    DontDestroyOnLoad(go);
}
```

In edit mode, generated runtime-system placeholders remain normal scene objects so the playable scene builder can finish and the scene can be saved.
