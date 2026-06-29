using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class AchievementTests
    {
        [Test]
        public void FirstEventUnlocksTheTargetOneAndReturnsIt()
        {
            var p = new AchievementProgress();
            var unlocked = p.Record(AchievementMetric.CreaturesDefeated, 1, "Crab");
            Assert.IsTrue(unlocked.Any(a => a.Id == "first_blood"));
            Assert.IsFalse(unlocked.Any(a => a.Id == "hunter")); // target 25, not yet
        }

        [Test]
        public void ProgressAccumulatesAndUnlocksAtTargetExactlyOnce()
        {
            var p = new AchievementProgress();
            p.Record(AchievementMetric.CreaturesDefeated, 24); // first_blood crosses here
            var atTarget = p.Record(AchievementMetric.CreaturesDefeated, 1); // 25 -> hunter crosses
            Assert.IsTrue(atTarget.Any(a => a.Id == "hunter"));
            var past = p.Record(AchievementMetric.CreaturesDefeated, 1); // 26 -> nothing new
            Assert.IsFalse(past.Any(a => a.Id == "hunter"));
        }

        [Test]
        public void TheAnyBucketCountsEveryQualifier()
        {
            var p = new AchievementProgress();
            p.Record(AchievementMetric.CreaturesDefeated, 1, "FireDragon");
            p.Record(AchievementMetric.CreaturesDefeated, 1, "Crab");
            Assert.AreEqual(2, p.CountFor(AchievementMetric.CreaturesDefeated));
        }

        [Test]
        public void AQualifiedAchievementOnlyCountsItsQualifier()
        {
            var p = new AchievementProgress();
            var coffers = AchievementCatalog.All.First(a => a.Id == "coffers"); // CurrencyEarned / 1000 / "Silver"
            p.Record(AchievementMetric.CurrencyEarned, 1000, "Ruby"); // wrong currency
            Assert.IsFalse(p.IsUnlocked(coffers));
            p.Record(AchievementMetric.CurrencyEarned, 1000, "Silver");
            Assert.IsTrue(p.IsUnlocked(coffers));
        }

        [Test]
        public void ProgressIsClampedToTarget()
        {
            var p = new AchievementProgress();
            var hunter = AchievementCatalog.All.First(a => a.Id == "hunter"); // target 25
            p.Record(AchievementMetric.CreaturesDefeated, 100);
            Assert.AreEqual(25, p.Progress(hunter));
            Assert.AreEqual(100, p.CountFor(AchievementMetric.CreaturesDefeated));
        }

        [Test]
        public void SaveLoadRoundTripsCountsAndUnlocks()
        {
            var p = new AchievementProgress();
            p.Record(AchievementMetric.CreaturesTamed, 5);   // tamer
            p.Record(AchievementMetric.AbilitiesCast, 60);   // channeler
            var saved = p.ToSave();

            var loaded = new AchievementProgress();
            loaded.LoadFrom(saved);
            Assert.AreEqual(p.UnlockedCount, loaded.UnlockedCount);
            Assert.AreEqual(5, loaded.CountFor(AchievementMetric.CreaturesTamed));
            Assert.IsTrue(loaded.IsUnlocked(AchievementCatalog.All.First(a => a.Id == "channeler")));
        }

        [Test]
        public void CatalogHasUniqueIdsAndPositiveTargets()
        {
            var ids = AchievementCatalog.All.Select(a => a.Id).ToList();
            Assert.AreEqual(ids.Count, ids.Distinct().Count());
            Assert.IsTrue(AchievementCatalog.All.All(a => a.Target >= 1));
            Assert.IsTrue(AchievementCatalog.All.All(a => !string.IsNullOrEmpty(a.Name)));
        }
    }
}
