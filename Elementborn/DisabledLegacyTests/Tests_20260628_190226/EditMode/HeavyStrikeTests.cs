using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class HeavyStrikeTests
    {
        [Test]
        public void ImpactLandsAheadAlongFacing()
        {
            var p = HeavyStrike.ImpactPoint(Vector3.zero, Vector3.forward);
            Assert.AreEqual(0f, p.x, 0.0001f);
            Assert.AreEqual(0f, p.y, 0.0001f);
            Assert.AreEqual(HeavyStrike.ImpactDistance, p.z, 0.0001f);
        }

        [Test]
        public void ImpactIgnoresVerticalFacing()
        {
            // Aiming up-and-forward still lands on the horizontal plane ahead.
            var p = HeavyStrike.ImpactPoint(Vector3.zero, new Vector3(0f, 5f, 1f));
            Assert.AreEqual(0f, p.y, 0.0001f);
            Assert.Greater(p.z, 0f);
        }

        [Test]
        public void NoFacingLandsAtTheOrigin()
        {
            Assert.AreEqual(Vector3.zero, HeavyStrike.ImpactPoint(Vector3.zero, Vector3.zero));
        }

        [Test]
        public void CoversInsideRadiusButNotOutside()
        {
            var impact = new Vector3(0f, 0f, 3f);
            Assert.IsTrue(HeavyStrike.Covers(impact, impact));                                  // dead centre
            Assert.IsTrue(HeavyStrike.Covers(impact, impact + new Vector3(HeavyStrike.ImpactRadius - 0.1f, 0f, 0f)));
            Assert.IsFalse(HeavyStrike.Covers(impact, impact + new Vector3(HeavyStrike.ImpactRadius + 0.5f, 0f, 0f)));
        }

        [Test]
        public void CoverageIgnoresHeight()
        {
            var impact = new Vector3(0f, 0f, 3f);
            Assert.IsTrue(HeavyStrike.Covers(impact, impact + new Vector3(0f, 10f, 0f)));
        }

        [Test]
        public void RadiusGrowsWithCharge()
        {
            Assert.AreEqual(HeavyStrike.ImpactRadius, HeavyStrike.RadiusForCharge(0f), 0.0001f);
            Assert.Greater(HeavyStrike.RadiusForCharge(0.5f), HeavyStrike.RadiusForCharge(0f));
            Assert.Greater(HeavyStrike.RadiusForCharge(1f), HeavyStrike.RadiusForCharge(0.5f));
        }

        [Test]
        public void TelegraphIsAPositiveWindow()
        {
            Assert.Greater(HeavyStrike.TelegraphSeconds, 0f);
        }

        [Test]
        public void ArcStartsAtStartAndEndsAtEnd()
        {
            var start = new Vector3(0f, 1f, 0f);
            var end = new Vector3(0f, 0f, 6f);
            Assert.AreEqual(start, HeavyStrike.ArcPoint(start, end, 3f, 0f));
            Assert.AreEqual(end, HeavyStrike.ArcPoint(start, end, 3f, 1f));
        }

        [Test]
        public void ArcLiftsAboveTheStraightLineAtMidpoint()
        {
            var start = new Vector3(0f, 1f, 0f);
            var end = new Vector3(0f, 1f, 6f);
            Assert.Greater(HeavyStrike.ArcPoint(start, end, 3f, 0.5f).y, 1f); // lifted above the flat line
        }
    }
}
