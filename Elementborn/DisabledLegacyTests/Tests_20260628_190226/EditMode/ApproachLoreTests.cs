using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class ApproachLoreTests
    {
        [Test]
        public void EveryKind_HasNonEmptyLine()
        {
            foreach (SiteKind k in System.Enum.GetValues(typeof(SiteKind)))
                Assert.IsFalse(string.IsNullOrEmpty(ApproachLore.Line(k)), k.ToString());
        }

        [Test]
        public void DistinctSites_ReadDifferently()
        {
            Assert.AreNotEqual(ApproachLore.Line(SiteKind.SunkenEntrance), ApproachLore.Line(SiteKind.Aerie));
        }
    }
}
