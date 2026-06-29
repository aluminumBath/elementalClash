using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SweepArcTests
    {
        private static readonly Vector3 Origin = Vector3.zero;
        private static readonly Vector3 Forward = Vector3.forward;

        [Test]
        public void TargetStraightAheadInRangeIsCovered()
        {
            Assert.IsTrue(SweepArc.Covers(Origin, Forward, new Vector3(0f, 0f, 2f)));
        }

        [Test]
        public void TargetBehindIsNotCovered()
        {
            Assert.IsFalse(SweepArc.Covers(Origin, Forward, new Vector3(0f, 0f, -2f)));
        }

        [Test]
        public void TargetBeyondRangeIsNotCovered()
        {
            Assert.IsFalse(SweepArc.Covers(Origin, Forward, new Vector3(0f, 0f, SweepArc.Range + 1f)));
        }

        [Test]
        public void TargetAtTheArcEdgeIsCoveredButJustOutsideIsNot()
        {
            // 60 degrees off +Z at distance 2 -> on the cone edge (covered).
            float r = 2f;
            var edge = new Vector3(Mathf.Sin(60f * Mathf.Deg2Rad) * r, 0f, Mathf.Cos(60f * Mathf.Deg2Rad) * r);
            Assert.IsTrue(SweepArc.Covers(Origin, Forward, edge));

            // 90 degrees off -> outside the 60-degree half-angle.
            Assert.IsFalse(SweepArc.Covers(Origin, Forward, new Vector3(2f, 0f, 0f)));
        }

        [Test]
        public void PointBlankIsCovered()
        {
            Assert.IsTrue(SweepArc.Covers(Origin, Forward, Origin));
        }

        [Test]
        public void HeightDifferenceIsIgnored()
        {
            // Directly ahead but well above: still covered, because the arc is judged on the XZ plane.
            Assert.IsTrue(SweepArc.Covers(Origin, Forward, new Vector3(0f, 5f, 2f)));
        }
    }
}
