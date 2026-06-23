using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// One place to ask "how high is the ground here?". Prefers the <see cref="MeshTerrainBuilder"/> if one
    /// built the world, otherwise falls back to a Unity <see cref="Terrain"/>, otherwise returns the point's
    /// own height. Spawns, structures, mounts, creatures, and companions all snap through this so either
    /// terrain path works unchanged.
    /// </summary>
    public static class TerrainHeight
    {
        public static float Sample(Vector3 pos)
        {
            if (MeshTerrainBuilder.Instance != null && MeshTerrainBuilder.Instance.TrySample(pos, out float y))
                return y;

            var terrain = Terrain.activeTerrain;
            if (terrain != null) return terrain.SampleHeight(pos) + terrain.GetPosition().y;

            return pos.y;
        }
    }
}
