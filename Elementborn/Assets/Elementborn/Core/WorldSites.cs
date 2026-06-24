namespace Elementborn.Core
{
    /// <summary>The kinds of discoverable, instanced destinations seeded across the world. Each has an entrance out
    /// in the open world (placed by the spawner) that an interior loader opens into.</summary>
    public enum SiteKind { CaveMouth, Aerie, SunkenEntrance, TempleDoor, Spring }

    /// <summary>Where a site lives, which decides the traversal and the hazard it implies.</summary>
    public enum SiteDomain { Surface, Aerial, Underwater, Subterranean }

    /// <summary>What waits inside a site.</summary>
    public enum SitePayload { Boss, Treasure, RareCreature, Lore }

    public readonly struct SiteInfo
    {
        public SiteKind Kind { get; }
        public string DisplayName { get; }
        public SiteDomain Domain { get; }
        public SitePayload Payload { get; }
        public BiomeType PreferredBiome { get; }
        public string Lore { get; }

        public SiteInfo(SiteKind kind, string displayName, SiteDomain domain, SitePayload payload,
                        BiomeType preferredBiome, string lore)
        {
            Kind = kind;
            DisplayName = displayName;
            Domain = domain;
            Payload = payload;
            PreferredBiome = preferredBiome;
            Lore = lore;
        }
    }

    /// <summary>The catalogue of instanced destinations: each <see cref="SiteKind"/>'s domain, payload, the biome it
    /// prefers to spawn in, and its flavour. Pure and testable; the Game layer reads this to place entrances and
    /// (in the interior pass) to build what's inside.</summary>
    public static class SiteCatalog
    {
        public static SiteKind[] All =>
            (SiteKind[])System.Enum.GetValues(typeof(SiteKind));

        public static SiteInfo For(SiteKind kind)
        {
            switch (kind)
            {
                case SiteKind.CaveMouth:
                    return new SiteInfo(kind, "Cave Mouth", SiteDomain.Subterranean, SitePayload.Treasure,
                        BiomeType.Mountains, "A cold draft breathes from the dark, and something glitters far inside.");
                case SiteKind.Aerie:
                    return new SiteInfo(kind, "Wind-Torn Aerie", SiteDomain.Aerial, SitePayload.Boss,
                        BiomeType.CloudTemple, "A perch high above the clouds, reached only by the winged and the daring.");
                case SiteKind.SunkenEntrance:
                    return new SiteInfo(kind, "Sunken Gate", SiteDomain.Underwater, SitePayload.Boss,
                        BiomeType.Island, "Beneath the waves a drowned doorway waits in the crushing deep.");
                case SiteKind.TempleDoor:
                    return new SiteInfo(kind, "Sealed Temple", SiteDomain.Surface, SitePayload.Lore,
                        BiomeType.ForestTemple, "Ancient doors carved with the four realms stand shut, awaiting a worthy hand.");
                case SiteKind.Spring:
                    return new SiteInfo(kind, "Hidden Spring", SiteDomain.Surface, SitePayload.RareCreature,
                        BiomeType.Marsh, "Still water pools among the reeds, where rare creatures come to drink.");
                default:
                    return new SiteInfo(kind, kind.ToString(), SiteDomain.Surface, SitePayload.Lore, BiomeType.Plains, "");
            }
        }

        /// <summary>The site kind that prefers to spawn in this biome, if any.</summary>
        public static SiteKind? ForBiome(BiomeType biome)
        {
            foreach (var kind in All)
                if (For(kind).PreferredBiome == biome) return kind;
            return null;
        }
    }
}
