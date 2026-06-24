using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class MountAbilitiesTests
    {
        [Test] public void GroundCharges()  => Assert.AreEqual(MountSkill.Charge,   MountAbilities.ForLocomotion(LocomotionType.Ground));
        [Test] public void WaterSurges()    => Assert.AreEqual(MountSkill.Surge,    MountAbilities.ForLocomotion(LocomotionType.Water));
        [Test] public void FlyersDivebomb() => Assert.AreEqual(MountSkill.Divebomb, MountAbilities.ForLocomotion(LocomotionType.Flying));

        [Test]
        public void ChargeAndDivebombImpactButSurgeDoesNot()
        {
            Assert.IsTrue(MountAbilities.HasImpact(MountSkill.Charge));
            Assert.IsTrue(MountAbilities.HasImpact(MountSkill.Divebomb));
            Assert.IsFalse(MountAbilities.HasImpact(MountSkill.Surge));
        }

        [Test]
        public void NoneIsInert()
        {
            Assert.AreEqual(MountSkill.None, MountAbilities.ForLocomotion((LocomotionType)999));
            Assert.AreEqual(0f, MountAbilities.Cooldown(MountSkill.None));
            Assert.AreEqual(0f, MountAbilities.BurstSpeed(MountSkill.None));
            Assert.AreEqual(0f, MountAbilities.BurstDuration(MountSkill.None));
        }

        [Test]
        public void ActiveSkillsHaveCooldownAndBurst()
        {
            Assert.Greater(MountAbilities.Cooldown(MountSkill.Charge), 0f);
            Assert.Greater(MountAbilities.BurstSpeed(MountSkill.Divebomb), 0f);
            Assert.Greater(MountAbilities.BurstDuration(MountSkill.Surge), 0f);
        }
    }
}
