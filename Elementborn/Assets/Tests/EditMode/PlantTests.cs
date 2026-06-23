using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class PlantTests
    {
        private static ChannelerLoadout PlantUser() =>
            ChannelerLoadout.FromState(new[] { Element.Water, Element.Earth }, new[] { SubArt.Verdancy }, WeaponType.None);

        private static ChannelerLoadout NonPlantUser() => ChannelerLoadout.SingleElement(Element.Fire);

        private static ChannelerLoadout SteamHealer() =>
            ChannelerLoadout.FromState(new[] { Element.Water, Element.Fire }, new[] { SubArt.Steamcraft }, WeaponType.None);

        [Test]
        public void OnlyVerdancyLoadoutsArePlantUsers()
        {
            Assert.IsTrue(PlantControl.IsPlantUser(PlantUser()));
            Assert.IsFalse(PlantControl.IsPlantUser(NonPlantUser()));
            Assert.IsFalse(PlantControl.IsPlantUser(null));
        }

        [Test]
        public void PlantUsersAndSteamHealersCanTendLilies()
        {
            Assert.IsTrue(PlantControl.CanTendLily(PlantUser()));
            Assert.IsTrue(PlantControl.CanTendLily(SteamHealer())); // steam/healers bloom lilies too
            Assert.IsFalse(PlantControl.CanTendLily(NonPlantUser()));
            // but only plant users open gates / shrug off spores
            Assert.IsFalse(PlantControl.CanOpenGate(SteamHealer()));
            Assert.IsFalse(PlantControl.ResistsSpores(SteamHealer()));
        }

        [Test]
        public void PlantUsersOpenGatesAndResistSpores()
        {
            Assert.IsTrue(PlantControl.CanOpenGate(PlantUser()));
            Assert.IsTrue(PlantControl.ResistsSpores(PlantUser()));
            Assert.IsFalse(PlantControl.CanOpenGate(NonPlantUser()));
            Assert.IsFalse(PlantControl.ResistsSpores(NonPlantUser()));
        }

        [Test]
        public void CatalogFlagsMatchBehaviour()
        {
            Assert.IsTrue(PlantCatalog.For(PlantKind.Snaptrap).Hostile);           // attacks on its own
            Assert.IsFalse(PlantCatalog.For(PlantKind.Vine).Hostile);
            Assert.IsTrue(PlantCatalog.For(PlantKind.WillowGate).NeedsPlantUser);  // only a plant user opens it
            Assert.IsTrue(PlantCatalog.For(PlantKind.Spore).NeedsPlantUser);
            Assert.IsFalse(PlantCatalog.For(PlantKind.Heartfruit).NeedsPlantUser); // anyone can be healed
        }

        [Test]
        public void StatusClearActsAsACure()
        {
            var s = new StatusController();
            s.Add(new StatusEffect(StatusKind.Slow, 0.5f, 5f));
            s.Add(new StatusEffect(StatusKind.Burn, 4f, 5f));
            Assert.Greater(s.ActiveCount, 0);
            s.Clear();
            Assert.AreEqual(0, s.ActiveCount);
            Assert.AreEqual(1f, s.SpeedMultiplier, 0.001f); // back to unhindered
        }
    }
}
