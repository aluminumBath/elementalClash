using NUnit.Framework;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class SaveSerializationTests
    {
        [Test]
        public void RoundTripsScalarFields()
        {
            var d = new SaveData
            {
                silver = 40, ruby = 3, emerald = 2, sapphire = 1, diamond = 0,
                hasHouse = true, houseX = 1.5f, houseY = 2f, houseZ = -3.5f,
                playerElement = "Water", isConfluence = false,
            };

            var r = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(d));

            Assert.AreEqual(40, r.silver);
            Assert.AreEqual(3, r.ruby);
            Assert.AreEqual(2, r.emerald);
            Assert.AreEqual(1, r.sapphire);
            Assert.IsTrue(r.hasHouse);
            Assert.AreEqual(1.5f, r.houseX, 1e-4f);
            Assert.AreEqual(-3.5f, r.houseZ, 1e-4f);
            Assert.AreEqual("Water", r.playerElement);
            Assert.IsFalse(r.isConfluence);
        }

        [Test]
        public void RoundTripsLuresAndOwnedLists()
        {
            var d = new SaveData();
            d.lureKinds.Add("Horse");
            d.lureCounts.Add(2);
            d.ownedKinds.Add("Horse");
            d.ownedKinds.Add("WaterDragon");
            d.ownedVehicles.Add("Sailboat");

            var r = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(d));

            Assert.AreEqual("Horse", r.lureKinds[0]);
            Assert.AreEqual(2, r.lureCounts[0]);
            Assert.AreEqual(2, r.ownedKinds.Count);
            CollectionAssert.Contains(r.ownedKinds, "WaterDragon");
            CollectionAssert.Contains(r.ownedVehicles, "Sailboat");
        }
    }
}
