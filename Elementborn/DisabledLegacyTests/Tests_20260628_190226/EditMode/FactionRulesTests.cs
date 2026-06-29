using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class FactionRulesTests
    {
        private static Disposition D(Faction v, Element? ve, Faction t, Element? te, bool provoked = false)
            => FactionRules.Resolve(v, ve, t, te, provoked);

        [Test]
        public void WildToleratesSameElementChanneler()
        {
            Assert.AreEqual(Disposition.Neutral, D(Faction.Wild, Element.Fire, Faction.Player, Element.Fire));
        }

        [Test]
        public void WildIsHostileToOtherElementChanneler()
        {
            Assert.AreEqual(Disposition.Hostile, D(Faction.Wild, Element.Fire, Faction.Player, Element.Water));
        }

        [Test]
        public void WildIgnoresNonChannelerAndCivilians()
        {
            Assert.AreEqual(Disposition.Neutral, D(Faction.Wild, Element.Fire, Faction.Player, null));
            Assert.AreEqual(Disposition.Neutral, D(Faction.Wild, Element.Fire, Faction.Civilian, null));
        }

        [Test]
        public void AttackingOverridesTolerance()
        {
            // a same-element channeler who attacks a Wild becomes a target anyway
            Assert.AreEqual(Disposition.Hostile, D(Faction.Wild, Element.Fire, Faction.Player, Element.Fire, provoked: true));
        }

        [Test]
        public void BanditIsHostileToPlayerButSparesCivilians()
        {
            Assert.AreEqual(Disposition.Hostile, D(Faction.Bandit, null, Faction.Player, Element.Earth));
            Assert.AreEqual(Disposition.Neutral, D(Faction.Bandit, null, Faction.Civilian, null));
            Assert.AreEqual(Disposition.Neutral, D(Faction.Bandit, null, Faction.Bandit, null));
        }

        [Test]
        public void CivilianIsPeacefulUntilAttacked()
        {
            Assert.AreEqual(Disposition.Neutral, D(Faction.Civilian, null, Faction.Wild, Element.Fire));
            Assert.AreEqual(Disposition.Hostile, D(Faction.Civilian, null, Faction.Wild, Element.Fire, provoked: true));
        }

        [Test]
        public void AllyFightsHostilesAndLikesThePlayer()
        {
            Assert.AreEqual(Disposition.Hostile, D(Faction.Ally, Element.Water, Faction.Wild, Element.Fire));
            Assert.AreEqual(Disposition.Hostile, D(Faction.Ally, Element.Water, Faction.Bandit, null));
            Assert.AreEqual(Disposition.Friendly, D(Faction.Ally, Element.Water, Faction.Player, Element.Water));
        }

        [Test]
        public void PlayerNeverAutoAggros()
        {
            Assert.AreEqual(Disposition.Neutral, D(Faction.Player, Element.Fire, Faction.Wild, Element.Water));
            Assert.AreEqual(Disposition.Neutral, D(Faction.Player, Element.Fire, Faction.Bandit, null));
        }
    }
}
