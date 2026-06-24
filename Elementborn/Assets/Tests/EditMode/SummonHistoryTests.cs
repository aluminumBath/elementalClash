using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SummonHistoryTests
    {
        private static SummonHistoryEntry Entry(CreatureKind k, SummonRarity r, long ticks, bool featured = false) =>
            new SummonHistoryEntry(k, r, featured, "Wild Beacon", ticks);

        [Test]
        public void RecordIsNewestFirst()
        {
            var h = new SummonHistory(8);
            h.Record(Entry(CreatureKind.Phoenix, SummonRarity.Legendary, 1));
            h.Record(Entry(CreatureKind.Roc, SummonRarity.Epic, 2));

            Assert.AreEqual(2, h.Count);
            Assert.AreEqual(CreatureKind.Roc, h.Recent[0].Kind, "most recent first");
            Assert.AreEqual(CreatureKind.Phoenix, h.Recent[1].Kind);
        }

        [Test]
        public void CapacityTrimsTheOldest()
        {
            var h = new SummonHistory(3);
            for (int i = 1; i <= 5; i++) h.Record(Entry(CreatureKind.Roc, SummonRarity.Epic, i));

            Assert.AreEqual(3, h.Count);
            // Newest five recorded were ticks 1..5; only 5,4,3 should survive (newest-first).
            Assert.AreEqual(new long[] { 5, 4, 3 }, h.Recent.Select(e => e.UtcTicks).ToArray());
        }

        [Test]
        public void EntryFieldsArePreserved()
        {
            var h = new SummonHistory();
            h.Record(new SummonHistoryEntry(CreatureKind.Phoenix, SummonRarity.Legendary, true, "Flamecaller Beacon", 999));
            var e = h.Recent[0];
            Assert.AreEqual(CreatureKind.Phoenix, e.Kind);
            Assert.AreEqual(SummonRarity.Legendary, e.Rarity);
            Assert.IsTrue(e.WonFeatured);
            Assert.AreEqual("Flamecaller Beacon", e.BannerName);
            Assert.AreEqual(999, e.UtcTicks);
        }

        [Test]
        public void LoadFromCapsAtCapacityAndPreservesOrder()
        {
            var h = new SummonHistory(2);
            h.LoadFrom(new List<SummonHistoryEntry>
            {
                Entry(CreatureKind.Phoenix, SummonRarity.Legendary, 10),
                Entry(CreatureKind.Roc, SummonRarity.Epic, 9),
                Entry(CreatureKind.IceCat, SummonRarity.Epic, 8),
            });
            Assert.AreEqual(2, h.Count);
            Assert.AreEqual(new long[] { 10, 9 }, h.Recent.Select(e => e.UtcTicks).ToArray());
        }

        [Test]
        public void CapacityIsAtLeastOne()
        {
            var h = new SummonHistory(0);
            Assert.AreEqual(1, h.Capacity);
            h.Record(Entry(CreatureKind.Roc, SummonRarity.Epic, 1));
            h.Record(Entry(CreatureKind.Phoenix, SummonRarity.Legendary, 2));
            Assert.AreEqual(1, h.Count);
            Assert.AreEqual(CreatureKind.Phoenix, h.Recent[0].Kind);
        }

        [Test]
        public void ClearEmptiesTheLog()
        {
            var h = new SummonHistory();
            h.Record(Entry(CreatureKind.Roc, SummonRarity.Epic, 1));
            h.Clear();
            Assert.AreEqual(0, h.Count);
        }
    }
}
