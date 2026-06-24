using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ShakeOffsetTests
    {
        [Test]
        public void ZeroAtAndAfterEnd()
        {
            ShakeOffset.Evaluate(1.0f, 1.0f, 0.5f, 25f, 1f, 2f, out float x, out float y);
            Assert.AreEqual(0f, x); Assert.AreEqual(0f, y);
            ShakeOffset.Evaluate(2.0f, 1.0f, 0.5f, 25f, 1f, 2f, out x, out y);
            Assert.AreEqual(0f, x); Assert.AreEqual(0f, y);
        }

        [Test]
        public void ZeroForNonPositiveOrNegativeInputs()
        {
            ShakeOffset.Evaluate(0.1f, 0f, 0.5f, 25f, 0f, 0f, out float x, out float y);
            Assert.AreEqual(0f, x); Assert.AreEqual(0f, y);
            ShakeOffset.Evaluate(0.1f, 1f, 0f, 25f, 0f, 0f, out x, out y);
            Assert.AreEqual(0f, x); Assert.AreEqual(0f, y);
            ShakeOffset.Evaluate(-0.1f, 1f, 0.5f, 25f, 0f, 0f, out x, out y);
            Assert.AreEqual(0f, x); Assert.AreEqual(0f, y);
        }

        [Test]
        public void NeitherAxisExceedsAmplitude()
        {
            const float amp = 0.4f;
            for (int i = 1; i < 50; i++)
            {
                float t = i / 50f; // (0,1)
                ShakeOffset.Evaluate(t, 1f, amp, 30f, 3.1f, 6.7f, out float x, out float y);
                Assert.LessOrEqual(System.Math.Abs(x), amp + 1e-5f, $"x at t={t}");
                Assert.LessOrEqual(System.Math.Abs(y), amp + 1e-5f, $"y at t={t}");
            }
        }

        [Test]
        public void DecaysOverTime()
        {
            // Peak magnitude in an early window should exceed peak magnitude in a late window.
            float early = 0f, late = 0f;
            for (int i = 0; i < 100; i++)
            {
                float te = 0.05f + 0.001f * i;  // ~0.05..0.15
                float tl = 0.85f + 0.001f * i;  // ~0.85..0.95
                ShakeOffset.Evaluate(te, 1f, 1f, 60f, 0.7f, 1.9f, out float xe, out float ye);
                ShakeOffset.Evaluate(tl, 1f, 1f, 60f, 0.7f, 1.9f, out float xl, out float yl);
                early = System.Math.Max(early, System.Math.Abs(xe) + System.Math.Abs(ye));
                late = System.Math.Max(late, System.Math.Abs(xl) + System.Math.Abs(yl));
            }
            Assert.Greater(early, late);
        }
    }
}
