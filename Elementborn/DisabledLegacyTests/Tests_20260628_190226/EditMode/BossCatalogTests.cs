using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class BossCatalogTests
    {
        [Test]
        public void Aerie_IsAirPhoenix()
        {
            var b = BossCatalog.For(SiteKind.Aerie);
            Assert.AreEqual(Element.Air, b.Element);
            Assert.AreEqual("Prismatic_Phoenix", b.ModelName);
        }

        [Test]
        public void SunkenGate_IsWaterGuardian()
        {
            var b = BossCatalog.For(SiteKind.SunkenEntrance);
            Assert.AreEqual(Element.Water, b.Element);
            Assert.AreEqual("Azure_Arbor_Guardian", b.ModelName);
        }

        [Test]
        public void Default_FallsBackToWarden()
        {
            var b = BossCatalog.For(SiteKind.CaveMouth);
            Assert.AreEqual(Element.Earth, b.Element);
            Assert.AreEqual("Ironhorn_Warden", b.ModelName);
        }

        [Test]
        public void EveryBoss_IsNamedScaledAndTanky()
        {
            foreach (SiteKind k in System.Enum.GetValues(typeof(SiteKind)))
            {
                var b = BossCatalog.For(k);
                Assert.IsFalse(string.IsNullOrEmpty(b.Name));
                Assert.IsFalse(string.IsNullOrEmpty(b.ModelName));
                Assert.Greater(b.Scale, 1f);
                Assert.Greater(b.HealthMultiplier, 1f);
                Assert.Greater(b.SilverReward, 0);
                Assert.GreaterOrEqual(b.GemReward, 1);
            }
        }
    }
}
