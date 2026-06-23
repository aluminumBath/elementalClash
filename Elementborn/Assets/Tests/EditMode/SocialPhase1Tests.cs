using NUnit.Framework;
using Elementborn.Core.Social;

namespace Elementborn.Tests.EditMode
{
    public class SocialPhase1Tests
    {
        [Test]
        public void DirectoryTracksRolesAndAdmins()
        {
            var dir = new LocalUserDirectory();
            dir.Register(new UserRef("u1", "Ann", UserRole.Player));
            dir.Register(new UserRef("u2", "Bo", UserRole.Admin));
            Assert.AreEqual(1, dir.Admins().Count);

            dir.SetRole("u1", UserRole.Admin);            // promote
            Assert.AreEqual(2, dir.Admins().Count);
            Assert.IsTrue(dir.TryGet("u1", out var u) && u.IsAdmin);

            dir.Register(new UserRef("u3", "Cy", UserRole.SessionAdmin));
            Assert.IsTrue(dir.TryGet("u3", out var s) && s.CanModerateSessions && !s.IsAdmin);
        }

        [Test]
        public void NotificationsDeliverAndTrackUnread()
        {
            var center = new NotificationCenter(new InMemoryNotificationStore());
            int posted = 0; center.Posted += _ => posted++;

            var n = center.Post(NotificationKind.System, "Welcome", "Hello", "u1");
            center.Post(NotificationKind.Message, "Hi", "from Bo", "u1", "u2");

            Assert.AreEqual(2, posted);
            Assert.AreEqual(2, center.InboxFor("u1").Count);
            Assert.AreEqual(0, center.InboxFor("u2").Count);   // u2 is the sender, not a recipient
            Assert.AreEqual(2, center.UnreadCountFor("u1"));

            center.MarkRead(n.Id);
            Assert.AreEqual(1, center.UnreadCountFor("u1"));
        }

        [Test]
        public void FeedbackNotifiesEveryAdminOnly()
        {
            var dir = new LocalUserDirectory();
            var center = new NotificationCenter(new InMemoryNotificationStore());
            var feedback = new FeedbackService(new InMemoryFeedbackStore(), center, dir);

            dir.Register(new UserRef("admin1", "Ada", UserRole.Admin));
            dir.Register(new UserRef("admin2", "Ben", UserRole.Admin));
            dir.Register(new UserRef("player1", "Cy", UserRole.Player));

            var report = feedback.Submit(FeedbackKind.Bug, "Crash on dash", "Repro steps...", "player1");
            Assert.AreEqual(FeedbackKind.Bug, report.Kind);
            Assert.AreEqual(1, feedback.AllReports().Count);

            Assert.AreEqual(1, center.InboxFor("admin1").Count);
            Assert.AreEqual(1, center.InboxFor("admin2").Count);
            Assert.AreEqual(0, center.InboxFor("player1").Count);   // submitter isn't notified
            Assert.AreEqual(NotificationKind.Feedback, center.InboxFor("admin1")[0].Kind);
        }
    }
}
