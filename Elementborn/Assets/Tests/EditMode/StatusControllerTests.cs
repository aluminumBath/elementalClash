using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class StatusControllerTests
    {
        [Test]
        public void Slow_ReducesSpeed_ThenExpires()
        {
            var sc = new StatusController();
            sc.Add(new StatusEffect(StatusKind.Slow, 0.5f, 1f));
            Assert.AreEqual(0.5f, sc.SpeedMultiplier, 0.001f);
            sc.Tick(1.1f);
            Assert.AreEqual(1f, sc.SpeedMultiplier, 0.001f);
            Assert.AreEqual(0, sc.ActiveCount);
        }

        [Test]
        public void Stun_StopsMovement_ThenClears()
        {
            var sc = new StatusController();
            sc.Add(new StatusEffect(StatusKind.Stun, 1f, 0.5f));
            Assert.IsTrue(sc.IsStunned);
            Assert.AreEqual(0f, sc.SpeedMultiplier, 0.001f);
            sc.Tick(0.6f);
            Assert.IsFalse(sc.IsStunned);
        }

        [Test]
        public void Control_Immobilizes_AndFlagsControlled()
        {
            var sc = new StatusController();
            sc.Add(new StatusEffect(StatusKind.Control, 1f, 2f));
            Assert.IsTrue(sc.IsControlled);
            Assert.AreEqual(0f, sc.SpeedMultiplier, 0.001f);
            sc.Tick(2.1f);
            Assert.IsFalse(sc.IsControlled);
            Assert.AreEqual(1f, sc.SpeedMultiplier, 0.001f);
        }

        [Test]
        public void TwoSlows_TakeTheStronger()
        {
            var sc = new StatusController();
            sc.Add(new StatusEffect(StatusKind.Slow, 0.3f, 2f));
            sc.Add(new StatusEffect(StatusKind.Slow, 0.7f, 2f));
            Assert.AreEqual(0.3f, sc.SpeedMultiplier, 0.001f);
        }

        [Test]
        public void Burn_ReportsDamagePerSecond()
        {
            var sc = new StatusController();
            sc.Add(new StatusEffect(StatusKind.Burn, 6f, 3f));
            Assert.AreEqual(6f, sc.BurnDamagePerSecond, 0.001f);
        }
    }
}
