using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ArenaTests
    {
        [Test]
        public void WavesEscalateAndDangerStaysInRange()
        {
            int prevCount = 0, prevDanger = 0;
            for (int w = 1; w <= ArenaProgression.TotalWaves; w++)
            {
                var p = ArenaProgression.For(w);
                Assert.GreaterOrEqual(p.EnemyCount, prevCount);   // never fewer enemies
                Assert.GreaterOrEqual(p.DangerLevel, prevDanger); // never easier
                Assert.GreaterOrEqual(p.DangerLevel, 1);
                Assert.LessOrEqual(p.DangerLevel, 5);
                prevCount = p.EnemyCount;
                prevDanger = p.DangerLevel;
            }
            Assert.AreEqual(4, ArenaProgression.For(1).EnemyCount);
            Assert.AreEqual(11, ArenaProgression.For(8).EnemyCount);
            Assert.AreEqual(5, ArenaProgression.For(8).DangerLevel);
        }

        [Test]
        public void StaminaSpendsRegensAndGates()
        {
            var s = new StaminaModel(100f, 20f);
            Assert.AreEqual(1f, s.Current01, 0.001f);

            Assert.IsTrue(s.TrySpend(30f));
            Assert.AreEqual(70f, s.Current, 0.001f);

            Assert.IsFalse(s.TrySpend(1000f));        // can't afford
            Assert.AreEqual(70f, s.Current, 0.001f);  // and nothing was spent

            s.Tick(1f);
            Assert.AreEqual(90f, s.Current, 0.001f);  // +20/s
            s.Tick(10f);
            Assert.AreEqual(100f, s.Current, 0.001f); // capped at max

            Assert.IsTrue(s.TrySpend(0f));            // free actions always succeed
        }

        [Test]
        public void StaminaCostsRankByCommitment()
        {
            Assert.AreEqual(0f, StaminaCost.For(IntentType.Defend));
            Assert.Greater(StaminaCost.For(IntentType.Heavy), StaminaCost.For(IntentType.PrimaryCast));
            Assert.Greater(StaminaCost.For(IntentType.SecondaryCast), StaminaCost.For(IntentType.PrimaryCast));
        }
    }
}
