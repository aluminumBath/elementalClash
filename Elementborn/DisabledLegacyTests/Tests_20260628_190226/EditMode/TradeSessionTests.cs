using NUnit.Framework;
using Elementborn.Core.Social;

namespace Elementborn.Tests
{
    public class TradeSessionTests
    {
        private static TradeSession New() => new TradeSession("a", "b");

        [Test]
        public void New_StartsBuilding_Empty()
        {
            var t = New();
            Assert.AreEqual(TradePhase.Building, t.Phase);
            Assert.IsTrue(t.OfferA.IsEmpty);
            Assert.IsTrue(t.OfferB.IsEmpty);
        }

        [Test]
        public void SetOffer_OnlyParties_AndZeroRemoves()
        {
            var t = New();
            Assert.IsFalse(t.SetOffer("stranger", "ember_shard", 2));
            Assert.IsTrue(t.SetOffer("a", "ember_shard", 2));
            Assert.AreEqual(2, t.OfferA.Lines["ember_shard"]);
            Assert.IsTrue(t.SetOffer("a", "ember_shard", 0));
            Assert.IsTrue(t.OfferA.IsEmpty);
        }

        [Test]
        public void BothLock_ReachesLocked()
        {
            var t = New();
            t.SetOffer("a", "hide", 1);
            t.Lock("a");
            Assert.AreEqual(TradePhase.Building, t.Phase); // waiting on b
            t.Lock("b");
            Assert.AreEqual(TradePhase.Locked, t.Phase);
        }

        [Test]
        public void Confirm_RequiresBothLocked()
        {
            var t = New();
            t.Lock("a");
            Assert.IsFalse(t.Confirm("a")); // not locked jointly yet
            t.Lock("b");
            Assert.IsTrue(t.Confirm("a"));
            Assert.AreEqual(TradePhase.Locked, t.Phase); // still waiting on b's confirm
            Assert.IsTrue(t.Confirm("b"));
            Assert.AreEqual(TradePhase.Completed, t.Phase);
        }

        [Test]
        public void EditingAfterLock_VoidsBothLocks()
        {
            var t = New();
            t.Lock("a");
            t.Lock("b");
            Assert.AreEqual(TradePhase.Locked, t.Phase);
            t.SetOffer("a", "river_pearl", 1); // sneaky change after both locked
            Assert.AreEqual(TradePhase.Building, t.Phase);
            Assert.IsFalse(t.IsLocked("a"));
            Assert.IsFalse(t.IsLocked("b"));
        }

        [Test]
        public void EditingAfterConfirm_VoidsConfirmation()
        {
            var t = New();
            t.Lock("a");
            t.Lock("b");
            t.Confirm("a");
            t.SetOffer("b", "ember_shard", 1);
            Assert.AreEqual(TradePhase.Building, t.Phase);
            Assert.IsFalse(t.IsConfirmed("a"));
        }

        [Test]
        public void Cancel_IsTerminal()
        {
            var t = New();
            t.Cancel();
            Assert.AreEqual(TradePhase.Cancelled, t.Phase);
            Assert.IsFalse(t.SetOffer("a", "hide", 1));
            Assert.IsFalse(t.Lock("a"));
        }

        [Test]
        public void Unlock_VoidsConfirmations()
        {
            var t = New();
            t.Lock("a");
            t.Lock("b");
            t.Confirm("a");
            t.Unlock("b");
            Assert.AreEqual(TradePhase.Building, t.Phase);
            Assert.IsFalse(t.IsConfirmed("a"));
        }
    }
}
