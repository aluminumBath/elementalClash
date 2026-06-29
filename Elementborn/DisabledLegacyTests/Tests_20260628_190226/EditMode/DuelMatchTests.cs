using NUnit.Framework;
using Elementborn.Core.Social;

namespace Elementborn.Tests
{
    public class DuelMatchTests
    {
        private static DuelMatch Offered() => new DuelMatch("me", "rival", 3);

        [Test]
        public void New_IsOffered_ZeroScores()
        {
            var d = Offered();
            Assert.AreEqual(DuelPhase.Offered, d.Phase);
            Assert.AreEqual(0, d.ChallengerScore);
            Assert.AreEqual(0, d.OpponentScore);
            Assert.IsNull(d.Winner);
        }

        [Test]
        public void Accept_StartsDuel_Once()
        {
            var d = Offered();
            Assert.IsTrue(d.Accept());
            Assert.AreEqual(DuelPhase.InProgress, d.Phase);
            Assert.IsFalse(d.Accept());
        }

        [Test]
        public void Decline_IsTerminal()
        {
            var d = Offered();
            Assert.IsTrue(d.Decline());
            Assert.AreEqual(DuelPhase.Declined, d.Phase);
            Assert.IsFalse(d.Accept());
        }

        [Test]
        public void Score_RejectedBeforeAccept_AndFromStranger()
        {
            var d = Offered();
            Assert.IsFalse(d.Score("me")); // not started
            d.Accept();
            Assert.IsFalse(d.Score("stranger"));
            Assert.IsTrue(d.Score("me"));
            Assert.AreEqual(1, d.ChallengerScore);
        }

        [Test]
        public void FirstToTarget_Wins_AndFurtherScoresIgnored()
        {
            var d = Offered();
            d.Accept();
            d.Score("me"); d.Score("me"); d.Score("me"); // 3 = target
            Assert.AreEqual(DuelPhase.Finished, d.Phase);
            Assert.AreEqual("me", d.Winner);
            Assert.IsFalse(d.Score("rival")); // already finished
            Assert.AreEqual(0, d.OpponentScore);
        }

        [Test]
        public void Cancel_FromInProgress_NotAfterFinish()
        {
            var d = Offered();
            d.Accept();
            d.Cancel();
            Assert.AreEqual(DuelPhase.Cancelled, d.Phase);

            var d2 = Offered();
            d2.Accept();
            d2.Score("me"); d2.Score("me"); d2.Score("me");
            d2.Cancel(); // no effect once finished
            Assert.AreEqual(DuelPhase.Finished, d2.Phase);
        }

        [Test]
        public void Target_ClampedToAtLeastOne()
        {
            var d = new DuelMatch("a", "b", 0);
            Assert.AreEqual(1, d.Target);
            d.Accept();
            d.Score("a");
            Assert.AreEqual(DuelPhase.Finished, d.Phase);
        }
    }
}
