
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class QuestChainDirectorTests
    {
        [SetUp]
        public void SetUp() => ElementbornEditModeTestUtility.ResetAll();

        [TearDown]
        public void TearDown() => ElementbornEditModeTestUtility.ResetAll();

        [Test]
        public void StartAndCompleteStages_AdvancesQuestChain()
        {
            var chain = ScriptableObject.CreateInstance<QuestChainDefinition>();
            ElementbornEditModeTestUtility.SetPrivate(chain, "chainId", "fire_chain");
            ElementbornEditModeTestUtility.SetPrivate(chain, "displayName", "Fire Chain");
            ElementbornEditModeTestUtility.SetPrivate(chain, "primaryCapital", CapitalId.FireCapital);
            ElementbornEditModeTestUtility.SetPrivate(chain, "summary", "A test quest chain.");

            var stageA = new QuestChainStageDefinition
            {
                StageId = "stage_a",
                Title = "Stage A",
                Summary = "First stage.",
                DefaultNextStageId = "stage_b"
            };
            var stageB = new QuestChainStageDefinition
            {
                StageId = "stage_b",
                Title = "Stage B",
                Summary = "Second stage."
            };
            ElementbornEditModeTestUtility.SetPrivate(chain, "stages", new List<QuestChainStageDefinition> { stageA, stageB });

            QuestChainDirector director = new GameObject("QuestChainDirector").AddComponent<QuestChainDirector>();
            director.SetQuestChains(new List<QuestChainDefinition> { chain });
            Assert.IsTrue(director.StartChain("fire_chain"));
            Assert.AreEqual("stage_a", director.GetOrCreateRecord("fire_chain").ActiveStageId);

            Assert.IsTrue(director.CompleteStage("fire_chain", "stage_a"));
            Assert.AreEqual("stage_b", director.GetOrCreateRecord("fire_chain").ActiveStageId);

            Assert.IsTrue(director.CompleteStage("fire_chain", "stage_b"));
            Assert.IsTrue(director.GetOrCreateRecord("fire_chain").Completed);
        }
    }
}
