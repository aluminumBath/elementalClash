using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class LogRingTests
    {
        [Test]
        public void AddsAndReadsOldestFirst()
        {
            var ring = new LogRing(4);
            ring.Add("a", LogSeverity.Info);
            ring.Add("b", LogSeverity.Warning);
            Assert.AreEqual(2, ring.Count);
            Assert.AreEqual("a", ring.GetLine(0));
            Assert.AreEqual("b", ring.GetLine(1));
            Assert.AreEqual(LogSeverity.Warning, ring.GetSeverity(1));
        }

        [Test]
        public void OverwritesOldestWhenFull()
        {
            var ring = new LogRing(3);
            ring.Add("1", LogSeverity.Info);
            ring.Add("2", LogSeverity.Info);
            ring.Add("3", LogSeverity.Info);
            ring.Add("4", LogSeverity.Error); // pushes out "1"
            Assert.AreEqual(3, ring.Count);
            Assert.AreEqual("2", ring.GetLine(0));
            Assert.AreEqual("4", ring.GetLine(2));
            Assert.AreEqual(LogSeverity.Error, ring.GetSeverity(2));
        }

        [Test]
        public void CountNeverExceedsCapacity()
        {
            var ring = new LogRing(2);
            for (int i = 0; i < 50; i++) ring.Add("x" + i, LogSeverity.Info);
            Assert.AreEqual(2, ring.Count);
            Assert.AreEqual("x48", ring.GetLine(0));
            Assert.AreEqual("x49", ring.GetLine(1));
        }

        [Test]
        public void OutOfRangeAndClearAreSafe()
        {
            var ring = new LogRing(4);
            ring.Add("only", LogSeverity.Info);
            Assert.IsNull(ring.GetLine(5));
            Assert.IsNull(ring.GetLine(-1));
            ring.Clear();
            Assert.AreEqual(0, ring.Count);
            Assert.IsNull(ring.GetLine(0));
        }

        [Test]
        public void CapacityIsAtLeastOne()
        {
            var ring = new LogRing(0);
            Assert.GreaterOrEqual(ring.Capacity, 1);
            ring.Add("safe", LogSeverity.Info);
            Assert.AreEqual("safe", ring.GetLine(0));
        }
    }
}
