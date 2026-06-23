using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class WeaponsTests
    {
        private static ChannelingIntent Cast(IntentType type) => new ChannelingIntent(type, Vector3.forward);

        [Test]
        public void Sword_PrimaryIsMelee()
        {
            var o = Weapons.Resolve(new WeaponInstance(WeaponType.Sword, WeaponMaterial.Metal), Cast(IntentType.PrimaryCast));
            Assert.AreEqual(OutcomeKind.Melee, o.Kind);
            Assert.Greater(o.Damage, 0f);
        }

        [Test]
        public void LongBow_PrimaryIsProjectile()
        {
            var o = Weapons.Resolve(new WeaponInstance(WeaponType.LongBow, WeaponMaterial.Wood), Cast(IntentType.PrimaryCast));
            Assert.AreEqual(OutcomeKind.Projectile, o.Kind);
            Assert.Greater(o.Speed, 0f);
        }

        [Test]
        public void Shield_DefendIsBarrier()
        {
            var o = Weapons.Resolve(new WeaponInstance(WeaponType.Shield, WeaponMaterial.Metal), Cast(IntentType.Defend));
            Assert.AreEqual(OutcomeKind.Barrier, o.Kind);
        }

        [Test]
        public void Metal_HitsHarderThanWood()
        {
            var wood = Weapons.Resolve(new WeaponInstance(WeaponType.Sword, WeaponMaterial.Wood), Cast(IntentType.PrimaryCast));
            var metal = Weapons.Resolve(new WeaponInstance(WeaponType.Sword, WeaponMaterial.Metal), Cast(IntentType.PrimaryCast));
            Assert.Greater(metal.Damage, wood.Damage);
        }

        [Test]
        public void Ice_SlowsOnHit()
        {
            var o = Weapons.Resolve(new WeaponInstance(WeaponType.Dagger, WeaponMaterial.Ice), Cast(IntentType.PrimaryCast));
            Assert.AreEqual(StatusKind.Slow, o.Status.Kind);
        }

        [Test]
        public void Hammer_KnocksBack()
        {
            var o = Weapons.Resolve(new WeaponInstance(WeaponType.Hammer, WeaponMaterial.Metal), Cast(IntentType.PrimaryCast));
            Assert.Greater(o.Knockback, 0f);
        }

        [Test]
        public void WoodBreaksToFire_NotWater()
        {
            var wood = new WeaponInstance(WeaponType.Sword, WeaponMaterial.Wood);
            Assert.IsTrue(Weapons.IsBrokenBy(wood, new DamageInfo(10f, Element.Fire)));
            Assert.IsFalse(Weapons.IsBrokenBy(wood, new DamageInfo(10f, Element.Water)));
        }

        [Test]
        public void MetalBreaksToOreshaping_NotPlainEarth()
        {
            var metal = new WeaponInstance(WeaponType.Hammer, WeaponMaterial.Metal);
            Assert.IsTrue(Weapons.IsBrokenBy(metal, new DamageInfo(10f, Element.Earth, AbilityVariant.Oreshaping)));
            Assert.IsFalse(Weapons.IsBrokenBy(metal, new DamageInfo(10f, Element.Earth, AbilityVariant.Standard)));
        }

        [Test]
        public void IceBreaksToWater_NotFire()
        {
            var ice = new WeaponInstance(WeaponType.Dagger, WeaponMaterial.Ice);
            Assert.IsTrue(Weapons.IsBrokenBy(ice, new DamageInfo(10f, Element.Water)));
            Assert.IsFalse(Weapons.IsBrokenBy(ice, new DamageInfo(10f, Element.Fire)));
        }
    }
}
