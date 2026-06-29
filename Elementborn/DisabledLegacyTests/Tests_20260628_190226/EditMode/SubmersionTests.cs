using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class SubmersionTests
    {
        [Test]
        public void AboveSurface_DepthIsZero()
        {
            Assert.AreEqual(0f, Submersion.Depth(14.4f, 20f), 1e-4f);
            Assert.IsFalse(Submersion.IsSubmerged(14.4f, 20f));
        }

        [Test]
        public void AtSurface_DepthIsZero_NotSubmerged()
        {
            Assert.AreEqual(0f, Submersion.Depth(14.4f, 14.4f), 1e-4f);
            Assert.IsFalse(Submersion.IsSubmerged(14.4f, 14.4f));
        }

        [Test]
        public void BelowSurface_DepthIsDistanceDown()
        {
            Assert.AreEqual(4.4f, Submersion.Depth(14.4f, 10f), 1e-4f);
            Assert.IsTrue(Submersion.IsSubmerged(14.4f, 10f));
        }

        [Test]
        public void DeepChamber_ReportsLargeDepth()
        {
            // The flooded Sunken Gate: a surface set far overhead yields crushing depth.
            Assert.AreEqual(99f, Submersion.Depth(100f, 1f), 1e-4f);
            Assert.Greater(Submersion.Depth(100f, 1f), 8f); // past the pressure threshold
        }

        [Test]
        public void DepthNeverGoesNegative()
        {
            Assert.GreaterOrEqual(Submersion.Depth(0f, 5f), 0f);
            Assert.GreaterOrEqual(Submersion.Depth(-3f, 100f), 0f);
        }
    }
}
