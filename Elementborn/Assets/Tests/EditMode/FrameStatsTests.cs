using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class FrameStatsTests
    {
        [Test]
        public void Empty_HasNoData()
        {
            var s = new FrameStats(10);
            Assert.IsFalse(s.HasData);
            Assert.AreEqual(0, s.Samples);
            Assert.AreEqual(0.0, s.Fps, 1e-9);
        }

        [Test]
        public void Average_Min_Max_OverPushes()
        {
            var s = new FrameStats(10);
            s.Push(10.0); s.Push(20.0); s.Push(30.0);
            Assert.AreEqual(3, s.Samples);
            Assert.AreEqual(20.0, s.AverageMs, 1e-9);
            Assert.AreEqual(10.0, s.MinMs, 1e-9);
            Assert.AreEqual(30.0, s.MaxMs, 1e-9);
        }

        [Test]
        public void Fps_IsInverseOfAverage()
        {
            var s = new FrameStats(10);
            s.Push(10.0); // 10 ms -> 100 fps
            Assert.AreEqual(100.0, s.Fps, 1e-6);
        }

        [Test]
        public void Window_CapsSamples_AndDropsOldest()
        {
            var s = new FrameStats(3);
            s.Push(100.0); // oldest, should fall out
            s.Push(10.0); s.Push(10.0); s.Push(10.0);
            Assert.AreEqual(3, s.Samples);
            Assert.AreEqual(10.0, s.AverageMs, 1e-9); // the 100 ms sample evicted
            Assert.AreEqual(10.0, s.MaxMs, 1e-9);
        }

        [Test]
        public void Clear_Resets()
        {
            var s = new FrameStats(5);
            s.Push(16.0);
            s.Clear();
            Assert.IsFalse(s.HasData);
            Assert.AreEqual(0.0, s.AverageMs, 1e-9);
        }
    }
}
