using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class OnboardingQuestsTests
    {
        private static QuestDefinition QuestWith(ObjectiveKind kind) =>
            new QuestDefinition("t_" + kind, kind.ToString(), "s", GuideNpcId.Willow.ToString(),
                new List<QuestObjective> { new QuestObjective(kind, "", 1, "do it") },
                new QuestReward(Currency.Silver, 1));

        [TestCase(ObjectiveKind.SummonCreature)]
        [TestCase(ObjectiveKind.CastAbility)]
        [TestCase(ObjectiveKind.EquipItem)]
        [TestCase(ObjectiveKind.CraftItem)]
        [TestCase(ObjectiveKind.ClaimFeatured)]
        public void RecordingAProcedureAdvancesItsObjective(ObjectiveKind kind)
        {
            var log = new QuestLog(new List<QuestDefinition> { QuestWith(kind) });
            string id = "t_" + kind;
            Assert.IsTrue(log.Start(id));
            log.Record(kind, "anything");   // the "" objective target matches any event payload
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, log.Get(id).Status);
        }

        [Test]
        public void CatalogHasAQuestForEachBasicProcedure()
        {
            var kinds = QuestCatalog.All
                .SelectMany(q => q.Objectives.Select(o => o.Kind))
                .Distinct().ToList();

            foreach (var k in new[] { ObjectiveKind.SummonCreature, ObjectiveKind.CastAbility,
                                      ObjectiveKind.EquipItem, ObjectiveKind.CraftItem, ObjectiveKind.ClaimFeatured })
                Assert.Contains(k, kinds, $"no starter quest teaches {k}");
        }

        [Test]
        public void OnboardingQuestsExistAndRootsAreStartable()
        {
            var log = new QuestLog(QuestCatalog.All);
            // All five onboarding quests are present.
            foreach (var id in new[] { "willow_first_cast", "willow_first_summon", "willow_claim_featured",
                                       "parfa_first_craft", "parfa_gear_up" })
                Assert.IsNotNull(log.Get(id), $"{id} missing from catalog");

            // The two chain roots can be started immediately...
            Assert.IsTrue(log.Start("willow_first_cast"));
            Assert.IsTrue(log.Start("parfa_first_craft"));

            // ...but the chained ones are gated until their prerequisite is completed.
            Assert.IsFalse(log.Start("willow_first_summon"), "gated behind First Channeling");
            Assert.IsFalse(log.Start("willow_claim_featured"), "gated behind Answer the Beacon");
            Assert.IsFalse(log.Start("parfa_gear_up"), "gated behind First Craft");
        }
    }
}
