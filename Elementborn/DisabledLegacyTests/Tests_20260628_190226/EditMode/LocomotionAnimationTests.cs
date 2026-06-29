using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class LocomotionAnimationTests
    {
        [Test]
        public void BlendIsZeroAtIdle()
        {
            Assert.AreEqual(0f, LocomotionAnimation.Blend(0f, 1.5f, 5.5f), 1e-4f);
            Assert.AreEqual(0f, LocomotionAnimation.Blend(0.05f, 1.5f, 5.5f), 1e-4f); // below idle threshold
        }

        [Test]
        public void BlendIsHalfAtWalkAndOneAtRun()
        {
            Assert.AreEqual(0.5f, LocomotionAnimation.Blend(1.5f, 1.5f, 5.5f), 1e-4f);
            Assert.AreEqual(1f, LocomotionAnimation.Blend(5.5f, 1.5f, 5.5f), 1e-4f);
            Assert.AreEqual(1f, LocomotionAnimation.Blend(99f, 1.5f, 5.5f), 1e-4f); // clamped above run
        }

        [Test]
        public void BlendInterpolatesBetweenWalkAndRun()
        {
            // midpoint of [1.5, 5.5] is 3.5 → 0.75
            Assert.AreEqual(0.75f, LocomotionAnimation.Blend(3.5f, 1.5f, 5.5f), 1e-4f);
        }

        [Test]
        public void IsMovingTracksIdleThreshold()
        {
            Assert.IsFalse(LocomotionAnimation.IsMoving(0.05f));
            Assert.IsTrue(LocomotionAnimation.IsMoving(2f));
        }

        [Test]
        public void DampApproachesTargetAndConverges()
        {
            float v = LocomotionAnimation.Damp(0f, 1f, 10f, 0.1f);
            Assert.Greater(v, 0f);
            Assert.Less(v, 1f);

            for (int i = 0; i < 200; i++) v = LocomotionAnimation.Damp(v, 1f, 10f, 0.1f);
            Assert.AreEqual(1f, v, 1e-3f);
        }

        [Test]
        public void DampWithNonPositiveRateOrDtSnaps()
        {
            Assert.AreEqual(1f, LocomotionAnimation.Damp(0f, 1f, 0f, 0.1f), 1e-6f);
            Assert.AreEqual(1f, LocomotionAnimation.Damp(0f, 1f, 10f, 0f), 1e-6f);
        }
    }
}
