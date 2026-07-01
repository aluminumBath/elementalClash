using System;
using System.Linq;
using NUnit.Framework;

namespace Elementborn.Tests.EditMode
{
    /// <summary>
    /// Reflection-only smoke coverage for the hidden-landmark work (Slices A–D): the landmark catalog, the access
    /// gate, the water-breathing boon, the Grimoire Locations section, and the Tideglass Draught consumable. Uses
    /// string type lookups (no direct Core/Game references) so it builds under the references-free test asmdef.
    /// </summary>
    public sealed class HiddenLandmarkSmokeTests
    {
        [Test]
        public void CoreAssembly_ContainsHiddenLandmarkTypes()
        {
            Assert.That(FindType("Elementborn.Core.Landmark"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Core.LandmarkAccess"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Core.LandmarkCatalog"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Core.LandmarkInfo"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Core.LandmarkAccessGate"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Core.LandmarkApproach"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Core.AccessResult"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Core.WaterBreathingBoon"), Is.Not.Null);
        }

        [Test]
        public void LandmarkEnum_HasTheFourHiddenLocations()
        {
            var t = FindType("Elementborn.Core.Landmark");
            Assert.That(t, Is.Not.Null);
            var names = Enum.GetNames(t);
            Assert.That(names.Length, Is.EqualTo(4));
            CollectionAssert.Contains(names, "ThalenVeyr");
            CollectionAssert.Contains(names, "AshwindAtoll");
            CollectionAssert.Contains(names, "Ilyrath");
            CollectionAssert.Contains(names, "TidecallerVillage");
        }

        [Test]
        public void GrimoireSection_HasLocations()
        {
            var t = FindType("Elementborn.Core.GrimoireSection");
            Assert.That(t, Is.Not.Null);
            CollectionAssert.Contains(Enum.GetNames(t), "Locations");
        }

        [Test]
        public void GameAssembly_ContainsLandmarkPortalAndPlacer()
        {
            Assert.That(FindType("Elementborn.Game.LandmarkPortal"), Is.Not.Null);
            Assert.That(FindType("Elementborn.Game.LandmarkPortalPlacer"), Is.Not.Null);
        }

        [Test]
        public void Consumables_KnowsTideglassDraught()
        {
            var t = FindType("Elementborn.Core.Consumables");
            Assert.That(t, Is.Not.Null);
            var m = t.GetMethod("IsConsumable", new[] { typeof(string) });
            Assert.That(m, Is.Not.Null);
            var result = (bool)m.Invoke(null, new object[] { "tideglass_draught" });
            Assert.That(result, Is.True);
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
