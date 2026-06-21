using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class KnockbackControllerTests
    {
        [Test]
        public void StartsAtRest()
        {
            var kb = new KnockbackController();
            Assert.IsFalse(kb.IsActive);
            Assert.AreEqual(Vector3.zero, kb.Velocity);
        }

        [Test]
        public void Add_SetsVelocity()
        {
            var kb = new KnockbackController();
            kb.Add(new Vector3(10f, 0f, 0f));
            Assert.IsTrue(kb.IsActive);
            Assert.AreEqual(10f, kb.Velocity.x, 0.001f);
        }

        [Test]
        public void Tick_DecaysToRest()
        {
            var kb = new KnockbackController { Damping = 6f };
            kb.Add(new Vector3(10f, 0f, 0f));
            kb.Tick(1f); // damping * dt clamps to 1 -> fully decayed
            Assert.IsFalse(kb.IsActive);
        }
    }
}
