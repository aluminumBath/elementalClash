using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class HealthBarStateTests
    {
        [Test]
        public void FractionClampsAndHandlesBadMax()
        {
            Assert.AreEqual(1f, HealthBarState.Fraction(100f, 100f), 1e-5f);
            Assert.AreEqual(0.5f, HealthBarState.Fraction(50f, 100f), 1e-5f);
            Assert.AreEqual(0f, HealthBarState.Fraction(0f, 100f), 1e-5f);
            Assert.AreEqual(0f, HealthBarState.Fraction(-10f, 100f), 1e-5f);
            Assert.AreEqual(1f, HealthBarState.Fraction(200f, 100f), 1e-5f);
            Assert.AreEqual(0f, HealthBarState.Fraction(50f, 0f), 1e-5f);  // bad max → empty
        }

        [Test]
        public void AlphaIsFullDuringHold()
        {
            Assert.AreEqual(1f, HealthBarState.Alpha(0f, 2.5f, 0.5f), 1e-5f);
            Assert.AreEqual(1f, HealthBarState.Alpha(2.5f, 2.5f, 0.5f), 1e-5f);
        }

        [Test]
        public void AlphaRampsToZeroAfterHold()
        {
            Assert.AreEqual(0.5f, HealthBarState.Alpha(2.75f, 2.5f, 0.5f), 1e-4f); // halfway through fade
            Assert.AreEqual(0f, HealthBarState.Alpha(3.0f, 2.5f, 0.5f), 1e-5f);
            Assert.AreEqual(0f, HealthBarState.Alpha(99f, 2.5f, 0.5f), 1e-5f);     // long past
        }

        [Test]
        public void AlphaZeroFadeHidesImmediatelyAfterHold()
        {
            Assert.AreEqual(0f, HealthBarState.Alpha(2.6f, 2.5f, 0f), 1e-5f);
        }
    }
}
