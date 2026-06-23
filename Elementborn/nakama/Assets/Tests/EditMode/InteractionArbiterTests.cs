using NUnit.Framework;
using System.Collections.Generic;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class InteractionArbiterTests
    {
        private static Interaction I(float dist, int prio, string verb) => new Interaction(dist, prio, verb, () => { });

        [Test]
        public void HigherPriorityWins()
        {
            var offers = new List<Interaction> { I(0.1f, 0, "Tend plants"), I(5f, 5, "Ride"), I(1f, 0, "Talk") };
            Assert.AreEqual("Ride", InteractionArbiter.PickBest(offers).Verb);
        }

        [Test]
        public void TiesBreakToNearest()
        {
            var offers = new List<Interaction> { I(3f, 0, "Far"), I(1f, 0, "Near"), I(2f, 0, "Mid") };
            Assert.AreEqual("Near", InteractionArbiter.PickBest(offers).Verb);
        }

        [Test]
        public void DismountAlwaysWins()
        {
            var offers = new List<Interaction> { I(0f, 10, "Dismount"), I(0.1f, 5, "Ride"), I(0.1f, 0, "Talk") };
            Assert.AreEqual("Dismount", InteractionArbiter.PickBest(offers).Verb);
        }

        [Test]
        public void InvalidOffersIgnoredAndEmptyYieldsNone()
        {
            var offers = new List<Interaction>
            {
                new Interaction(1f, 9, "", () => { }),  // no verb
                new Interaction(1f, 9, "X", null)        // no action
            };
            Assert.IsFalse(InteractionArbiter.PickBest(offers).IsValid);
            Assert.IsFalse(InteractionArbiter.PickBest(new List<Interaction>()).IsValid);
        }
    }
}
