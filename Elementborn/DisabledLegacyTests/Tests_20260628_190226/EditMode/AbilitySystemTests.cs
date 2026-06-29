using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class AbilitySystemTests
    {
        private static AbilitySystem Of(Element e, bool subArt = false) => new AbilitySystem(
            subArt ? ChannelerLoadout.ElementWithSubArt(e) : ChannelerLoadout.SingleElement(e));
        private static ChannelingIntent Cast(IntentType type, float charge = 0f) =>
            new ChannelingIntent(type, Vector3.forward, charge);

        // Fire ----------------------------------------------------------------------
        [Test]
        public void FirePrimary_ProducesProjectile()
        {
            var o = Of(Element.Fire).Resolve(Cast(IntentType.PrimaryCast));
            Assert.AreEqual(OutcomeKind.Projectile, o.Kind);
            Assert.AreEqual(AbilitySystem.FireBaseDamage, o.Damage, 0.001f);
        }

        [Test]
        public void Lava_BoostsDamageAndIgnites()
        {
            var lava = Of(Element.Fire, subArt: true).Resolve(Cast(IntentType.PrimaryCast, 0.5f));
            Assert.AreEqual(AbilityVariant.Magmacraft, lava.Variant);
            Assert.AreEqual(StatusKind.Burn, lava.Status.Kind);
        }

        [Test]
        public void FireSecondary_IsLightning_WithStun()
        {
            var o = Of(Element.Fire).Resolve(Cast(IntentType.SecondaryCast, 0.5f));
            Assert.AreEqual(AbilityVariant.Lightning, o.Variant);
            Assert.AreEqual(StatusKind.Stun, o.Status.Kind);
        }

        // Water + Sanguine Grip ------------------------------------------------------
        [Test]
        public void WaterSecondary_IsIce_WithSlow()
        {
            var o = Of(Element.Water).Resolve(Cast(IntentType.SecondaryCast, 0.5f));
            Assert.AreEqual(AbilityVariant.Ice, o.Variant);
            Assert.AreEqual(StatusKind.Slow, o.Status.Kind);
        }

        [Test]
        public void SanguineGrip_ReplacesIce_WithControlGrip()
        {
            var o = Of(Element.Water, subArt: true).Resolve(Cast(IntentType.SecondaryCast, 0.5f));
            Assert.AreEqual(OutcomeKind.Control, o.Kind);
            Assert.AreEqual(AbilityVariant.SanguineGrip, o.Variant);
            Assert.AreEqual(StatusKind.Control, o.Status.Kind);
            Assert.Greater(o.Knockback, 0f); // can fling the gripped target
        }

        // Earth + Metal -------------------------------------------------------------
        [Test]
        public void EarthSecondary_Boulder_KnocksBack()
        {
            var o = Of(Element.Earth).Resolve(Cast(IntentType.SecondaryCast, 0.5f));
            Assert.Greater(o.Knockback, 0f);
        }

        [Test]
        public void Metal_BoostsEarthDamage()
        {
            var plain = Of(Element.Earth).Resolve(Cast(IntentType.PrimaryCast, 0.5f));
            var metal = Of(Element.Earth, subArt: true).Resolve(Cast(IntentType.PrimaryCast, 0.5f));
            Assert.Greater(metal.Damage, plain.Damage);
            Assert.AreEqual(AbilityVariant.Oreshaping, metal.Variant);
        }

        // Air + Flight glide --------------------------------------------------------
        [Test]
        public void AirPrimary_KnocksBack_LowDamage()
        {
            var o = Of(Element.Air).Resolve(Cast(IntentType.PrimaryCast));
            Assert.Greater(o.Knockback, 0f);
            Assert.Less(o.Damage, AbilitySystem.FireBaseDamage);
        }

        [Test]
        public void AirDash_IsQuickDash_ButFlightGlides()
        {
            var dash = Of(Element.Air).Resolve(Cast(IntentType.Dash));
            var glide = Of(Element.Air, subArt: true).Resolve(Cast(IntentType.Dash));

            Assert.AreEqual(OutcomeKind.Movement, dash.Kind);
            Assert.AreEqual(AbilityVariant.Standard, dash.Variant);
            Assert.AreEqual(AbilityVariant.Flight, glide.Variant);
            Assert.Greater(glide.Speed, dash.Speed); // flight covers more ground
        }

        // Shared --------------------------------------------------------------------
        [Test]
        public void NoneIntent_IsEmpty() => Assert.IsTrue(Of(Element.Fire).Resolve(ChannelingIntent.None).IsEmpty);
    }
}
