using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class GlideMotionTests
    {
        [Test]
        public void NotGlidingWhenGrounded()
        {
            Assert.IsFalse(GlideMotion.IsGliding(true, -5f, true));
        }

        [Test]
        public void NotGlidingWhenRisingOrButtonReleased()
        {
            Assert.IsFalse(GlideMotion.IsGliding(false, 3f, true));   // still rising
            Assert.IsFalse(GlideMotion.IsGliding(false, -5f, false)); // button not held
        }

        [Test]
        public void GlidingWhenAirborneFallingAndHeld()
        {
            Assert.IsTrue(GlideMotion.IsGliding(false, -5f, true));
        }

        [Test]
        public void ClampDescentCapsFastFalls()
        {
            Assert.AreEqual(-2.2f, GlideMotion.ClampDescent(-30f, 2.2f), 1e-4f);
            Assert.AreEqual(-2.2f, GlideMotion.ClampDescent(-2.2f, 2.2f), 1e-4f); // exactly at the floor
        }

        [Test]
        public void ClampDescentLeavesGentleAndRisingUntouched()
        {
            Assert.AreEqual(-1f, GlideMotion.ClampDescent(-1f, 2.2f), 1e-4f); // slower than glide: unchanged
            Assert.AreEqual(4f, GlideMotion.ClampDescent(4f, 2.2f), 1e-4f);   // rising: unchanged
            Assert.AreEqual(0f, GlideMotion.ClampDescent(0f, 2.2f), 1e-4f);
        }
    }
}
