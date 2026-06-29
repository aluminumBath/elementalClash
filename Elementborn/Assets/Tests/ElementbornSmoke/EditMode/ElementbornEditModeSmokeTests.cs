using System;
using System.Linq;
using NUnit.Framework;

namespace Elementborn.Tests.EditMode
{
    public sealed class ElementbornEditModeSmokeTests
    {
        [Test]
        public void TestAssembly_IsDiscoveredByUnityTestRunner()
        {
            Assert.That(true, Is.True);
        }

        [Test]
        public void RuntimeAssembly_ContainsCriticalElementbornTypes()
        {
            Assert.That(FindType("Elementborn.Game.ElementbornRuntimeBootstrap"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Game.PlayerAttunementHud"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Game.BoatController"), Is.Not.Null);
        }

        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType(fullName, false))
                .FirstOrDefault(type => type != null);
        }
    }
}
