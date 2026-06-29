using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SiteCatalogTests
    {
        [Test]
        public void EveryKindHasNameAndIsCovered()
        {
            Assert.AreEqual(5, SiteCatalog.All.Length);
            foreach (var kind in SiteCatalog.All)
            {
                var info = SiteCatalog.For(kind);
                Assert.AreEqual(kind, info.Kind);
                Assert.IsFalse(string.IsNullOrEmpty(info.DisplayName));
                Assert.IsFalse(string.IsNullOrEmpty(info.Lore));
            }
        }

        [Test]
        public void DomainsMatchTheConcept()
        {
            Assert.AreEqual(SiteDomain.Subterranean, SiteCatalog.For(SiteKind.CaveMouth).Domain);
            Assert.AreEqual(SiteDomain.Aerial, SiteCatalog.For(SiteKind.Aerie).Domain);
            Assert.AreEqual(SiteDomain.Underwater, SiteCatalog.For(SiteKind.SunkenEntrance).Domain);
            Assert.AreEqual(SiteDomain.Surface, SiteCatalog.For(SiteKind.TempleDoor).Domain);
            Assert.AreEqual(SiteDomain.Surface, SiteCatalog.For(SiteKind.Spring).Domain);
        }

        [Test]
        public void BiomeMatchingPicksTheRightKind()
        {
            Assert.AreEqual(SiteKind.CaveMouth, SiteCatalog.ForBiome(BiomeType.Mountains));
            Assert.AreEqual(SiteKind.Aerie, SiteCatalog.ForBiome(BiomeType.CloudTemple));
            Assert.AreEqual(SiteKind.SunkenEntrance, SiteCatalog.ForBiome(BiomeType.Island));
            Assert.AreEqual(SiteKind.TempleDoor, SiteCatalog.ForBiome(BiomeType.ForestTemple));
            Assert.AreEqual(SiteKind.Spring, SiteCatalog.ForBiome(BiomeType.Marsh));
        }

        [Test]
        public void UnmatchedBiomeReturnsNull()
        {
            Assert.IsNull(SiteCatalog.ForBiome(BiomeType.Desert));
            Assert.IsNull(SiteCatalog.ForBiome(BiomeType.Plains));
        }
    }
}
