using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Generates (or holds) the world and exposes it to the map screen and spawners. Set a non-zero
    /// seed for a reproducible world, or leave it 0 to roll a fresh one each run.
    /// </summary>
    public sealed class WorldMapController : MonoBehaviour
    {
        [SerializeField] private int seed = 12345;
        [SerializeField] private int regionCount = 14;
        [SerializeField] private float mapSize = 1200f;

        public WorldMap World { get; private set; }
        public event System.Action<WorldMap> WorldReady;

        private void Awake() => Generate();

        /// <summary>Sets the seed + region count and regenerates. Used by the flow controller.</summary>
        public WorldMap Configure(int newSeed, int newRegionCount)
        {
            seed = newSeed;
            regionCount = newRegionCount;
            return Generate();
        }

        public WorldMap Generate()
        {
            var rng = seed != 0 ? new SystemRandomSource(seed) : new SystemRandomSource();
            World = WorldGenerator.Generate(rng, new WorldGenConfig { RegionCount = regionCount, MapSize = mapSize });
            WorldReady?.Invoke(World);
            Debug.Log($"[Elementborn] World generated: {World.Regions.Count} regions, capital = {World.Capital()?.Name}");
            return World;
        }
    }
}
