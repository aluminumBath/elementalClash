using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class EquipmentTests
    {
        [Test]
        public void EquipPlacesGearInItsSlot()
        {
            var l = new EquipLoadout();
            Assert.IsTrue(l.Equip("tough_leather"));
            Assert.AreEqual("tough_leather", l.EquippedIn(EquipSlot.Chest));
            Assert.IsTrue(l.IsEquipped("tough_leather"));
        }

        [Test]
        public void EquippingTheSameSlotReplaces()
        {
            var l = new EquipLoadout();
            l.Equip("hide");          // Armor
            l.Equip("tough_leather"); // Armor again -> replaces
            Assert.AreEqual("tough_leather", l.EquippedIn(EquipSlot.Chest));
            Assert.IsFalse(l.IsEquipped("hide"));
        }

        [Test]
        public void NonGearCannotBeEquipped()
        {
            var l = new EquipLoadout();
            Assert.IsFalse(l.Equip("ember_shard"));
            Assert.IsFalse(l.Equip(null));
        }

        [Test]
        public void BonusesAggregateAcrossSlots()
        {
            var l = new EquipLoadout();
            l.Equip("tough_leather");   // +30 HP
            l.Equip("elemental_charm"); // +15 HP, +0.15 power
            l.Equip("old_relic");       // +0.10 power
            Assert.AreEqual(45, l.MaxHealthBonus);
            Assert.AreEqual(1.25f, l.OffenseMultiplier, 0.0001f);
        }

        [Test]
        public void UnequipDropsTheBonus()
        {
            var l = new EquipLoadout();
            l.Equip("elemental_charm");
            l.Unequip(EquipSlot.Charm);
            Assert.IsNull(l.EquippedIn(EquipSlot.Charm));
            Assert.AreEqual(0, l.MaxHealthBonus);
            Assert.AreEqual(1f, l.OffenseMultiplier, 0.0001f);
        }

        [Test]
        public void SaveLoadRoundTrips()
        {
            var l = new EquipLoadout();
            l.Equip("tough_leather");
            l.Equip("old_relic");
            var saved = l.ToSave();

            var loaded = new EquipLoadout();
            loaded.Load(saved);
            Assert.AreEqual("tough_leather", loaded.EquippedIn(EquipSlot.Chest));
            Assert.AreEqual("old_relic", loaded.EquippedIn(EquipSlot.Trinket));
            Assert.AreEqual(l.MaxHealthBonus, loaded.MaxHealthBonus);
            Assert.AreEqual(l.OffenseMultiplier, loaded.OffenseMultiplier, 0.0001f);
        }

        [Test]
        public void EveryGearItemIsARealCatalogItem()
        {
            foreach (var g in GearCatalog.All)
                Assert.IsTrue(ItemCatalog.Exists(g.ItemId), g.ItemId + " must exist in the catalog");
        }
    }
}
