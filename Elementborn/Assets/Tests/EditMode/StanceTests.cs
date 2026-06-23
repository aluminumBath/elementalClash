using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class StanceTests
    {
        private static List<GestureSample> Window(Vector3 velocity, int samples = 6)
        {
            var list = new List<GestureSample>();
            for (int i = 0; i < samples; i++)
                list.Add(new GestureSample(velocity * (0.016f * i), velocity, 0.016f * i));
            return list;
        }

        [Test]
        public void ForwardDownScoopIsPaddle()
        {
            var r = new GestureRecognizer(1.5f);
            // forward + down diagonal = canoe-paddle stroke
            Assert.AreEqual(GestureKind.Paddle, r.Recognize(Window(new Vector3(0, -4, 4)), Vector3.forward, Vector3.up));
            // a pure forward / pure down stays a thrust / slam
            Assert.AreEqual(GestureKind.Thrust, r.Recognize(Window(new Vector3(0, 0, 5)), Vector3.forward, Vector3.up));
            Assert.AreEqual(GestureKind.Slam, r.Recognize(Window(new Vector3(0, -5, 0)), Vector3.forward, Vector3.up));
        }

        private static HandInput Fist(float hold, bool raised = false)
            => new HandInput(HandPose.Fist, GestureKind.None, 0f, raised, hold);

        private static HandInput Paddling()
            => new HandInput(HandPose.Neutral, GestureKind.Paddle, 4f, false, 0f);

        [Test]
        public void WaterFistPlusPaddleIsSustainedIceFlow()
        {
            var resolver = new StanceResolver();

            var a = resolver.Resolve(Element.Water, Fist(1f), Paddling()); // fist left, paddle right
            Assert.AreEqual(IntentType.SecondaryCast, a.Intent);
            Assert.IsTrue(a.Sustained);
            Assert.AreEqual("IceFlow", a.Label);

            var b = resolver.Resolve(Element.Water, Paddling(), Fist(1f)); // hands swapped
            Assert.AreEqual(IntentType.SecondaryCast, b.Intent);
            Assert.AreEqual("IceFlow", b.Label);
        }

        [Test]
        public void IceFlowChargeScalesWithFistHold()
        {
            var resolver = new StanceResolver();
            Assert.AreEqual(1.0f, resolver.Resolve(Element.Water, Fist(1.5f), Paddling()).Charge, 0.001f);
            Assert.AreEqual(0.5f, resolver.Resolve(Element.Water, Fist(0.75f), Paddling()).Charge, 0.001f);
        }

        [Test]
        public void IceFlowIsWaterOnly()
        {
            var resolver = new StanceResolver();
            Assert.IsTrue(resolver.Resolve(Element.Fire, Fist(1f), Paddling()).IsNone);
            Assert.IsTrue(resolver.Resolve(Element.Earth, Fist(1f), Paddling()).IsNone);
        }

        [Test]
        public void BothHandsRaisedAndStillIsGuard()
        {
            var resolver = new StanceResolver();
            var left = new HandInput(HandPose.Fist, GestureKind.None, 0f, raised: true, 0f);
            var right = new HandInput(HandPose.OpenPalm, GestureKind.None, 0f, raised: true, 0f);

            var guard = resolver.Resolve(Element.Fire, left, right);
            Assert.AreEqual(IntentType.Defend, guard.Intent);
            Assert.IsTrue(guard.Sustained);
            Assert.AreEqual("Guard", guard.Label);
        }

        [Test]
        public void NeutralHandsProduceNothing()
        {
            var resolver = new StanceResolver();
            var neutral = new HandInput(HandPose.Neutral, GestureKind.None, 0f, false, 0f);
            Assert.IsTrue(resolver.Resolve(Element.Fire, neutral, neutral).IsNone);
        }
    }
}
