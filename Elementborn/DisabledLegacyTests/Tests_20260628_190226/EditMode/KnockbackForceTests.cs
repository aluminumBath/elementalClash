using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class KnockbackForceTests
    {
        [Test]
        public void ResolveSplitsHorizontalAndPop()
        {
            KnockbackForce.Resolve(10f, out float h, out float v);
            Assert.AreEqual(10f, h, 1e-5f);
            Assert.AreEqual(10f * KnockbackForce.PopRatio, v, 1e-5f);
        }

        [Test]
        public void ResolveClampsToCeiling()
        {
            KnockbackForce.Resolve(999f, out float h, out float v);
            Assert.AreEqual(KnockbackForce.MaxHorizontal, h, 1e-5f);
            Assert.AreEqual(KnockbackForce.MaxHorizontal * KnockbackForce.PopRatio, v, 1e-5f);
        }

        [Test]
        public void ResolveFloorsNegativeToZero()
        {
            KnockbackForce.Resolve(-7f, out float h, out float v);
            Assert.AreEqual(0f, h, 1e-5f);
            Assert.AreEqual(0f, v, 1e-5f);
        }
    }
}
