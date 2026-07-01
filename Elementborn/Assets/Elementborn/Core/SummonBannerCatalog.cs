using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// The summon banners and the single source of summon rarity. Pools are existing creatures — the six
    /// rare combat companions plus the rideable mounts — so everything summoned is immediately usable (a
    /// companion via the companion summoner, a mount via the mount summoner) with no new art. Rarity is
    /// assigned here rather than on <see cref="CreatureInfo"/>, keeping this the one balance knob.
    /// </summary>
    public static class SummonBannerCatalog
    {
        // Apex / iconic creatures.
        private static readonly CreatureKind[] LegendaryPool =
        {
            CreatureKind.Phoenix, CreatureKind.FireDragon, CreatureKind.WaterDragon,
            CreatureKind.Skytyrant, CreatureKind.Tidewarden, CreatureKind.Direstalker,
            CreatureKind.EarthDragon, CreatureKind.AirDragon
        };

        // Strong companions and high-end mounts.
        private static readonly CreatureKind[] EpicPool =
        {
            CreatureKind.IceCat, CreatureKind.ElectricSquirrel, CreatureKind.Roc, CreatureKind.Thunderbird,
            CreatureKind.Ridgewing, CreatureKind.Goldkoi, CreatureKind.Gillcloak, CreatureKind.Skimfin,
            CreatureKind.BoneBehemoth, CreatureKind.CoralLeviathan
        };

        // Everyday helpers and mounts.
        private static readonly CreatureKind[] RarePool =
        {
            CreatureKind.Spider, CreatureKind.WaterCat, CreatureKind.Dog, CreatureKind.EarthMole,
            CreatureKind.AirDragonfly, CreatureKind.AirJellyfish, CreatureKind.Horse, CreatureKind.Rhino,
            CreatureKind.Glidewisp,
            CreatureKind.StormWolf, CreatureKind.VoltWolf
        };

        /// <summary>The stable id for the rotating featured slot. Pity and the lost-50/50 guarantee persist under
        /// this id, so they carry across rotations even as the featured creature changes.</summary>
        public const string FeaturedSlotId = "featured";

        // The featured rotation: one themed beacon per Legendary, cycled by period (see FeaturedForPeriod).
        private static readonly (string Name, CreatureKind Kind)[] Rotation =
        {
            ("Flamecaller Beacon", CreatureKind.Phoenix),
            ("Emberwyrm Beacon",   CreatureKind.FireDragon),
            ("Tidewyrm Beacon",    CreatureKind.WaterDragon),
            ("Stormcrown Beacon",  CreatureKind.Skytyrant),
            ("Deepward Beacon",    CreatureKind.Tidewarden),
            ("Nightprowl Beacon",  CreatureKind.Direstalker),
        };

        /// <summary>No rate-up: any Legendary is equally likely. The "collect anything" pool.</summary>
        public static readonly SummonBanner Standard =
            new SummonBanner("standard", "Wild Beacon", RarePool, EpicPool, LegendaryPool);

        /// <summary>The first featured banner (rotation period 0 — the Phoenix-rate-up Flamecaller Beacon). The
        /// live featured banner rotates; see <see cref="FeaturedForPeriod"/> and the controller's CurrentFeatured.</summary>
        public static readonly SummonBanner Featured = FeaturedForPeriod(0);

        public static readonly IReadOnlyList<SummonBanner> All = new[] { Standard, Featured };

        public static SummonBanner ById(string id)
        {
            if (!string.IsNullOrEmpty(id))
                foreach (var b in All) if (b.Id == id) return b;
            return Standard;
        }

        /// <summary>The rarity a banner assigns to a creature (Rare if it isn't in any pool).</summary>
        public static SummonRarity RarityOf(CreatureKind kind)
        {
            foreach (var k in LegendaryPool) if (k == kind) return SummonRarity.Legendary;
            foreach (var k in EpicPool) if (k == kind) return SummonRarity.Epic;
            return SummonRarity.Rare;
        }

        /// <summary>How many featured banners are in the rotation.</summary>
        public static int FeaturedRotationLength => Rotation.Length;

        /// <summary>The featured banner for a rotation period (periods cycle; negatives wrap safely). The slot id is
        /// constant (<see cref="FeaturedSlotId"/>) so pity carries across rotations; only the name/creature change.</summary>
        public static SummonBanner FeaturedForPeriod(int period)
        {
            int n = Rotation.Length;
            int i = ((period % n) + n) % n;
            var entry = Rotation[i];
            return new SummonBanner(FeaturedSlotId, entry.Name, RarePool, EpicPool, LegendaryPool,
                hasFeature: true, featured: entry.Kind);
        }

        /// <summary>The rotation period for a UTC instant: whole <paramref name="periodDays"/>-day windows since
        /// <paramref name="epochUtc"/>, clamped at 0 before the epoch. Takes the time as an argument (no hidden
        /// clock), so it's pure and unit-tested.</summary>
        public static int PeriodFor(System.DateTime utcNow, System.DateTime epochUtc, int periodDays)
        {
            if (periodDays < 1) periodDays = 1;
            double days = (utcNow - epochUtc).TotalDays;
            if (days <= 0) return 0;
            return (int)(days / periodDays);
        }
    }
}
