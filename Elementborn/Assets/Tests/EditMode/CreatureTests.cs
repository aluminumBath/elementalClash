using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class CreatureTests
    {
        [Test]
        public void WanderTurnsBackWhenBeyondRadius()
        {
            // Heading further out from home, no random nudge: steering should point back toward home.
            var home = Vector3.zero;
            var pos = new Vector3(20f, 0f, 0f);
            var dir = AmbientWander.Steer(pos, Vector3.right, home, 8f, Vector3.zero);
            Assert.Less(dir.x, 0f, "should steer back toward home");
            Assert.AreEqual(1f, dir.magnitude, 0.01f);
        }

        [Test]
        public void WanderStaysNormalizedInsideRadius()
        {
            var dir = AmbientWander.Steer(Vector3.zero, Vector3.forward, Vector3.zero, 8f, Vector3.zero);
            Assert.AreEqual(1f, dir.magnitude, 0.01f);
        }

        [Test]
        public void BossPhasesEscalateWithLostHealth()
        {
            Assert.AreEqual(BossPhase.Calm, BossPhases.For(0.9f));
            Assert.AreEqual(BossPhase.Aggressive, BossPhases.For(0.5f));
            Assert.AreEqual(BossPhase.Frenzy, BossPhases.For(0.1f));
        }

        [Test]
        public void BossAttacksFasterInLaterPhases()
        {
            float calm = BossPhases.AttackInterval(BossPhase.Calm, 2f);
            float aggressive = BossPhases.AttackInterval(BossPhase.Aggressive, 2f);
            float frenzy = BossPhases.AttackInterval(BossPhase.Frenzy, 2f);
            Assert.Greater(calm, aggressive);
            Assert.Greater(aggressive, frenzy);
        }

        [Test]
        public void NewWildlifeUseTheRightLocomotion()
        {
            Assert.AreEqual(LocomotionType.Water, Locomotion.For(CreatureKind.Eel));
            Assert.AreEqual(LocomotionType.Flying, Locomotion.For(CreatureKind.Roc));
            Assert.AreEqual(LocomotionType.Flying, Locomotion.For(CreatureKind.Thunderbird));
            Assert.AreEqual(LocomotionType.Ground, Locomotion.For(CreatureKind.Tiger));
            Assert.AreEqual(LocomotionType.Ground, Locomotion.For(CreatureKind.Crab));
        }

        [Test]
        public void ExoticCreaturesUseTheRightLocomotion()
        {
            Assert.AreEqual(LocomotionType.Flying, Locomotion.For(CreatureKind.Skytyrant));
            Assert.AreEqual(LocomotionType.Flying, Locomotion.For(CreatureKind.Ridgewing));
            Assert.AreEqual(LocomotionType.Water, Locomotion.For(CreatureKind.Tidewarden));
            Assert.AreEqual(LocomotionType.Water, Locomotion.For(CreatureKind.Skimfin));
            Assert.AreEqual(LocomotionType.Ground, Locomotion.For(CreatureKind.Direstalker));
        }

        [Test]
        public void ExoticCreaturesAreTameableButHard()
        {
            // tameable (a chance > 0) yet much stubborner than common wildlife
            float skytyrant = CreatureCatalog.For(CreatureKind.Skytyrant).TameChance;
            float crab = CreatureCatalog.For(CreatureKind.Crab).TameChance;
            Assert.Greater(skytyrant, 0f);
            Assert.Less(skytyrant, crab);
            Assert.IsTrue(CreatureCatalog.For(CreatureKind.Ridgewing).Rideable);
        }
    }
}
