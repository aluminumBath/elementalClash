using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class HitFeedbackTests
    {
        [Test]
        public void Intensity01ClampsToUnitRange()
        {
            Assert.AreEqual(0f, HitFeedback.Intensity01(0f, 40f), 1e-5f);
            Assert.AreEqual(0.5f, HitFeedback.Intensity01(20f, 40f), 1e-5f);
            Assert.AreEqual(1f, HitFeedback.Intensity01(40f, 40f), 1e-5f);
            Assert.AreEqual(1f, HitFeedback.Intensity01(999f, 40f), 1e-5f); // saturates
            Assert.AreEqual(0f, HitFeedback.Intensity01(-5f, 40f), 1e-5f);  // negative floored
        }

        [Test]
        public void Intensity01HandlesNonPositiveReference()
        {
            Assert.AreEqual(1f, HitFeedback.Intensity01(5f, 0f), 1e-5f);
            Assert.AreEqual(0f, HitFeedback.Intensity01(0f, 0f), 1e-5f);
            Assert.AreEqual(1f, HitFeedback.Intensity01(5f, -10f), 1e-5f);
        }

        [Test]
        public void SquashScaleIsOneAtEndsAndOutside()
        {
            Assert.AreEqual(1f, HitFeedback.SquashScale(0f, 0.2f, 0.3f), 1e-5f);
            Assert.AreEqual(1f, HitFeedback.SquashScale(0.2f, 0.2f, 0.3f), 1e-5f);
            Assert.AreEqual(1f, HitFeedback.SquashScale(0.5f, 0.2f, 0.3f), 1e-5f); // past end
            Assert.AreEqual(1f, HitFeedback.SquashScale(0.1f, 0f, 0.3f), 1e-5f);   // zero duration
        }

        [Test]
        public void SquashScaleDipsInTheMiddle()
        {
            float mid = HitFeedback.SquashScale(0.1f, 0.2f, 0.3f); // sin(pi/2)=1 → 1-0.3
            Assert.AreEqual(0.7f, mid, 1e-4f);
        }

        [Test]
        public void SquashScaleStrengthIsClamped()
        {
            // strength clamped to 0.9 → never below 0.1
            float mid = HitFeedback.SquashScale(0.1f, 0.2f, 5f);
            Assert.AreEqual(0.1f, mid, 1e-4f);
            Assert.GreaterOrEqual(mid, 0.1f);
        }
    }
}
