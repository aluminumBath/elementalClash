using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>One notable summon, kept for the Beacon's recent-pulls log: the creature, its tier, whether it was
    /// a featured win, the banner it came from, and when (UTC ticks, supplied by the caller so this stays pure).</summary>
    public readonly struct SummonHistoryEntry
    {
        public readonly CreatureKind Kind;
        public readonly SummonRarity Rarity;
        public readonly bool WonFeatured;
        public readonly string BannerName;
        public readonly long UtcTicks;

        public SummonHistoryEntry(CreatureKind kind, SummonRarity rarity, bool wonFeatured, string bannerName, long utcTicks)
        {
            Kind = kind;
            Rarity = rarity;
            WonFeatured = wonFeatured;
            BannerName = bannerName ?? "";
            UtcTicks = utcTicks;
        }
    }

    /// <summary>
    /// A small, newest-first ring buffer of notable summons (the controller records Epics and Legendaries). Pure
    /// and bounded — no Unity, no clock — so it's unit-tested; the <see cref="SummonController"/> feeds it and
    /// persists it. Drives the "Recent pulls" readout.
    /// </summary>
    public sealed class SummonHistory
    {
        public const int DefaultCapacity = 8;

        private readonly List<SummonHistoryEntry> _entries = new List<SummonHistoryEntry>();
        private readonly int _capacity;

        public SummonHistory(int capacity = DefaultCapacity) { _capacity = capacity < 1 ? 1 : capacity; }

        public int Count => _entries.Count;
        public int Capacity => _capacity;

        /// <summary>Newest first.</summary>
        public IReadOnlyList<SummonHistoryEntry> Recent => _entries;

        /// <summary>Add a fresh entry to the front, dropping the oldest beyond capacity.</summary>
        public void Record(SummonHistoryEntry e)
        {
            _entries.Insert(0, e);
            if (_entries.Count > _capacity) _entries.RemoveRange(_capacity, _entries.Count - _capacity);
        }

        /// <summary>Replace the log from a save (expects newest-first; capped at capacity).</summary>
        public void LoadFrom(IEnumerable<SummonHistoryEntry> newestFirst)
        {
            _entries.Clear();
            if (newestFirst == null) return;
            foreach (var e in newestFirst)
            {
                if (_entries.Count >= _capacity) break;
                _entries.Add(e);
            }
        }

        public void Clear() => _entries.Clear();
    }
}
