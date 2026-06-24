using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class WristGestureTests
    {
        // Head at origin looking down +Z; wrist 1m ahead; right hand right next to the wrist.
        [Test]
        public void ActivatesWhenGazingAndTouching()
        {
            bool ok = WristGesture.IsActivated(
                0, 0, 0, 0, 0, 1,        // head + forward +Z
                0, 0, 1,                 // wrist 1m ahead
                0.03f, 0, 1.0f,          // right hand 3cm away
                35f, 0.12f);
            Assert.IsTrue(ok);
        }

        [Test]
        public void FailsWhenRightHandTooFar()
        {
            bool ok = WristGesture.IsActivated(
                0, 0, 0, 0, 0, 1,
                0, 0, 1,
                0.5f, 0, 1.0f,           // 50cm away → not touching
                35f, 0.12f);
            Assert.IsFalse(ok);
        }

        [Test]
        public void FailsWhenNotGazingAtWrist()
        {
            // looking +X but wrist is at +Z → ~90 degrees off
            bool ok = WristGesture.IsActivated(
                0, 0, 0, 1, 0, 0,
                0, 0, 1,
                0.02f, 0, 1.0f,
                35f, 0.12f);
            Assert.IsFalse(ok);
        }

        [Test]
        public void FailsSafelyWhenHeadAtWrist()
        {
            bool ok = WristGesture.IsActivated(
                0, 0, 1, 0, 0, 1,
                0, 0, 1,                 // wrist coincides with head → zero gaze vector
                0, 0, 1,
                35f, 0.12f);
            Assert.IsFalse(ok);
        }
    }
}
