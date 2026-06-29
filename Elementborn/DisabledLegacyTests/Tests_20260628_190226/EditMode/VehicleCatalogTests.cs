using NUnit.Framework;
using System;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class VehicleCatalogTests
    {
        [Test]
        public void EveryVehicleHasValidInfo()
        {
            foreach (VehicleKind k in Enum.GetValues(typeof(VehicleKind)))
            {
                var info = VehicleCatalog.For(k);
                Assert.IsFalse(string.IsNullOrEmpty(info.Name), $"{k} name");
                Assert.Greater(info.Price, 0, $"{k} price");
                Assert.GreaterOrEqual(info.Capacity, 1, $"{k} capacity");
            }
        }

        [Test]
        public void FlyingShipsAreElementLockedAndBoatsAreOpen()
        {
            Assert.AreEqual(Element.Fire, VehicleCatalog.For(VehicleKind.FireGalleon).RequiredElement);
            Assert.AreEqual(Element.Air, VehicleCatalog.For(VehicleKind.AirSkiff).RequiredElement);
            Assert.IsNull(VehicleCatalog.For(VehicleKind.Rowboat).RequiredElement);
            Assert.IsNull(VehicleCatalog.For(VehicleKind.Sailboat).RequiredElement);
        }

        [Test]
        public void VehicleLocomotionMatches()
        {
            Assert.AreEqual(LocomotionType.Flying, VehicleCatalog.For(VehicleKind.FireGalleon).Locomotion);
            Assert.AreEqual(LocomotionType.Water, VehicleCatalog.For(VehicleKind.Rowboat).Locomotion);
        }

        [Test]
        public void CreatureLocomotionMatches()
        {
            Assert.AreEqual(LocomotionType.Flying, Locomotion.For(CreatureKind.FireDragon));
            Assert.AreEqual(LocomotionType.Flying, Locomotion.For(CreatureKind.Phoenix));
            Assert.AreEqual(LocomotionType.Water, Locomotion.For(CreatureKind.Mermaid));
            Assert.AreEqual(LocomotionType.Ground, Locomotion.For(CreatureKind.Horse));
            Assert.AreEqual(LocomotionType.Ground, Locomotion.For(CreatureKind.Dog));
        }
    }
}
