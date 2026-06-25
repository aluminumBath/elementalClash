// Intentionally left as a compatibility shim.
//
// The old contents of this file declared a static Elementborn.Core.WorldMap class.
// That collided with the generated runtime WorldMap class in Core/World/World.cs.
//
// The static map/minimap layout now lives in WorldMapLayout.cs as:
//   Elementborn.Core.WorldMapLayout
//
// Keeping this file as a no-op makes ZIP-overwrite installs safe: it overwrites
// any stale Assets/Elementborn/Core/WorldMap.cs that Unity may still be compiling.
