using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class MixAttackTests
    {
        [Test]
        public void ExoticsAndStormBirdsHaveMixAttacks()
        {
            CreatureKind[] withAttacks =
            {
                CreatureKind.Ridgewing, CreatureKind.Glidewisp, CreatureKind.Skytyrant, CreatureKind.Goldkoi,
                CreatureKind.Direstalker, CreatureKind.Skimfin, CreatureKind.Gillcloak, CreatureKind.Tidewarden,
                CreatureKind.Roc, CreatureKind.Thunderbird
            };
            foreach (var k in withAttacks)
            {
                var a = CreatureMixAttacks.For(k);
                Assert.IsFalse(a.IsNone, $"{k} should have a mix attack");
                Assert.AreNotEqual(a.Primary, a.Secondary, $"{k}'s attack should blend two different elements");
                Assert.Greater(a.Damage, 0f);
            }
        }

        [Test]
        public void OrdinaryWildlifeHasNoMixAttack()
        {
            Assert.IsTrue(CreatureMixAttacks.For(CreatureKind.Crab).IsNone);
            Assert.IsTrue(CreatureMixAttacks.For(CreatureKind.Horse).IsNone);
        }

        [Test]
        public void ToOutcomeProducesAUsableProjectile()
        {
            var a = CreatureMixAttacks.For(CreatureKind.Skytyrant);
            var outcome = a.ToOutcome(Vector3.forward);
            Assert.AreEqual(OutcomeKind.Projectile, outcome.Kind);
            Assert.AreEqual(a.Primary, outcome.Element);
            Assert.AreEqual(a.Damage, outcome.Damage, 0.001f);
            Assert.IsFalse(outcome.IsEmpty);
        }
    }
}
