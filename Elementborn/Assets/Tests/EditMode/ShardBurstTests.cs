using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ShardBurstTests
    {
        [Test]
        public void DirectionsAreUnitLength()
        {
            for (int i = 0; i < 8; i++)
            {
                ShardBurst.Direction(i, 8, 0.6f, out float x, out float y, out float z);
                float len = Mathf.Sqrt(x * x + y * y + z * z);
                Assert.AreEqual(1f, len, 1e-4f);
            }
        }

        [Test]
        public void UpBiasGivesPositiveY()
        {
            ShardBurst.Direction(0, 6, 0.6f, out _, out float y, out _);
            Assert.Greater(y, 0f);
        }

        [Test]
        public void NonPositiveUpBiasIsFlat()
        {
            ShardBurst.Direction(2, 6, -3f, out _, out float y, out _);
            Assert.AreEqual(0f, y, 1e-5f);
        }

        [Test]
        public void DirectionsSpreadAroundTheRing()
        {
            ShardBurst.Direction(0, 6, 0f, out float x0, out _, out float z0);
            ShardBurst.Direction(3, 6, 0f, out float x3, out _, out float z3);
            // opposite sides of the ring should point roughly opposite on the x/z plane
            Assert.Less(x0 * x3 + z0 * z3, 0.5f);
        }

        [Test]
        public void IsDeterministic()
        {
            ShardBurst.Direction(4, 7, 0.6f, out float ax, out float ay, out float az);
            ShardBurst.Direction(4, 7, 0.6f, out float bx, out float by, out float bz);
            Assert.AreEqual(ax, bx, 1e-6f);
            Assert.AreEqual(ay, by, 1e-6f);
            Assert.AreEqual(az, bz, 1e-6f);
        }

        [Test]
        public void ZeroCountIsSafe()
        {
            ShardBurst.Direction(0, 0, 0.6f, out float x, out float y, out float z);
            float len = Mathf.Sqrt(x * x + y * y + z * z);
            Assert.AreEqual(1f, len, 1e-4f);
        }
    }
}
