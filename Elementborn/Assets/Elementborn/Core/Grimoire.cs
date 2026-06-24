using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    public enum GrimoireSection { Bestiary, Attacks, Bloodlines }

    /// <summary>How fully an entry has been discovered. Higher tiers reveal more of it.</summary>
    public enum DiscoveryTier { Unknown = 0, Glimpsed = 1, Known = 2, Mastered = 3 }

    /// <summary>The full (unredacted) text of a grimoire entry, layered by reveal tier.</summary>
    public readonly struct GrimoireEntry
    {
        public readonly GrimoireSection Section;
        public readonly string Id;
        public readonly string Name;     // shown from Glimpsed up
        public readonly string Glimpse;  // revealed at Glimpsed
        public readonly string Detail;   // revealed at Known
        public readonly string Mastery;  // revealed at Mastered (deepest)

        public GrimoireEntry(GrimoireSection section, string id, string name, string glimpse, string detail, string mastery)
        {
            Section = section; Id = id; Name = name; Glimpse = glimpse; Detail = detail; Mastery = mastery;
        }
    }

    /// <summary>What the player actually sees for an entry, after redaction by their discovery tier.</summary>
    public readonly struct RedactedEntry
    {
        public readonly GrimoireSection Section;
        public readonly string Id;
        public readonly DiscoveryTier Tier;
        public readonly string Name;                  // "???" while Unknown
        public readonly IReadOnlyList<string> Lines;  // revealed lines (empty while Unknown)

        public RedactedEntry(GrimoireSection section, string id, DiscoveryTier tier, string name, IReadOnlyList<string> lines)
        {
            Section = section; Id = id; Tier = tier; Name = name; Lines = lines;
        }

        public bool IsLocked => Tier == DiscoveryTier.Unknown;
    }

    /// <summary>Redaction: reveal an entry only to the degree a tier allows.</summary>
    public static class Grimoire
    {
        public const string Hidden = "???";

        public static RedactedEntry Redact(GrimoireEntry e, DiscoveryTier tier)
        {
            if (tier <= DiscoveryTier.Unknown)
                return new RedactedEntry(e.Section, e.Id, DiscoveryTier.Unknown, Hidden, new List<string>());

            var lines = new List<string>();
            if (tier >= DiscoveryTier.Glimpsed && !string.IsNullOrEmpty(e.Glimpse)) lines.Add(e.Glimpse);
            if (tier >= DiscoveryTier.Known && !string.IsNullOrEmpty(e.Detail)) lines.Add(e.Detail);
            if (tier >= DiscoveryTier.Mastered && !string.IsNullOrEmpty(e.Mastery)) lines.Add(e.Mastery);
            return new RedactedEntry(e.Section, e.Id, tier, e.Name, lines);
        }
    }

    /// <summary>
    /// The player's grimoire discovery state — which entries they've uncovered and how fully. Mutable and savable;
    /// entries fill in as the player first does the thing (sights/defeats/tames a creature, casts an attack, meets
    /// a bloodline). A tier never downgrades.
    /// </summary>
    public sealed class GrimoireProgress
    {
        private readonly Dictionary<string, DiscoveryTier> _tiers = new Dictionary<string, DiscoveryTier>();

        private static string Key(GrimoireSection s, string id) => s + ":" + id;

        public DiscoveryTier TierOf(GrimoireSection section, string id) =>
            _tiers.TryGetValue(Key(section, id), out var t) ? t : DiscoveryTier.Unknown;

        public bool IsDiscovered(GrimoireSection section, string id) => TierOf(section, id) > DiscoveryTier.Unknown;

        /// <summary>Advance an entry to at least <paramref name="tier"/> (never downgrades). True if it changed.</summary>
        public bool Discover(GrimoireSection section, string id, DiscoveryTier tier)
        {
            if (string.IsNullOrEmpty(id) || tier <= DiscoveryTier.Unknown) return false;
            string key = Key(section, id);
            DiscoveryTier cur = _tiers.TryGetValue(key, out var t) ? t : DiscoveryTier.Unknown;
            if (tier <= cur) return false;
            _tiers[key] = tier;
            return true;
        }

        public int CountAtLeast(GrimoireSection section, DiscoveryTier tier)
        {
            string prefix = section + ":";
            int n = 0;
            foreach (var kv in _tiers)
                if (kv.Key.StartsWith(prefix, StringComparison.Ordinal) && kv.Value >= tier) n++;
            return n;
        }

        public int CountDiscovered(GrimoireSection section) => CountAtLeast(section, DiscoveryTier.Glimpsed);

        // --- discovery events (called the first time the player does the thing) ---
        public bool RecordSighting(CreatureKind kind) => Discover(GrimoireSection.Bestiary, kind.ToString(), DiscoveryTier.Glimpsed);
        public bool RecordDefeat(CreatureKind kind)   => Discover(GrimoireSection.Bestiary, kind.ToString(), DiscoveryTier.Known);
        public bool RecordTame(CreatureKind kind)     => Discover(GrimoireSection.Bestiary, kind.ToString(), DiscoveryTier.Mastered);
        public bool RecordCast(Element element, IntentType intent) => Discover(GrimoireSection.Attacks, GrimoireCatalog.AttackId(element, intent), DiscoveryTier.Known);
        public bool RecordBloodlineSeen(BloodlineId bloodline)    => Discover(GrimoireSection.Bloodlines, bloodline.ToString(), DiscoveryTier.Glimpsed);
        public bool RecordBloodlineStudied(BloodlineId bloodline) => Discover(GrimoireSection.Bloodlines, bloodline.ToString(), DiscoveryTier.Mastered);

        // --- persistence ---
        public IReadOnlyDictionary<string, int> ToSave()
        {
            var d = new Dictionary<string, int>();
            foreach (var kv in _tiers) d[kv.Key] = (int)kv.Value;
            return d;
        }

        public static GrimoireProgress LoadFrom(IReadOnlyDictionary<string, int> data)
        {
            var p = new GrimoireProgress();
            if (data != null)
                foreach (var kv in data) p._tiers[kv.Key] = (DiscoveryTier)kv.Value;
            return p;
        }
    }

    /// <summary>
    /// Assembles each grimoire section's entries, composed from existing data: the Bestiary from
    /// <see cref="CreatureCatalog"/> + <see cref="CreatureHints"/>, Attacks from each element × the moveset, and
    /// Bloodlines from <see cref="Bloodlines"/>. The progress decides how much of each is revealed.
    /// </summary>
    public static class GrimoireCatalog
    {
        public static readonly IntentType[] MovesetIntents =
        {
            IntentType.PrimaryCast, IntentType.SecondaryCast, IntentType.Defend,
            IntentType.Heavy, IntentType.Sweep, IntentType.Signature
        };

        private static readonly Element[] Elements = { Element.Fire, Element.Water, Element.Earth, Element.Air };

        public static string AttackId(Element element, IntentType intent) => element + "/" + intent;

        public static IReadOnlyList<GrimoireEntry> ForSection(GrimoireSection section)
        {
            switch (section)
            {
                case GrimoireSection.Bestiary: return Bestiary();
                case GrimoireSection.Attacks: return Attacks();
                case GrimoireSection.Bloodlines: return BloodlineEntries();
                default: return new List<GrimoireEntry>();
            }
        }

        public static IReadOnlyList<GrimoireEntry> Bestiary()
        {
            var list = new List<GrimoireEntry>();
            foreach (CreatureKind kind in (CreatureKind[])Enum.GetValues(typeof(CreatureKind)))
            {
                var info = CreatureCatalog.For(kind);
                string role = info.IsCompanion ? "A rare companion." : info.Rideable ? "Large enough to ride." : "Wildlife.";
                string element = info.Element.HasValue ? info.Element.Value + "-natured. " : "";
                int tamePct = (int)(info.TameChance * 100f + 0.5f);
                list.Add(new GrimoireEntry(GrimoireSection.Bestiary, kind.ToString(),
                    info.Name,
                    element + role,
                    CreatureHints.WhereToFind(kind) + " Tame it with a " + info.Name + " lure (markets, shrines, camps).",
                    "Tame chance about " + tamePct + "% once weakened and lured."));
            }
            return list;
        }

        public static IReadOnlyList<GrimoireEntry> Attacks()
        {
            var list = new List<GrimoireEntry>();
            foreach (var e in Elements)
                foreach (var intent in MovesetIntents)
                    list.Add(new GrimoireEntry(GrimoireSection.Attacks, AttackId(e, intent),
                        e + " " + Readable(intent),
                        Shape(intent),
                        e + " — " + Effect(e),
                        ""));
            return list;
        }

        public static IReadOnlyList<GrimoireEntry> BloodlineEntries()
        {
            var list = new List<GrimoireEntry>();
            foreach (var id in Bloodlines.AllIds)
            {
                var b = Bloodlines.For(id);
                list.Add(new GrimoireEntry(GrimoireSection.Bloodlines, id.ToString(),
                    b.Name,
                    string.Join(", ", ElementNames(b.Elements)),
                    b.Trait,
                    string.IsNullOrEmpty(b.Notable) ? "" : "Known bearer: " + b.Notable));
            }
            return list;
        }

        private static string Readable(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.PrimaryCast: return "Cast";
                case IntentType.SecondaryCast: return "Sub-art";
                case IntentType.Defend: return "Guard";
                case IntentType.Heavy: return "Heavy";
                case IntentType.Sweep: return "Sweep";
                case IntentType.Signature: return "Signature";
                default: return intent.ToString();
            }
        }

        private static string Shape(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.PrimaryCast: return "A single ranged bolt.";
                case IntentType.SecondaryCast: return "The charged sub-art variant.";
                case IntentType.Defend: return "A brief defensive shield.";
                case IntentType.Heavy: return "A committed impact zone that arcs in and lands.";
                case IntentType.Sweep: return "A wide, multi-target arc up close.";
                case IntentType.Signature: return "A hidden, element-specific gesture move.";
                default: return "";
            }
        }

        private static string Effect(Element e)
        {
            switch (e)
            {
                case Element.Fire: return "burns over time.";
                case Element.Water: return "chills and slows.";
                case Element.Earth: return "staggers and knocks back.";
                case Element.Air: return "launches with knockback.";
                default: return "";
            }
        }

        private static IEnumerable<string> ElementNames(IReadOnlyList<Element> elements)
        {
            var names = new List<string>();
            foreach (var e in elements) names.Add(e.ToString());
            return names;
        }
    }
}
