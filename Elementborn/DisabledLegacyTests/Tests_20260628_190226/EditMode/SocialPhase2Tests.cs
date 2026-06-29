using NUnit.Framework;
using Elementborn.Core.Social;

namespace Elementborn.Tests.EditMode
{
    public class SocialPhase2Tests
    {
        [Test]
        public void RequestThenAcceptMakesFriends()
        {
            var g = new LocalFriendGraph();
            Assert.AreEqual(FriendStatus.PendingOutgoing, g.SendRequest("a", "b"));
            Assert.AreEqual(FriendStatus.PendingIncoming, g.StatusBetween("b", "a"));
            Assert.IsFalse(g.AreFriends("a", "b"));

            Assert.IsTrue(g.Accept("b", "a"));
            Assert.IsTrue(g.AreFriends("a", "b"));
            Assert.Contains("b", (System.Collections.ICollection)g.FriendsOf("a"));
            Assert.Contains("a", (System.Collections.ICollection)g.FriendsOf("b"));
            Assert.AreEqual(0, g.PendingIncoming("b").Count);
        }

        [Test]
        public void ReciprocalRequestAutoAccepts()
        {
            var g = new LocalFriendGraph();
            g.SendRequest("a", "b");
            Assert.AreEqual(FriendStatus.Friends, g.SendRequest("b", "a")); // they asked back -> instant friends
            Assert.IsTrue(g.AreFriends("a", "b"));
        }

        [Test]
        public void DeclineAndUnfriend()
        {
            var g = new LocalFriendGraph();
            g.SendRequest("a", "b");
            Assert.IsTrue(g.Decline("b", "a"));
            Assert.IsFalse(g.AreFriends("a", "b"));
            Assert.AreEqual(0, g.PendingIncoming("b").Count);

            g.SendRequest("a", "b"); g.Accept("b", "a");
            Assert.IsTrue(g.Remove("a", "b"));
            Assert.IsFalse(g.AreFriends("a", "b"));
            Assert.IsFalse(g.SendRequest("a", "a") == FriendStatus.PendingOutgoing); // can't friend yourself
        }

        [Test]
        public void FriendServicePostsNotifications()
        {
            var dir = new LocalUserDirectory();
            var center = new NotificationCenter(new InMemoryNotificationStore());
            var svc = new FriendService(new LocalFriendGraph(), center, dir);
            dir.Register(new UserRef("a", "Ann", UserRole.Player));
            dir.Register(new UserRef("b", "Bo", UserRole.Player));

            svc.SendRequest("a", "b");
            Assert.AreEqual(1, center.InboxFor("b").Count);
            Assert.AreEqual(NotificationKind.FriendRequest, center.InboxFor("b")[0].Kind);

            svc.Accept("b", "a");
            Assert.AreEqual(1, center.InboxFor("a").Count); // requester told it was accepted
        }

        [Test]
        public void InviteRequiresFriendshipThenJoins()
        {
            var dir = new LocalUserDirectory();
            var center = new NotificationCenter(new InMemoryNotificationStore());
            var graph = new LocalFriendGraph();
            var invites = new InviteService(new InMemoryInviteStore(), center, dir, graph);
            dir.Register(new UserRef("host", "Ann", UserRole.Player));
            dir.Register(new UserRef("pal", "Bo", UserRole.Player));

            Assert.IsNull(invites.Invite("host", "pal", "sess-1"));   // not friends yet
            graph.SendRequest("host", "pal"); graph.Accept("pal", "host");

            var invite = invites.Invite("host", "pal", "sess-1");
            Assert.IsNotNull(invite);
            Assert.AreEqual(1, invites.PendingFor("pal").Count);
            Assert.AreEqual(NotificationKind.Invite, center.InboxFor("pal")[0].Kind);

            string session = invites.Accept(invite.Id, "pal");
            Assert.AreEqual("sess-1", session);                       // returns the host's session to join
            Assert.AreEqual(0, invites.PendingFor("pal").Count);      // no longer pending
            Assert.IsNull(invites.Accept(invite.Id, "pal"));          // can't accept twice
        }
    }
}
