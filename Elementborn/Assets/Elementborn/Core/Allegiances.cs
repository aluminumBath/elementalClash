namespace Elementborn.Core
{
    /// <summary>The joinable social factions (distinct from the combat <see cref="Faction"/> allegiance).</summary>
    public enum FactionId { None, Symbiasts, Separatists, Cleicists, Synodists }

    /// <summary>A faction's doctrinal view of something, strongest to weakest approval.</summary>
    public enum Doctrine { Reveres, Accepts, Dislikes, Abhors }

    /// <summary>How a faction member regards a person, on a spectrum.</summary>
    public enum Attitude { Revered, Friendly, Neutral, Unfriendly, Hostile }

    /// <summary>Passive bonuses a member gains. Above 1 is a strength, below 1 a weakness.</summary>
    public readonly struct FactionPerk
    {
        public readonly float OffenseMultiplier;
        public readonly float DefenseMultiplier;

        public FactionPerk(float offense, float defense)
        {
            OffenseMultiplier = offense;
            DefenseMultiplier = defense;
        }

        public static FactionPerk None => new FactionPerk(1f, 1f);
    }

    /// <summary>Everything that defines a faction: its creed, what it's good and bad at, its passive perk,
    /// and how it regards the Confluence (all elements in one person) and mixed gifts (lava/blood/metal/flight).</summary>
    public readonly struct FactionProfile
    {
        public readonly FactionId Id;
        public readonly string Name;
        public readonly string Creed;
        public readonly string Strength;
        public readonly string Weakness;
        public readonly FactionPerk Perk;
        public readonly Doctrine OnConfluence;
        public readonly Doctrine OnMixedGifts;

        public FactionProfile(FactionId id, string name, string creed, string strength, string weakness,
            FactionPerk perk, Doctrine onConfluence, Doctrine onMixedGifts)
        {
            Id = id;
            Name = name;
            Creed = creed;
            Strength = strength;
            Weakness = weakness;
            Perk = perk;
            OnConfluence = onConfluence;
            OnMixedGifts = onMixedGifts;
        }
    }

    /// <summary>Faction data. Tweak creeds, perks, and stances here; add new factions by extending the switch.</summary>
    public static class FactionCatalog
    {
        public static readonly FactionId[] Joinable =
            { FactionId.Symbiasts, FactionId.Separatists, FactionId.Cleicists, FactionId.Synodists };

        public static FactionProfile For(FactionId id)
        {
            switch (id)
            {
                case FactionId.Symbiasts:
                    return new FactionProfile(id, "Symbiasts",
                        "All elements live in equality and harmony, in peace; the convergence of elements in one soul is holy.",
                        "Resilient and balanced — harmony lends staying power.",
                        "Slow to anger, with little raw offense.",
                        new FactionPerk(1.05f, 1.15f), Doctrine.Reveres, Doctrine.Accepts);

                case FactionId.Separatists:
                    return new FactionProfile(id, "Separatists",
                        "Each in their place — element users kept apart from non-users, and from each other by element. 'Safety,' they say; mostly it's envy.",
                        "Aggressive and hard-hitting.",
                        "Brittle — they trade defense for force.",
                        new FactionPerk(1.20f, 0.92f), Doctrine.Abhors, Doctrine.Dislikes);

                case FactionId.Cleicists:
                    return new FactionProfile(id, "Cleicists",
                        "All elements together in harmony — yet any convergence of elements is an abomination.",
                        "Stalwart defenders.",
                        "Dogmatic, and uneasy around mixed gifts.",
                        new FactionPerk(1.0f, 1.22f), Doctrine.Abhors, Doctrine.Dislikes);

                case FactionId.Synodists:
                    return new FactionProfile(id, "Synodists",
                        "Every voice counts, weighted by population; the synod governs by those numbers.",
                        "Adaptable and well-supported.",
                        "Consensus is slow, with no single specialty.",
                        new FactionPerk(1.10f, 1.05f), Doctrine.Accepts, Doctrine.Accepts);

                default:
                    return new FactionProfile(FactionId.None, "Unaligned", "No allegiance.", "—", "—",
                        FactionPerk.None, Doctrine.Accepts, Doctrine.Accepts);
            }
        }
    }

    /// <summary>
    /// How a faction member regards someone with the given traits — pure, so NPC AI and dialogue can read it
    /// consistently. Symbiasts revere the Confluence; Cleicists abhor it and distrust mixed gifts though they
    /// otherwise keep the peace; Separatists are cold to outsiders and hostile to convergence; Synodists
    /// judge by other things.
    /// </summary>
    public static class FactionAttitudes
    {
        public static Attitude Toward(FactionId viewer, bool targetIsConfluence, bool targetHasMixedGift)
        {
            switch (viewer)
            {
                case FactionId.Symbiasts:
                    return targetIsConfluence ? Attitude.Revered : Attitude.Friendly;

                case FactionId.Cleicists:
                    if (targetIsConfluence) return Attitude.Hostile;
                    return targetHasMixedGift ? Attitude.Unfriendly : Attitude.Friendly;

                case FactionId.Separatists:
                    if (targetIsConfluence) return Attitude.Hostile;
                    return targetHasMixedGift ? Attitude.Unfriendly : Attitude.Neutral;

                case FactionId.Synodists:
                    return Attitude.Neutral;

                default:
                    return Attitude.Neutral;
            }
        }

        /// <summary>Generic doctrine-driven attitude, used for modded factions: a member reacts to a Confluence
        /// or to mixed gifts per the faction's declared doctrine, and is Neutral toward ordinary people.</summary>
        public static Attitude FromDoctrine(Doctrine onConfluence, Doctrine onMixedGifts,
            bool targetIsConfluence, bool targetHasMixedGift)
        {
            if (targetIsConfluence) return FromDoctrine(onConfluence);
            if (targetHasMixedGift) return FromDoctrine(onMixedGifts);
            return Attitude.Neutral;
        }

        private static Attitude FromDoctrine(Doctrine d)
        {
            switch (d)
            {
                case Doctrine.Reveres: return Attitude.Revered;
                case Doctrine.Accepts: return Attitude.Friendly;
                case Doctrine.Dislikes: return Attitude.Unfriendly;
                case Doctrine.Abhors: return Attitude.Hostile;
                default: return Attitude.Neutral;
            }
        }
    }
}
