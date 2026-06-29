using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class HiddenMoveTests
    {
        [Test]
        public void EachElementMapsToItsSignatureAndBack()
        {
            foreach (Element e in new[] { Element.Fire, Element.Water, Element.Earth, Element.Air })
            {
                var move = HiddenMoves.For(e);
                Assert.AreNotEqual(HiddenMove.None, move, e + " should have a signature move");
                Assert.AreEqual(e, HiddenMoves.ElementOf(move));
            }
            Assert.AreEqual(HiddenMove.WaterDash, HiddenMoves.For(Element.Water));
            Assert.AreEqual(HiddenMove.FireBreath, HiddenMoves.For(Element.Fire));
        }

        [Test]
        public void FullTurnNeedsNearlyAComplete360()
        {
            Assert.IsTrue(HiddenGestures.CompletedFullTurn(350f));
            Assert.IsTrue(HiddenGestures.CompletedFullTurn(-340f)); // either direction
            Assert.IsFalse(HiddenGestures.CompletedFullTurn(90f));
        }

        [Test]
        public void WristSpinNeedsFastAngularVelocity()
        {
            Assert.IsTrue(HiddenGestures.WristSpinning(new Vector3(0, 0, 10f), 8f));
            Assert.IsFalse(HiddenGestures.WristSpinning(new Vector3(1f, 0, 0), 8f));
        }

        [Test]
        public void CrossedArmsNeedsHandsHighAndTogether()
        {
            var head = new Vector3(0, 1.6f, 0);
            Assert.IsTrue(HiddenGestures.HandsCrossedHigh(
                new Vector3(0.1f, 1.35f, 0.1f), new Vector3(-0.1f, 1.35f, 0.1f), head));
            // hands low and far apart -> not the pose
            Assert.IsFalse(HiddenGestures.HandsCrossedHigh(
                new Vector3(1f, 0.6f, 0), new Vector3(-1f, 0.6f, 0), head));
        }

        [Test]
        public void HandAtMouthIsClose()
        {
            var head = new Vector3(0, 1.6f, 0);
            Assert.IsTrue(HiddenGestures.HandNearHead(new Vector3(0.05f, 1.55f, 0.08f), head, 0.3f));
            Assert.IsFalse(HiddenGestures.HandNearHead(new Vector3(0.8f, 1.6f, 0), head, 0.3f));
        }
    }
}
