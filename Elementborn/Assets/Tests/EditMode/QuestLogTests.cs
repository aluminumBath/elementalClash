using NUnit.Framework;
using System.Collections.Generic;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class QuestLogTests
    {
        private static QuestDefinition Defeat(string id, int n, string target = "") => new QuestDefinition(
            id, "T", "s", "Npc",
            new List<QuestObjective> { new QuestObjective(ObjectiveKind.DefeatCreature, target, n, "defeat") },
            new QuestReward(Currency.Silver, 10));

        [Test]
        public void StartActivatesOnlyOnceAndOnlyFromNotStarted()
        {
            var log = new QuestLog(new[] { Defeat("q", 1) });
            Assert.AreEqual(QuestStatus.NotStarted, log.Get("q").Status);
            Assert.IsTrue(log.Start("q"));
            Assert.AreEqual(QuestStatus.Active, log.Get("q").Status);
            Assert.IsFalse(log.Start("q"));          // already active
            Assert.IsFalse(log.Start("missing"));
        }

        [Test]
        public void ProgressAdvancesActiveQuestsToReadyAndCapsAtRequired()
        {
            var log = new QuestLog(new[] { Defeat("q", 3) });
            log.Start("q");
            log.Record(ObjectiveKind.DefeatCreature, "Wolf");
            Assert.AreEqual(QuestStatus.Active, log.Get("q").Status);
            Assert.AreEqual(1, log.Get("q").Progress[0]);
            log.Record(ObjectiveKind.DefeatCreature, "Wolf", 9);   // overshoot
            Assert.AreEqual(3, log.Get("q").Progress[0]);          // capped
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, log.Get("q").Status);
        }

        [Test]
        public void NotStartedQuestsDoNotProgress()
        {
            var log = new QuestLog(new[] { Defeat("q", 1) });
            log.Record(ObjectiveKind.DefeatCreature, "Wolf"); // never started
            Assert.AreEqual(0, log.Get("q").Progress[0]);
            Assert.AreEqual(QuestStatus.NotStarted, log.Get("q").Status);
        }

        [Test]
        public void TargetedObjectiveIgnoresOtherTargets()
        {
            var log = new QuestLog(new[] { Defeat("q", 1, "Phoenix") });
            log.Start("q");
            log.Record(ObjectiveKind.DefeatCreature, "Wolf");     // wrong target
            Assert.AreEqual(0, log.Get("q").Progress[0]);
            log.Record(ObjectiveKind.DefeatCreature, "Phoenix");
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, log.Get("q").Status);
        }

        [Test]
        public void MultiObjectiveRequiresEveryObjective()
        {
            var def = new QuestDefinition("q", "T", "s", "Npc",
                new List<QuestObjective>
                {
                    new QuestObjective(ObjectiveKind.TalkToNpc, "Kiana", 1, "talk"),
                    new QuestObjective(ObjectiveKind.CollectCurrency, "Silver", 30, "gather"),
                },
                new QuestReward(Currency.Emerald, 1));
            var log = new QuestLog(new[] { def });
            log.Start("q");
            log.Record(ObjectiveKind.TalkToNpc, "Kiana");
            Assert.AreEqual(QuestStatus.Active, log.Get("q").Status);   // currency not yet gathered
            log.Record(ObjectiveKind.CollectCurrency, "Silver", 30);
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, log.Get("q").Status);
        }

        [Test]
        public void TurnInGrantsRewardOnceAndOnlyWhenReady()
        {
            var log = new QuestLog(new[] { Defeat("q", 1) });
            Assert.IsNull(log.TurnIn("q"));      // not ready (not started)
            log.Start("q");
            Assert.IsNull(log.TurnIn("q"));      // not ready (no progress)
            log.Record(ObjectiveKind.DefeatCreature, "Wolf");
            var reward = log.TurnIn("q");
            Assert.IsNotNull(reward);
            Assert.AreEqual(Currency.Silver, reward.Currency);
            Assert.AreEqual(QuestStatus.Completed, log.Get("q").Status);
            Assert.IsNull(log.TurnIn("q"));      // can't turn in twice
        }

        [Test]
        public void CollectItemObjectiveTracksItemIds()
        {
            var def = new QuestDefinition("q", "T", "s", "Npc",
                new List<QuestObjective> { new QuestObjective(ObjectiveKind.CollectItem, "hide", 3, "gather") },
                new QuestReward(Currency.Ruby, 1));
            var log = new QuestLog(new[] { def });
            log.Start("q");
            log.Record(ObjectiveKind.CollectItem, "ember_shard");   // wrong item
            Assert.AreEqual(0, log.Get("q").Progress[0]);
            log.Record(ObjectiveKind.CollectItem, "hide", 3);
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, log.Get("q").Status);
        }

        [Test]
        public void CatalogIsWellFormed()
        {
            Assert.Greater(QuestCatalog.All.Count, 0);
            var log = new QuestLog(QuestCatalog.All);
            foreach (var q in QuestCatalog.All)
            {
                Assert.IsNotNull(log.Get(q.Id), "duplicate or missing id: " + q.Id);
                Assert.Greater(q.Objectives.Count, 0, q.Id + " has no objectives");
                Assert.IsNotNull(q.Reward, q.Id + " has no reward");
            }
        }
    }
}
