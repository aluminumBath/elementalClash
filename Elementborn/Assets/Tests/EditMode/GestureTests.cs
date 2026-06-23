using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class GestureTests
    {
        // A short constant-velocity window in a given direction (head frame = world axes here).
        private static List<GestureSample> Window(Vector3 velocity, int samples = 6)
        {
            var list = new List<GestureSample>();
            for (int i = 0; i < samples; i++)
                list.Add(new GestureSample(velocity * (0.016f * i), velocity, 0.016f * i));
            return list;
        }

        private static readonly Vector3 Fwd = Vector3.forward;
        private static readonly Vector3 Up = Vector3.up;

        [Test]
        public void ForwardMotionIsThrust()
        {
            var r = new GestureRecognizer(1.5f);
            Assert.AreEqual(GestureKind.Thrust, r.Recognize(Window(new Vector3(0, 0, 5)), Fwd, Up));
        }

        [Test]
        public void LateralMotionIsWhip()
        {
            var r = new GestureRecognizer(1.5f);
            Assert.AreEqual(GestureKind.Whip, r.Recognize(Window(new Vector3(5, 0, 0)), Fwd, Up));
        }

        [Test]
        public void RisingMotionIsUppercutAndFallingIsSlam()
        {
            var r = new GestureRecognizer(1.5f);
            Assert.AreEqual(GestureKind.Uppercut, r.Recognize(Window(new Vector3(0, 5, 0)), Fwd, Up));
            Assert.AreEqual(GestureKind.Slam, r.Recognize(Window(new Vector3(0, -5, 0)), Fwd, Up));
        }

        [Test]
        public void SlowOrBackwardMotionIsNone()
        {
            var r = new GestureRecognizer(1.5f);
            Assert.AreEqual(GestureKind.None, r.Recognize(Window(new Vector3(0, 0, 0.5f)), Fwd, Up));   // below floor
            Assert.AreEqual(GestureKind.None, r.Recognize(Window(new Vector3(0, 0, -5f)), Fwd, Up));    // retract
            Assert.AreEqual(GestureKind.None, r.Recognize(null, Fwd, Up));
        }

        [Test]
        public void EachElementHasItsOwnVocabulary()
        {
            // Fire: linear
            var fire = GestureProfile.For(Element.Fire);
            Assert.AreEqual(IntentType.PrimaryCast, fire.IntentFor(GestureKind.Thrust));
            Assert.AreEqual(IntentType.SecondaryCast, fire.IntentFor(GestureKind.Uppercut));
            Assert.AreEqual(IntentType.None, fire.IntentFor(GestureKind.Whip)); // not a fire motion

            // Water: flowing
            var water = GestureProfile.For(Element.Water);
            Assert.AreEqual(IntentType.PrimaryCast, water.IntentFor(GestureKind.Whip));
            Assert.AreEqual(IntentType.SecondaryCast, water.IntentFor(GestureKind.Slam));
            Assert.AreEqual(IntentType.None, water.IntentFor(GestureKind.Thrust));

            // Earth: heavy
            var earth = GestureProfile.For(Element.Earth);
            Assert.AreEqual(IntentType.PrimaryCast, earth.IntentFor(GestureKind.Slam));
            Assert.AreEqual(IntentType.SecondaryCast, earth.IntentFor(GestureKind.Uppercut));

            // Air: light, two-hand + dash
            var air = GestureProfile.For(Element.Air);
            Assert.AreEqual(IntentType.PrimaryCast, air.IntentFor(GestureKind.Push));
            Assert.AreEqual(IntentType.SecondaryCast, air.IntentFor(GestureKind.Whip));
            Assert.AreEqual(IntentType.Dash, air.IntentFor(GestureKind.DashStep));
        }

        [Test]
        public void HandlesReflectsTwoHandSupport()
        {
            Assert.IsTrue(GestureProfile.For(Element.Air).Handles(GestureKind.Push));
            Assert.IsFalse(GestureProfile.For(Element.Fire).Handles(GestureKind.Push));
            Assert.IsTrue(GestureProfile.For(Element.Fire).Handles(GestureKind.Guard));
        }
    }
}
