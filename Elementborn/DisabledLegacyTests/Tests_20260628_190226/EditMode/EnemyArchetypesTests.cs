using NUnit.Framework;
using System;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class EnemyArchetypesTests
    {
        [Test]
        public void EveryKindHasPositiveStats()
        {
            foreach (EnemyKind k in Enum.GetValues(typeof(EnemyKind)))
            {
                var s = EnemyArchetypes.For(k);
                Assert.Greater(s.MaxHealth, 0f, $"{k} health");
                Assert.Greater(s.MoveSpeed, 0f, $"{k} speed");
                Assert.Greater(s.Damage, 0f, $"{k} damage");
                Assert.Greater(s.AttackRange, 0f, $"{k} range");
                Assert.Greater(s.AttackCooldown, 0f, $"{k} cooldown");
                Assert.Greater(s.ScoreValue, 0, $"{k} score");
            }
        }

        [Test]
        public void SelectorIsDeterministic()
        {
            var a = EnemySelector.Pick(3, BiomeType.Plains, new SystemRandomSource(123));
            var b = EnemySelector.Pick(3, BiomeType.Plains, new SystemRandomSource(123));
            Assert.AreEqual(a, b);
        }

        [Test]
        public void LowDangerPlainsNeverSpawnsBruteOrElementalist()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var k = EnemySelector.Pick(1, BiomeType.Plains, new SystemRandomSource(seed));
                Assert.AreNotEqual(EnemyKind.Brute, k);
                Assert.AreNotEqual(EnemyKind.Elementalist, k);
            }
        }

        [Test]
        public void HighDangerVolcanoCanSpawnElementalist()
        {
            bool any = false;
            for (int seed = 0; seed < 200 && !any; seed++)
                if (EnemySelector.Pick(5, BiomeType.Volcano, new SystemRandomSource(seed)) == EnemyKind.Elementalist)
                    any = true;
            Assert.IsTrue(any);
        }

        [Test]
        public void SelectorAlwaysReturnsDefinedKind()
        {
            foreach (BiomeType biome in Enum.GetValues(typeof(BiomeType)))
                for (int danger = 1; danger <= 5; danger++)
                {
                    var k = EnemySelector.Pick(danger, biome, new SystemRandomSource(danger * 31 + (int)biome));
                    Assert.IsTrue(Enum.IsDefined(typeof(EnemyKind), k));
                }
        }
    }
}
