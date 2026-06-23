using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// The moddable form of a faction — a plain record keyed by string id, so mods can add factions the built-in
    /// <see cref="FactionId"/> enum never knew about. Built-ins are bridged in via <see cref="FromProfile"/>.
    /// </summary>
    public sealed class FactionDef : IContentDef
    {
        public string Id { get; }
        public readonly string Name;
        public readonly string Creed;
        public readonly string Strength;
        public readonly string Weakness;
        public readonly float OffenseMultiplier;
        public readonly float DefenseMultiplier;
        public readonly Doctrine OnConfluence;
        public readonly Doctrine OnMixedGifts;

        public FactionDef(string id, string name, string creed, string strength, string weakness,
            float offenseMultiplier, float defenseMultiplier, Doctrine onConfluence, Doctrine onMixedGifts)
        {
            Id = id;
            Name = string.IsNullOrEmpty(name) ? id : name;
            Creed = creed ?? "";
            Strength = strength ?? "";
            Weakness = weakness ?? "";
            OffenseMultiplier = offenseMultiplier;
            DefenseMultiplier = defenseMultiplier;
            OnConfluence = onConfluence;
            OnMixedGifts = onMixedGifts;
        }

        public FactionPerk Perk => new FactionPerk(OffenseMultiplier, DefenseMultiplier);

        /// <summary>How a member of this faction regards a person with the given traits (derives from doctrine).</summary>
        public Attitude AttitudeToward(bool isConfluence, bool hasMixedGift) =>
            FactionAttitudes.FromDoctrine(OnConfluence, OnMixedGifts, isConfluence, hasMixedGift);

        public static FactionDef FromProfile(FactionProfile p) =>
            new FactionDef(p.Id.ToString(), p.Name, p.Creed, p.Strength, p.Weakness,
                p.Perk.OffenseMultiplier, p.Perk.DefenseMultiplier, p.OnConfluence, p.OnMixedGifts);
    }

    /// <summary>
    /// The moddable superset of factions. Seeds itself from the built-in <see cref="FactionCatalog"/> on first
    /// use, then mods register more through <see cref="Register"/> (see <c>ModLoader</c>). A join menu can list
    /// <see cref="All"/> to show built-in and modded factions side by side.
    /// </summary>
    public static class FactionRegistry
    {
        private static readonly ContentRegistry<FactionDef> _reg = new ContentRegistry<FactionDef>();
        private static bool _seeded;

        public static void EnsureSeeded()
        {
            if (_seeded) return;
            _seeded = true;
            foreach (var id in FactionCatalog.Joinable)
                _reg.Register(FactionDef.FromProfile(FactionCatalog.For(id)));
        }

        public static void Register(FactionDef def) { EnsureSeeded(); _reg.Register(def); }
        public static FactionDef Get(string id) { EnsureSeeded(); return _reg.Get(id); }
        public static bool TryGet(string id, out FactionDef def) { EnsureSeeded(); return _reg.TryGet(id, out def); }
        public static IEnumerable<FactionDef> All { get { EnsureSeeded(); return _reg.All; } }
        public static int Count { get { EnsureSeeded(); return _reg.Count; } }
    }
}
