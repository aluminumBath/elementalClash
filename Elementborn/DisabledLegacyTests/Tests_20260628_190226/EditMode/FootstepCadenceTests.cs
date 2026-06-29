using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class FootstepCadenceTests
    {
        [Test]
        public void EmitsOneStepPerStride()
        {
            var gait = new FootstepCadence(1.6f);
            Assert.AreEqual(0, gait.Accumulate(1.0f)); // not a full stride yet
            Assert.AreEqual(1, gait.Accumulate(1.0f)); // 2.0 total → one step, 0.4 carried
            Assert.AreEqual(0, gait.Accumulate(1.0f)); // 1.4 carried
            Assert.AreEqual(1, gait.Accumulate(0.3f)); // 1.7 carried → one step
        }

        [Test]
        public void LongDistanceEmitsMultipleSteps()
        {
            var gait = new FootstepCadence(1.0f);
            Assert.AreEqual(5, gait.Accumulate(5.0f));
        }

        [Test]
        public void IgnoresZeroOrNegativeDistance()
        {
            var gait = new FootstepCadence(1.0f);
            Assert.AreEqual(0, gait.Accumulate(0f));
            Assert.AreEqual(0, gait.Accumulate(-3f));
        }

        [Test]
        public void FeetAlternate()
        {
            var gait = new FootstepCadence();
            Assert.IsTrue(gait.NextIsLeft());
            Assert.IsFalse(gait.NextIsLeft());
            Assert.IsTrue(gait.NextIsLeft());
        }

        [Test]
        public void ResetClearsAccumulationAndFoot()
        {
            var gait = new FootstepCadence(1.0f);
            gait.Accumulate(0.9f);
            gait.NextIsLeft(); // now right is next
            gait.Reset();
            Assert.AreEqual(0, gait.Accumulate(0.9f)); // accumulation cleared → still under a stride
            Assert.IsTrue(gait.NextIsLeft());          // foot reset to left
        }
    }
}
