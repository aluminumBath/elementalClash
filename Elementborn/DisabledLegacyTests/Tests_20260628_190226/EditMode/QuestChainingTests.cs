using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class QuestChainingTests
    {
        private static QuestDefinition Def(string id, params string[] prereqs) =>
            new QuestDefinition(id, id, "s", GuideNpcId.Willow.ToString(),
                new List<QuestObjective> { new QuestObjective(ObjectiveKind.TalkToNpc, "", 1, "x") },
                new QuestReward(Currency.Silver, 1),
                prereqs.Length == 0 ? null : new List<string>(prereqs));

        private static bool Offered(QuestLog log, string id) =>
            log.AvailableFrom(GuideNpcId.Willow.ToString()).Any(s => s.Definition.Id == id);

        [Test]
        public void UnmetPrerequisiteHidesAndBlocksAQuest()
        {
            var log = new QuestLog(new List<QuestDefinition> { Def("a"), Def("b", "a") });
            Assert.IsFalse(log.PrerequisitesMet("b"));
            Assert.IsFalse(Offered(log, "b"), "b shouldn't be offered until a is done");
            Assert.IsFalse(log.Start("b"), "b shouldn't be startable until a is done");
            Assert.IsTrue(Offered(log, "a"), "a has no prereqs, so it's offered");
        }

        [Test]
        public void CompletingThePrerequisiteUnlocksTheNext()
        {
            var log = new QuestLog(new List<QuestDefinition> { Def("a"), Def("b", "a") });
            Assert.IsTrue(log.Start("a"));
            log.Record(ObjectiveKind.TalkToNpc, "anyone");
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, log.Get("a").Status);
            Assert.IsFalse(log.PrerequisitesMet("b"), "ready-to-turn-in is not yet completed");

            Assert.IsNotNull(log.TurnIn("a"));
            Assert.AreEqual(QuestStatus.Completed, log.Get("a").Status);
            Assert.IsTrue(log.PrerequisitesMet("b"));
            Assert.IsTrue(Offered(log, "b"));
            Assert.IsTrue(log.Start("b"));
        }

        [Test]
        public void MultiplePrerequisitesAllMustComplete()
        {
            var log = new QuestLog(new List<QuestDefinition> { Def("a"), Def("b"), Def("c", "a", "b") });
            Complete(log, "a");
            Assert.IsFalse(log.PrerequisitesMet("c"), "b is still outstanding");
            Complete(log, "b");
            Assert.IsTrue(log.PrerequisitesMet("c"));
        }

        [Test]
        public void NoPrerequisitesMeansImmediatelyAvailable()
        {
            var log = new QuestLog(new List<QuestDefinition> { Def("solo") });
            Assert.IsTrue(log.PrerequisitesMet("solo"));
            Assert.IsTrue(Offered(log, "solo"));
        }

        [Test]
        public void CatalogOnboardingChainIsWiredInOrder()
        {
            var all = QuestCatalog.All.ToDictionary(q => q.Id);
            CollectionAssert.Contains(all["willow_first_summon"].Prerequisites.ToList(), "willow_first_cast");
            CollectionAssert.Contains(all["willow_claim_featured"].Prerequisites.ToList(), "willow_first_summon");
            CollectionAssert.Contains(all["parfa_gear_up"].Prerequisites.ToList(), "parfa_first_craft");
            Assert.IsEmpty(all["willow_first_cast"].Prerequisites);
            Assert.IsEmpty(all["parfa_first_craft"].Prerequisites);
        }

        private static void Complete(QuestLog log, string id)
        {
            log.Start(id);
            log.Record(ObjectiveKind.TalkToNpc, "anyone");
            log.TurnIn(id);
        }
    }
}
