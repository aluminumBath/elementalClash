using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class HealthTests
    {
        [Test]
        public void SetMaxKeepsTheSameObjectSoSubscribersStayValid()
        {
            var hp = new Health(30f);
            int deaths = 0;
            hp.Died += () => deaths++;          // subscribe BEFORE the max change

            hp.SetMax(100f);                    // in place — must not detach the subscriber
            Assert.AreEqual(100f, hp.Max);
            Assert.AreEqual(100f, hp.Current);  // refills to full by default

            hp.Apply(new DamageInfo(100f, Element.Earth));
            Assert.IsTrue(hp.IsDead);
            Assert.AreEqual(1, deaths);         // the pre-existing Died handler still fired
        }

        [Test]
        public void SetMaxWithoutRefillClampsCurrent()
        {
            var hp = new Health(100f);
            hp.Apply(new DamageInfo(40f, Element.Earth)); // Current = 60
            hp.SetMax(50f, refill: false);       // clamp down
            Assert.AreEqual(50f, hp.Max);
            Assert.AreEqual(50f, hp.Current);    // 60 clamped to new max of 50

            hp.SetMax(20f, refill: false);
            Assert.AreEqual(20f, hp.Current);
        }
    }
}
