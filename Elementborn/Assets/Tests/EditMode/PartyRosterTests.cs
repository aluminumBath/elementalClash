using NUnit.Framework;
using Elementborn.Core.Social;

namespace Elementborn.Tests
{
    public class PartyRosterTests
    {
        [Test]
        public void Form_SetsLeaderAndSingleMember()
        {
            var p = new PartyRoster();
            p.Form("me");
            Assert.IsTrue(p.Active);
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual("me", p.LeaderId);
            Assert.IsTrue(p.IsLeader("me"));
        }

        [Test]
        public void Join_AddsUpToCap_ThenReportsFull()
        {
            var p = new PartyRoster();
            p.Form("a");
            Assert.AreEqual(PartyJoinResult.Joined, p.Join("b"));
            Assert.AreEqual(PartyJoinResult.AlreadyMember, p.Join("b"));
            Assert.AreEqual(PartyJoinResult.Joined, p.Join("c"));
            Assert.AreEqual(PartyJoinResult.Joined, p.Join("d")); // 4th = cap
            Assert.IsTrue(p.IsFull);
            Assert.AreEqual(PartyJoinResult.PartyFull, p.Join("e"));
        }

        [Test]
        public void Join_WithoutParty_ReportsNoParty()
        {
            var p = new PartyRoster();
            Assert.AreEqual(PartyJoinResult.NoParty, p.Join("x"));
        }

        [Test]
        public void Leave_Leader_PromotesNextMember()
        {
            var p = new PartyRoster();
            p.Form("a");
            p.Join("b");
            p.Leave("a");
            Assert.AreEqual("b", p.LeaderId);
            Assert.IsFalse(p.Contains("a"));
        }

        [Test]
        public void Leave_LastMember_Disbands()
        {
            var p = new PartyRoster();
            p.Form("a");
            p.Leave("a");
            Assert.IsFalse(p.Active);
            Assert.IsNull(p.LeaderId);
        }

        [Test]
        public void Kick_OnlyLeader_AndNotSelf()
        {
            var p = new PartyRoster();
            p.Form("a");
            p.Join("b");
            Assert.IsFalse(p.Kick("b", "a")); // non-leader can't kick
            Assert.IsFalse(p.Kick("a", "a")); // leader can't kick self
            Assert.IsTrue(p.Kick("a", "b"));
            Assert.IsFalse(p.Contains("b"));
        }

        [Test]
        public void Promote_OnlyLeader_TransfersLead()
        {
            var p = new PartyRoster();
            p.Form("a");
            p.Join("b");
            Assert.IsFalse(p.Promote("b", "a")); // non-leader can't promote
            Assert.IsTrue(p.Promote("a", "b"));
            Assert.AreEqual("b", p.LeaderId);
        }
    }
}
