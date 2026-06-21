namespace Elementborn.Core
{
    /// <summary>
    /// Which bestiary creatures roam a given biome. Returns non-companion mounts/beasts only — the rare
    /// combat companions are acquired differently. Deterministic per RNG.
    /// </summary>
    public static class Wildlife
    {
        public static CreatureKind Pick(BiomeType biome, IRandomSource rng)
        {
            double r = rng.NextUnit();
            switch (biome)
            {
                case BiomeType.Volcano:
                    return r < 0.6 ? CreatureKind.FireDragon : CreatureKind.EarthMole;
                case BiomeType.Beach:
                case BiomeType.Island:
                    return r < 0.4 ? CreatureKind.WaterDragon : CreatureKind.Mermaid;
                case BiomeType.Swamp:
                case BiomeType.Marsh:
                    return r < 0.5 ? CreatureKind.Mermaid : CreatureKind.WaterDragon;
                case BiomeType.Desert:
                    return r < 0.6 ? CreatureKind.EarthMole : CreatureKind.Horse;
                case BiomeType.Mountains:
                    return r < 0.5 ? CreatureKind.EarthMole : CreatureKind.EarthCat;
                case BiomeType.CloudTemple:
                    return r < 0.5 ? CreatureKind.AirDragonfly : CreatureKind.AirJellyfish;
                case BiomeType.Plains:
                case BiomeType.ForestTemple:
                    return r < 0.5 ? CreatureKind.Horse : CreatureKind.EarthCat;
                default:
                    return CreatureKind.Horse;
            }
        }
    }
}
