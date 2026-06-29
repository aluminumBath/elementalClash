using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class DamagePopupTests
    {
        [Test]
        public void FormatRoundsToNearestInteger()
        {
            Assert.AreEqual("13", DamagePopup.Format(13.2f));
            Assert.AreEqual("13", DamagePopup.Format(12.7f));
            Assert.AreEqual("14", DamagePopup.Format(13.5f));
            Assert.AreEqual("100", DamagePopup.Format(99.6f));
        }

        [Test]
        public void FormatFloorsPositiveHitsToOneAndZeroStaysZero()
        {
            Assert.AreEqual("1", DamagePopup.Format(0.3f)); // a real hit always reads at least 1
            Assert.AreEqual("1", DamagePopup.Format(1f));
            Assert.AreEqual("0", DamagePopup.Format(0f));
            Assert.AreEqual("0", DamagePopup.Format(-5f));
        }

        [Test]
        public void EvaluateAtStart()
        {
            DamagePopup.Evaluate(0f, 0.9f, out float rise, out float alpha, out float scale);
            Assert.AreEqual(0f, rise, 1e-5f);
            Assert.AreEqual(1f, alpha, 1e-5f);
            Assert.AreEqual(0.6f, scale, 1e-5f);
        }

        [Test]
        public void EvaluateAtEnd()
        {
            DamagePopup.Evaluate(0.9f, 0.9f, out float rise, out float alpha, out float scale);
            Assert.AreEqual(1f, rise, 1e-5f);
            Assert.AreEqual(0f, alpha, 1e-5f);
            Assert.AreEqual(1f, scale, 1e-5f);
        }

        [Test]
        public void EvaluateMidFadeAndPop()
        {
            DamagePopup.Evaluate(0.72f, 0.9f, out _, out float alpha, out _); // t=0.8 → alpha 0.5
            Assert.AreEqual(0.5f, alpha, 1e-4f);

            DamagePopup.Evaluate(0.0675f, 0.9f, out _, out _, out float scale); // t=0.075 → pop half → 0.8
            Assert.AreEqual(0.8f, scale, 1e-4f);
        }

        [Test]
        public void EvaluateRiseIsMonotonicAndAlphaBounded()
        {
            float prev = -1f;
            for (int i = 0; i <= 10; i++)
            {
                DamagePopup.Evaluate(i * 0.09f, 0.9f, out float rise, out float alpha, out _);
                Assert.GreaterOrEqual(rise, prev);
                Assert.GreaterOrEqual(alpha, 0f);
                Assert.LessOrEqual(alpha, 1f);
                prev = rise;
            }
        }

        [Test]
        public void EvaluateZeroLifetimeIsSafe()
        {
            DamagePopup.Evaluate(0.5f, 0f, out float rise, out float alpha, out float scale);
            Assert.AreEqual(1f, rise, 1e-5f);
            Assert.AreEqual(0f, alpha, 1e-5f);
            Assert.AreEqual(1f, scale, 1e-5f);
        }
    }
}
