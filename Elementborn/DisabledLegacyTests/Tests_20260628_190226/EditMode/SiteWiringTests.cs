using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    /// <summary>Locks the open-world -> interior loop. A SiteEntrance configures from <c>SiteCatalog.For(kind)</c>
    /// and the interior themes off <c>info.Kind</c>/<c>Domain</c>/<c>Payload</c> (boss via <c>BossCatalog</c>,
    /// flood when Underwater, approach lore by kind). These assert the catalogs stay consistent, so an entrance
    /// always opens the right themed interior with the right boss and lore.</summary>
    public class SiteWiringTests
    {
        [Test]
        public void EveryKind_ReportsItsOwnKind()
        {
            foreach (SiteKind k in SiteCatalog.All)
                Assert.AreEqual(k, SiteCatalog.For(k).Kind);
        }

        [Test]
        public void EveryKind_IsReachableFromItsBiome()
        {
            // The placer seeds each region via ForBiome(region.Biome); each catalog site must round-trip.
            foreach (SiteKind k in SiteCatalog.All)
            {
                var found = SiteCatalog.ForBiome(SiteCatalog.For(k).PreferredBiome);
                Assert.IsTrue(found.HasValue, k.ToString());
                Assert.AreEqual(k, found.Value);
            }
        }

        [Test]
        public void BossSites_ResolveToThemedBosses()
        {
            Assert.AreEqual(SitePayload.Boss, SiteCatalog.For(SiteKind.Aerie).Payload);
            Assert.AreEqual(Element.Air, BossCatalog.For(SiteKind.Aerie).Element);

            Assert.AreEqual(SitePayload.Boss, SiteCatalog.For(SiteKind.SunkenEntrance).Payload);
            Assert.AreEqual(Element.Water, BossCatalog.For(SiteKind.SunkenEntrance).Element);
        }

        [Test]
        public void EveryBossSite_HasARealBoss()
        {
            foreach (SiteKind k in SiteCatalog.All)
                if (SiteCatalog.For(k).Payload == SitePayload.Boss)
                {
                    var b = BossCatalog.For(k);
                    Assert.IsFalse(string.IsNullOrEmpty(b.ModelName), k.ToString());
                    Assert.Greater(b.HealthMultiplier, 1f);
                }
        }

        [Test]
        public void SunkenGate_IsUnderwater()
        {
            // The interior floods only for Underwater-domain sites, so the Sunken Gate must be one.
            Assert.AreEqual(SiteDomain.Underwater, SiteCatalog.For(SiteKind.SunkenEntrance).Domain);
        }
    }
}
