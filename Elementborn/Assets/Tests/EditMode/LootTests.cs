using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class LootTests
    {
        // Scripted RNG: returns queued unit values in order, then 0.
        private sealed class ScriptedRandom : IRandomSource
        {
            private readonly Queue<double> _q;
            public ScriptedRandom(params double[] values) => _q = new Queue<double>(values);
            public double NextUnit() => _q.Count > 0 ? _q.Dequeue() : 0.0;
        }

        [Test]
        public void SingleEntryAlwaysDropsIt()
        {
            var t = new LootTable(1, new LootEntry("hide", 1, 1, 1));
            var drops = t.Roll(new ScriptedRandom(0.0, 0.0));
            Assert.AreEqual(1, drops.Count);
            Assert.AreEqual("hide", drops[0].ItemId);
            Assert.AreEqual(1, drops[0].Count);
        }

        [Test]
        public void WeightSelectionPicksByCumulativeWeight()
        {
            var t = new LootTable(1,
                new LootEntry("a", 1, 1, 1),
                new LootEntry("b", 3, 1, 1)); // total weight 4
            // pick r = 0.1*4 = 0.4 < cum(a)=1 -> a
            Assert.AreEqual("a", t.Roll(new ScriptedRandom(0.1, 0.0))[0].ItemId);
            // pick r = 0.5*4 = 2.0 -> past a(1), within b(4) -> b
            Assert.AreEqual("b", t.Roll(new ScriptedRandom(0.5, 0.0))[0].ItemId);
        }

        [Test]
        public void QuantityStaysWithinRange()
        {
            var t = new LootTable(1, new LootEntry("hide", 1, 2, 5));
            Assert.AreEqual(5, t.Roll(new ScriptedRandom(0.0, 0.999))[0].Count); // top of range
            Assert.AreEqual(2, t.Roll(new ScriptedRandom(0.0, 0.0))[0].Count);   // bottom of range
        }

        [Test]
        public void NothingSlotCanYieldNoDrop()
        {
            var t = new LootTable(1, new LootEntry("", 1, 1, 1)); // only a nothing slot
            Assert.AreEqual(0, t.Roll(new ScriptedRandom(0.0)).Count);
        }

        [Test]
        public void IdenticalItemsStackAcrossRolls()
        {
            var t = new LootTable(2, new LootEntry("hide", 1, 1, 1));
            var drops = t.Roll(new ScriptedRandom(0.0, 0.0, 0.0, 0.0)); // two rolls, hide x1 each
            Assert.AreEqual(1, drops.Count);
            Assert.AreEqual("hide", drops[0].ItemId);
            Assert.AreEqual(2, drops[0].Count);
        }

        [Test]
        public void SameSeedIsDeterministic()
        {
            var t = LootTables.For(CreatureKind.FireDragon);
            var a = t.Roll(new SystemRandomSource(123)).Select(d => d.ItemId + ":" + d.Count).ToList();
            var b = t.Roll(new SystemRandomSource(123)).Select(d => d.ItemId + ":" + d.Count).ToList();
            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void EveryCreatureKindHasATableOfValidItems()
        {
            foreach (CreatureKind k in Enum.GetValues(typeof(CreatureKind)))
            {
                var table = LootTables.For(k);
                Assert.IsNotNull(table, k + " has no loot table");
                foreach (var e in table.Entries)
                    Assert.IsTrue(e.ItemId == "" || ItemCatalog.Exists(e.ItemId),
                        k + ": loot item '" + e.ItemId + "' must exist in ItemCatalog");
            }
        }
    }
}
