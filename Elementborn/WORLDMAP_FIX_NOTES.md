# WorldMap compile fix

This build avoids the duplicate `Elementborn.Core.WorldMap` error by splitting the two concepts:

- `Assets/Elementborn/Core/World/World.cs` keeps the generated runtime `WorldMap` instance class.
- `Assets/Elementborn/Core/WorldMapLayout.cs` contains the static minimap/rift/checkpoint layout.
- `Assets/Elementborn/Core/WorldMap.cs` is now an empty compatibility shim so ZIP-overwrite installs replace the stale static `WorldMap` file.

If Unity still reports `Assets/Elementborn/Core/WorldMap.cs(13,25): error CS0101`, manually open that file in your Unity project. It should contain only comments. If it still says `public static class WorldMap`, delete it or replace it with the shim in this ZIP.

Recommended import method:

1. Close Unity.
2. Extract this ZIP over your project and allow overwrites.
3. Delete `Library/ScriptAssemblies` or the full `Library` folder if Unity still shows stale compiler output.
4. Reopen Unity.
