using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class BodyAnimationTests
    {
        [Test]
        public void Bob_StaysWithinAmplitude()
        {
            for (float t = 0f; t < 12f; t += 0.13f)
                Assert.LessOrEqual(System.Math.Abs(BodyAnimation.Bob(t, 0.06f, 3.2f)), 0.06f + 1e-4f);
        }

        [Test]
        public void Sway_StaysWithinAmplitude()
        {
            for (float t = 0f; t < 12f; t += 0.13f)
                Assert.LessOrEqual(System.Math.Abs(BodyAnimation.Sway(t, 3f, 3.2f)), 3f + 1e-4f);
        }

        [Test]
        public void LungeCurve_ZeroAtEnds_PeaksMid()
        {
            Assert.AreEqual(0f, BodyAnimation.LungeCurve(0f), 1e-6f);
            Assert.AreEqual(0f, BodyAnimation.LungeCurve(1f), 1e-6f);
            Assert.AreEqual(1f, BodyAnimation.LungeCurve(0.35f), 1e-3f);
        }

        [Test]
        public void LungeCurve_InteriorWithinUnit()
        {
            for (float t = 0.01f; t < 1f; t += 0.05f)
            {
                float v = BodyAnimation.LungeCurve(t);
                Assert.Greater(v, 0f);
                Assert.LessOrEqual(v, 1f + 1e-4f);
            }
        }
    }
}
