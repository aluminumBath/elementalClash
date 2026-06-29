using NUnit.Framework;
using Elementborn.Core.Social;

namespace Elementborn.Tests
{
    public class GuildTests
    {
        private static Guild Founded() => new Guild("g1", "Emberwatch", "leader");

        [Test]
        public void Found_SetsLeader()
        {
            var g = Founded();
            Assert.AreEqual(1, g.Count);
            Assert.AreEqual("leader", g.LeaderId);
            Assert.IsTrue(g.IsLeader("leader"));
        }

        [Test]
        public void Join_AddsMember_NoDuplicates()
        {
            var g = Founded();
            Assert.IsTrue(g.Join("m1"));
            Assert.IsFalse(g.Join("m1"));
            Assert.AreEqual(GuildRank.Member, g.Find("m1").Rank);
        }

        [Test]
        public void Kick_RequiresOutranking_NotSelf()
        {
            var g = Founded();
            g.Join("m1");
            g.Join("m2");
            g.Promote("leader", "m2"); // m2 -> Officer
            Assert.IsFalse(g.Kick("leader", "leader")); // not self
            Assert.IsFalse(g.Kick("m1", "m2"));         // member can't kick officer
            Assert.IsTrue(g.Kick("m2", "m1"));          // officer can kick member
            Assert.IsFalse(g.Contains("m1"));
        }

        [Test]
        public void Promote_Member_To_Officer()
        {
            var g = Founded();
            g.Join("m1");
            Assert.IsTrue(g.Promote("leader", "m1"));
            Assert.AreEqual(GuildRank.Officer, g.Find("m1").Rank);
        }

        [Test]
        public void Promote_Officer_To_Leader_TransfersCommand()
        {
            var g = Founded();
            g.Join("m1");
            g.Promote("leader", "m1"); // Officer
            Assert.IsTrue(g.Promote("leader", "m1")); // -> Leader
            Assert.AreEqual("m1", g.LeaderId);
            Assert.AreEqual(GuildRank.Officer, g.Find("leader").Rank); // old leader stepped down
        }

        [Test]
        public void Promote_NonLeader_Denied()
        {
            var g = Founded();
            g.Join("m1");
            g.Join("m2");
            Assert.IsFalse(g.Promote("m1", "m2"));
        }

        [Test]
        public void Demote_Officer_To_Member_Only()
        {
            var g = Founded();
            g.Join("m1");
            g.Promote("leader", "m1"); // Officer
            Assert.IsTrue(g.Demote("leader", "m1"));
            Assert.AreEqual(GuildRank.Member, g.Find("m1").Rank);
            Assert.IsFalse(g.Demote("leader", "m1")); // already a Member
        }

        [Test]
        public void Leave_Leader_PromotesHighestSuccessor()
        {
            var g = Founded();
            g.Join("m1");
            g.Join("m2");
            g.Promote("leader", "m2"); // m2 -> Officer, outranks m1
            Assert.IsTrue(g.Leave("leader"));
            Assert.AreEqual("m2", g.LeaderId); // the officer succeeds, not the plain member
        }

        [Test]
        public void Rename_LeaderOnly_NoBlank()
        {
            var g = Founded();
            g.Join("m1");
            Assert.IsFalse(g.Rename("m1", "Hijack"));
            Assert.IsFalse(g.Rename("leader", "   "));
            Assert.IsTrue(g.Rename("leader", "Stormforged"));
            Assert.AreEqual("Stormforged", g.Name);
        }

        [Test]
        public void Restore_RebuildsRosterWithRanks()
        {
            var ids = new System.Collections.Generic.List<string> { "leader", "off", "mem" };
            var ranks = new System.Collections.Generic.List<GuildRank> { GuildRank.Leader, GuildRank.Officer, GuildRank.Member };
            var g = Guild.Restore("g1", "Emberwatch", ids, ranks);
            Assert.AreEqual(3, g.Count);
            Assert.AreEqual("leader", g.LeaderId);
            Assert.AreEqual(GuildRank.Officer, g.Find("off").Rank);
            Assert.AreEqual(GuildRank.Member, g.Find("mem").Rank);
        }
    }
}
