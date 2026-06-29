using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ExperienceTests
    {
        [Test]
        public void TougherCreaturesAreWorthMore()
        {
            int dragon = Experience.ForCreature(320f, 25f);   // big, hits hard
            int cat = Experience.ForCreature(50f, 8f);        // small
            Assert.Greater(dragon, cat);
        }

        [Test]
        public void ThereIsAFloorForTrivialCreatures()
        {
            Assert.AreEqual(Experience.MinCreatureXp, Experience.ForCreature(0f, 0f));
            Assert.GreaterOrEqual(Experience.ForCreature(1f, 0f), Experience.MinCreatureXp);
        }

        [Test]
        public void ScalesWithBothHealthAndDamage()
        {
            int baseXp = Experience.ForCreature(100f, 10f);
            Assert.Greater(Experience.ForCreature(200f, 10f), baseXp);   // more health
            Assert.Greater(Experience.ForCreature(100f, 30f), baseXp);   // more damage
        }
    }
}
