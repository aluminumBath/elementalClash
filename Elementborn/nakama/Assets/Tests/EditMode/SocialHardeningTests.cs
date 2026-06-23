using NUnit.Framework;
using System.Collections.Generic;
using Elementborn.Core.Social;

namespace Elementborn.Tests.EditMode
{
    /// <summary>
    /// Locks the cross-service invariants the SocialHub composition relies on — built the same way SocialHub
    /// builds them (services sharing one backend's seams), so these guard the wiring, not just a unit.
    /// </summary>
    public class SocialHardeningTests
    {
        private static (FriendService friends, InviteService invites, NotificationCenter notes, IUserDirectory dir) Build()
        {
            var backend = new LocalSocialBackend();
            var notes = new NotificationCenter(backend.Notifications);
            var dir = backend.Directory;
            dir.Register(new UserRef("a", "Ann", UserRole.Player));
            dir.Register(new UserRef("b", "Bo", UserRole.Player));
            dir.Register(new UserRef("c", "Cy", UserRole.Player));
            var friends = new FriendService(backend.Friends, notes, dir);                       // same graph...
            var invites = new InviteService(backend.Invites, notes, dir, backend.Friends);       // ...as invites
            return (friends, invites, notes, dir);
        }

        [Test]
        public void InviteRequiresFriendshipAcrossTheSharedGraph()
        {
            var s = Build();
            Assert.IsNull(s.invites.Invite("a", "b", "sess1"), "non-friends should not be invitable");

            s.friends.SendRequest("a", "b");
            s.friends.Accept("b", "a");
            Assert.IsTrue(((List<string>)s.friends.FriendsOf("a")).Contains("b"));

            var inv = s.invites.Invite("a", "b", "sess1");   // friendship made via FriendService is visible to InviteService
            Assert.IsNotNull(inv);
            Assert.AreEqual("sess1", s.invites.Accept(inv.Id, "b"));
        }

        [Test]
        public void ReciprocalRequestsBecomeFriendsWithoutAccept()
        {
            var s = Build();
            Assert.AreEqual(FriendStatus.PendingOutgoing, s.friends.SendRequest("a", "b"));
            Assert.AreEqual(FriendStatus.Friends, s.friends.SendRequest("b", "a")); // b already had a's request
            Assert.IsTrue(((List<string>)s.friends.FriendsOf("a")).Contains("b"));
        }

        [Test]
        public void SelfFriendRequestIsRejected()
        {
            var s = Build();
            Assert.AreEqual(FriendStatus.None, s.friends.SendRequest("a", "a"));
            Assert.AreEqual(0, s.friends.PendingIncoming("a").Count);
        }

        [Test]
        public void DeclineRemovesThePendingRequest()
        {
            var s = Build();
            s.friends.SendRequest("a", "b");
            Assert.IsTrue(((List<string>)s.friends.PendingIncoming("b")).Contains("a"));
            Assert.IsTrue(s.friends.Decline("b", "a"));
            Assert.AreEqual(0, s.friends.PendingIncoming("b").Count);
            Assert.IsFalse(((List<string>)s.friends.FriendsOf("b")).Contains("a"));
        }

        [Test]
        public void UnfriendIsSymmetric()
        {
            var s = Build();
            s.friends.SendRequest("a", "b");
            s.friends.Accept("b", "a");
            s.friends.Unfriend("a", "b");
            Assert.IsFalse(((List<string>)s.friends.FriendsOf("a")).Contains("b"));
            Assert.IsFalse(((List<string>)s.friends.FriendsOf("b")).Contains("a"));
        }

        [Test]
        public void InviteAcceptByTheWrongUserFails()
        {
            var s = Build();
            s.friends.SendRequest("a", "b");
            s.friends.Accept("b", "a");
            var inv = s.invites.Invite("a", "b", "sess1");
            Assert.IsNull(s.invites.Accept(inv.Id, "c")); // c isn't the invitee
            Assert.AreEqual("sess1", s.invites.Accept(inv.Id, "b"));
        }

        [Test]
        public void NotificationsAreIsolatedPerUserAndDecrementOnRead()
        {
            var s = Build();
            var na = s.notes.Post(NotificationKind.System, "Hi A", "x", "a", null);
            s.notes.Post(NotificationKind.System, "Hi B", "y", "b", null);

            Assert.AreEqual(1, s.notes.UnreadCountFor("a"));
            Assert.AreEqual(1, s.notes.InboxFor("a").Count);
            Assert.AreEqual(1, s.notes.UnreadCountFor("b"));

            s.notes.MarkRead(na.Id);
            Assert.AreEqual(0, s.notes.UnreadCountFor("a"));
            Assert.AreEqual(1, s.notes.UnreadCountFor("b"), "marking A's read must not touch B");
        }
    }
}
