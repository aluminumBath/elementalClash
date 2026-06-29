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
                case BiomeType.CoralReefForest:
                    if (r < 0.28) return CreatureKind.WaterDragon;
                    if (r < 0.5) return CreatureKind.Mermaid;
                    if (r < 0.72) return CreatureKind.Crab;
                    if (r < 0.86) return CreatureKind.Eel;
                    if (r < 0.93) return CreatureKind.Skimfin;
                    if (r < 0.98) return CreatureKind.Goldkoi;
                    return CreatureKind.Tidewarden; // rarest — colossal sea creature
                case BiomeType.Swamp:
                case BiomeType.Marsh:
                    if (r < 0.24) return CreatureKind.Crocodile;
                    if (r < 0.46) return CreatureKind.Snake;
                    if (r < 0.64) return CreatureKind.Eel;
                    if (r < 0.8) return CreatureKind.Mermaid;
                    if (r < 0.92) return CreatureKind.WaterDragon;
                    return CreatureKind.Gillcloak; // rare mantled aquatic
                case BiomeType.Desert:
                    if (r < 0.45) return CreatureKind.EarthMole;
                    if (r < 0.75) return CreatureKind.Horse;
                    return CreatureKind.Snake;
                case BiomeType.Mountains:
                    if (r < 0.4) return CreatureKind.EarthMole;
                    if (r < 0.66) return CreatureKind.EarthCat;
                    if (r < 0.86) return CreatureKind.Roc;
                    if (r < 0.95) return CreatureKind.Ridgewing;
                    return CreatureKind.Skytyrant; // rarest — apex flyer
                case BiomeType.CloudTemple:
                    if (r < 0.4) return CreatureKind.AirDragonfly;
                    if (r < 0.7) return CreatureKind.AirJellyfish;
                    return CreatureKind.Thunderbird;
                case BiomeType.Plains:
                    if (r < 0.35) return CreatureKind.Horse;
                    if (r < 0.6) return CreatureKind.EarthCat;
                    if (r < 0.8) return CreatureKind.Rhino;
                    return CreatureKind.Tiger;
                case BiomeType.ForestTemple:
                    if (r < 0.32) return CreatureKind.Horse;
                    if (r < 0.55) return CreatureKind.EarthCat;
                    if (r < 0.74) return CreatureKind.Monkey;
                    if (r < 0.9) return CreatureKind.Tiger;
                    if (r < 0.96) return CreatureKind.Glidewisp;
                    return CreatureKind.Direstalker; // rare land apex
                default:
                    return CreatureKind.Horse;
            }
        }
    }
}
