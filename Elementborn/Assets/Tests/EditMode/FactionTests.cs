using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class FactionTests
    {
        [Test]
        public void EveryJoinableFactionHasAProfile()
        {
            Assert.AreEqual(4, FactionCatalog.Joinable.Length);
            foreach (var id in FactionCatalog.Joinable)
            {
                var p = FactionCatalog.For(id);
                Assert.AreEqual(id, p.Id);
                Assert.IsFalse(string.IsNullOrEmpty(p.Name));
                Assert.IsFalse(string.IsNullOrEmpty(p.Creed));
            }
        }

        [Test]
        public void PerksEncodeStrengthsAndWeaknesses()
        {
            var sep = FactionCatalog.For(FactionId.Separatists).Perk;
            var sym = FactionCatalog.For(FactionId.Symbiasts).Perk;
            var cle = FactionCatalog.For(FactionId.Cleicists).Perk;
            Assert.Greater(sep.OffenseMultiplier, sym.OffenseMultiplier); // separatists hit harder
            Assert.Less(sep.DefenseMultiplier, 1f);                       // but are brittle
            Assert.Greater(cle.DefenseMultiplier, sep.DefenseMultiplier); // cleicists are stout
        }

        [Test]
        public void AttitudesFollowDoctrine()
        {
            // toward a Confluence (all-element) person
            Assert.AreEqual(Attitude.Revered, FactionAttitudes.Toward(FactionId.Symbiasts, true, false));
            Assert.AreEqual(Attitude.Hostile, FactionAttitudes.Toward(FactionId.Cleicists, true, false));
            Assert.AreEqual(Attitude.Hostile, FactionAttitudes.Toward(FactionId.Separatists, true, false));

            // toward a mixed-gift (sub-art) person
            Assert.AreEqual(Attitude.Unfriendly, FactionAttitudes.Toward(FactionId.Cleicists, false, true));

            // toward a plain single-element user
            Assert.AreEqual(Attitude.Friendly, FactionAttitudes.Toward(FactionId.Symbiasts, false, false));
            Assert.AreEqual(Attitude.Neutral, FactionAttitudes.Toward(FactionId.Synodists, true, true));
        }

        [Test]
        public void DoctrineMapsToAttitude()
        {
            // a faction's doctrine drives how it greets a Confluence or mixed-gift person (used for modded factions)
            Assert.AreEqual(Attitude.Hostile, FactionAttitudes.FromDoctrine(Doctrine.Abhors, Doctrine.Accepts, true, false));
            Assert.AreEqual(Attitude.Revered, FactionAttitudes.FromDoctrine(Doctrine.Reveres, Doctrine.Accepts, true, false));
            Assert.AreEqual(Attitude.Unfriendly, FactionAttitudes.FromDoctrine(Doctrine.Accepts, Doctrine.Dislikes, false, true));
            Assert.AreEqual(Attitude.Neutral, FactionAttitudes.FromDoctrine(Doctrine.Reveres, Doctrine.Abhors, false, false));
        }
    }
}
