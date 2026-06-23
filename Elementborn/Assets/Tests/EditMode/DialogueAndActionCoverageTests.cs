using System;
using System.Collections.Generic;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    /// <summary>
    /// Completeness coverage for the things NPCs, the player, and the world actually *say and do* — so a new
    /// guide, quest, or creature can't ship with an empty greeting, a missing finding-hint, or a broken reward.
    /// (Enemy stats/actions are covered by EnemyArchetypesTests; the player's combat actions by the ability /
    /// sweep / heavy moveset tests. This fills the dialogue + reward gaps.)
    /// </summary>
    public class DialogueAndActionCoverageTests
    {
        private static IEnumerable<GuideNpcId> Guides => (GuideNpcId[])Enum.GetValues(typeof(GuideNpcId));
        private static IEnumerable<CreatureKind> AllCreatures => (CreatureKind[])Enum.GetValues(typeof(CreatureKind));

        // ---- NPC dialogue & profile ----

        [Test]
        public void EveryGuideHasAFullSpokenProfile()
        {
            foreach (var id in Guides)
            {
                var n = NpcCatalog.For(id);
                Assert.IsFalse(string.IsNullOrWhiteSpace(n.Name), $"{id} name");
                Assert.IsFalse(string.IsNullOrWhiteSpace(n.Greeting), $"{id} greeting");
                Assert.IsFalse(string.IsNullOrWhiteSpace(n.Home), $"{id} home");
                Assert.IsFalse(string.IsNullOrWhiteSpace(n.Sidekick), $"{id} sidekick");
                Assert.IsFalse(string.IsNullOrWhiteSpace(n.Appearance), $"{id} appearance");
                Assert.IsTrue(Enum.IsDefined(typeof(Element), n.Element), $"{id} element");

                // The Keeper/Locator give a standing service line; the CreatureFinder composes hints live instead.
                if (n.Role != NpcRole.CreatureFinder)
                    Assert.IsFalse(string.IsNullOrWhiteSpace(n.ServiceLine), $"{id} service line");
            }
        }

        [Test]
        public void EveryNpcRoleIsRepresentedByAGuide()
        {
            var roles = new HashSet<NpcRole>();
            foreach (var id in Guides) roles.Add(NpcCatalog.For(id).Role);
            foreach (NpcRole role in Enum.GetValues(typeof(NpcRole)))
                Assert.IsTrue(roles.Contains(role), $"no guide fills the {role} role");
        }

        // ---- The CreatureFinder's live "dialogue": a finding hint for every creature ----

        [Test]
        public void EveryCreatureHasAFindingHint()
        {
            foreach (var kind in AllCreatures)
                Assert.IsFalse(string.IsNullOrWhiteSpace(CreatureHints.WhereToFind(kind)), $"{kind} hint");
        }

        // ---- Quest dialogue & rewards ----

        [Test]
        public void EveryQuestIsFullyAuthored()
        {
            Assert.Greater(QuestCatalog.All.Count, 0, "there should be starter quests");
            foreach (var q in QuestCatalog.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(q.Id), "quest id");
                Assert.IsFalse(string.IsNullOrWhiteSpace(q.Title), $"{q.Id} title");
                Assert.IsFalse(string.IsNullOrWhiteSpace(q.Summary), $"{q.Id} summary");
                Assert.IsTrue(Enum.TryParse(q.GiverNpcId, out GuideNpcId _), $"{q.Id} giver '{q.GiverNpcId}' is a real guide");

                Assert.Greater(q.Objectives.Count, 0, $"{q.Id} has objectives");
                foreach (var o in q.Objectives)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(o.Description), $"{q.Id} objective text");
                    Assert.Greater(o.Required, 0, $"{q.Id} objective count");
                }

                Assert.Greater(q.Reward.Amount, 0, $"{q.Id} reward amount");
            }
        }
    }
}
