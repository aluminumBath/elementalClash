using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class QuestRewardTests
    {
        [Test]
        public void SigilsDefaultToZero()
        {
            var r = new QuestReward(Currency.Silver, 50, "note");
            Assert.AreEqual(0, r.Sigils);
            Assert.AreEqual(50, r.Amount);
        }

        [Test]
        public void SigilsArePreservedAndNegativesClamp()
        {
            Assert.AreEqual(120, new QuestReward(Currency.Silver, 50, "n", sigils: 120).Sigils);
            Assert.AreEqual(0, new QuestReward(Currency.Silver, 50, "n", sigils: -10).Sigils);
            Assert.AreEqual(0, new QuestReward(Currency.Silver, -5, "n").Amount, "amount still clamps too");
        }

        [Test]
        public void CatalogProvidesASigilFaucet()
        {
            // At least one starter quest must reward Sigils, or there's no in-world faucet beyond level-ups.
            int totalSigils = QuestCatalog.All.Sum(q => q.Reward.Sigils);
            Assert.Greater(totalSigils, 0, "expected at least one quest to grant Sigils");
            Assert.IsTrue(QuestCatalog.All.Any(q => q.Reward.Sigils > 0));
        }

        [Test]
        public void EverySigilRewardIsNonNegative()
        {
            foreach (var q in QuestCatalog.All)
                Assert.GreaterOrEqual(q.Reward.Sigils, 0, $"{q.Id} has a negative Sigil reward");
        }
    }
}
