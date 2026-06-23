using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class NpcTests
    {
        [Test]
        public void EveryGuideHasAProfile()
        {
            foreach (GuideNpcId id in System.Enum.GetValues(typeof(GuideNpcId)))
            {
                var n = NpcCatalog.For(id);
                Assert.AreEqual(id, n.Id);
                Assert.IsFalse(string.IsNullOrEmpty(n.Name));
                Assert.IsFalse(string.IsNullOrEmpty(n.Greeting));
            }
            Assert.AreEqual(NpcRole.CreatureKeeper, NpcCatalog.For(GuideNpcId.Kiana).Role);
            Assert.AreEqual(SubArt.SanguineGrip, NpcCatalog.For(GuideNpcId.Kiana).SubArt); // water + blood
            Assert.AreEqual(Element.Fire, NpcCatalog.For(GuideNpcId.Parfa).Element);
        }

        [Test]
        public void HintsDescribeHabitatApproachAndRarity()
        {
            string hint = CreatureHints.WhereToFind(CreatureKind.Skytyrant);
            StringAssert.Contains("Skytyrant", hint);
            StringAssert.Contains("peaks", hint);   // habitat
            StringAssert.Contains("fly", hint);     // a flyer — reach it by air
            StringAssert.Contains("tame", hint);    // rare/stubborn note (very low tame chance)

            string koi = CreatureHints.WhereToFind(CreatureKind.Goldkoi);
            StringAssert.Contains("dive", koi);     // aquatic — reach it by water
        }

        [Test]
        public void CareVerdictTracksTreatment()
        {
            var care = new CareTracker(0.7f);
            Assert.AreEqual(CareVerdict.Cared, care.Verdict);

            care.Mistreat(); // 0.35
            Assert.AreEqual(CareVerdict.Neglected, care.Verdict);

            care.Mistreat(); // 0.0 -> abused
            Assert.AreEqual(CareVerdict.Abused, care.Verdict);

            care.CareFor(); care.CareFor(); care.CareFor(); // recover
            Assert.AreEqual(CareVerdict.Cared, care.Verdict);
        }

        [Test]
        public void FeedingAllSidekicksWithinWindowUnlocksHint()
        {
            var t = new SidekickFeedingTracker();
            double day = 86400.0;
            // feed four of five across a day and a half — not complete yet
            t.Feed(WillowSidekick.Gunnar, 0);
            t.Feed(WillowSidekick.Parrot, 0.5 * day);
            t.Feed(WillowSidekick.Blobfish, day);
            t.Feed(WillowSidekick.Mushroom, 1.5 * day);
            Assert.IsFalse(t.AllFed);
            Assert.IsFalse(t.AllFedWithin(2 * day));
            // the fifth, still within the 2-day span
            t.Feed(WillowSidekick.Chameleon, 1.8 * day);
            Assert.IsTrue(t.AllFed);
            Assert.IsTrue(t.AllFedWithin(2 * day));
            // but a span longer than the window doesn't count
            Assert.IsFalse(t.AllFedWithin(day));
        }

        [Test]
        public void FrogAccordPaysOutOnce()
        {
            var accord = new FrogAccord();
            Assert.IsFalse(accord.Agreed);
            Assert.IsTrue(accord.Reconcile());   // first time — reward due
            Assert.IsTrue(accord.Agreed);
            Assert.IsFalse(accord.Reconcile());  // no second payout
        }
    }
}
