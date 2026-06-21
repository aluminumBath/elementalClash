namespace Elementborn.Core
{
    public enum WeatherKind { Clear, Rain, Blizzard, Sandstorm, HeatHaze, Tornado, Hurricane }

    /// <summary>Which weather a biome can throw at you. Pure + unit-tested.</summary>
    public static class WeatherProfiles
    {
        /// <summary>The non-clear weather a biome can produce.</summary>
        public static WeatherKind[] Severe(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Mountains:
                case BiomeType.CloudTemple:
                    return new[] { WeatherKind.Blizzard };
                case BiomeType.Desert:
                    return new[] { WeatherKind.Sandstorm, WeatherKind.HeatHaze };
                case BiomeType.Volcano:
                    return new[] { WeatherKind.HeatHaze };
                case BiomeType.Beach:
                case BiomeType.Island:
                case BiomeType.Swamp:
                case BiomeType.Marsh:
                    return new[] { WeatherKind.Rain, WeatherKind.Hurricane };
                case BiomeType.Plains:
                case BiomeType.ForestTemple:
                    return new[] { WeatherKind.Rain, WeatherKind.Tornado };
                case BiomeType.CapitalCity:
                    return new[] { WeatherKind.Rain };
                default:
                    return new WeatherKind[0];
            }
        }

        /// <summary>Roll the next weather for a biome — clear about half the time, else a biome storm.</summary>
        public static WeatherKind Pick(BiomeType biome, IRandomSource rng)
        {
            var severe = Severe(biome);
            if (severe.Length == 0) return WeatherKind.Clear;
            if (rng.NextUnit() < 0.5) return WeatherKind.Clear;

            int idx = (int)(rng.NextUnit() * severe.Length);
            if (idx >= severe.Length) idx = severe.Length - 1;
            return severe[idx];
        }
    }

    /// <summary>How weather slightly helps or hinders a channeler's element. Pure + unit-tested.</summary>
    public static class WeatherEffects
    {
        public static float ElementMultiplier(WeatherKind weather, Element element)
        {
            switch (weather)
            {
                case WeatherKind.Rain:
                    if (element == Element.Water) return 1.15f;
                    if (element == Element.Fire) return 0.85f;
                    return 1f;
                case WeatherKind.Blizzard:
                    if (element == Element.Water) return 1.15f;
                    if (element == Element.Fire) return 0.8f;
                    if (element == Element.Air) return 0.9f;
                    return 1f;
                case WeatherKind.Sandstorm:
                    if (element == Element.Earth) return 1.15f;
                    if (element == Element.Air) return 1.05f;
                    return 0.95f;
                case WeatherKind.HeatHaze:
                    if (element == Element.Fire) return 1.2f;
                    if (element == Element.Water) return 0.85f;
                    return 1f;
                case WeatherKind.Tornado:
                    if (element == Element.Air) return 1.2f;
                    return 0.95f;
                case WeatherKind.Hurricane:
                    if (element == Element.Water) return 1.15f;
                    if (element == Element.Air) return 1.15f;
                    if (element == Element.Fire) return 0.8f;
                    return 0.95f;
                default:
                    return 1f; // Clear
            }
        }
    }
}
