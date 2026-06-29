using NUnit.Framework;
using Elementborn.Core.Social;

namespace Elementborn.Tests.EditMode
{
    public class SocialPhase3Tests
    {
        [Test]
        public void DirectMessagesShareACanonicalChannel()
        {
            var chat = new ChatService(new InMemoryMessageTransport());
            int received = 0; chat.MessageReceived += _ => received++;

            chat.SendDirect("a", "b", "hi");
            chat.SendDirect("b", "a", "yo");   // same conversation, other direction

            Assert.AreEqual(2, received);
            Assert.AreEqual(2, chat.DirectHistory("a", "b").Count);
            Assert.AreEqual(2, chat.DirectHistory("b", "a").Count);   // resolves to the same channel
            Assert.AreEqual(ChatService.DirectChannelId("a", "b"), ChatService.DirectChannelId("b", "a"));
            Assert.IsNull(chat.SendDirect("a", "b", "   "));          // blank text ignored
        }

        [Test]
        public void SessionMessagesAreScopedToTheSession()
        {
            var chat = new ChatService(new InMemoryMessageTransport());
            chat.SendToSession("a", "s1", "hello team");
            chat.SendToSession("b", "s1", "hi");
            Assert.AreEqual(2, chat.SessionHistory("s1").Count);
            Assert.AreEqual(0, chat.SessionHistory("s2").Count);
            Assert.AreEqual(0, chat.DirectHistory("a", "b").Count);   // not a DM
        }

        [Test]
        public void AdminGlobalBanBlocksJoinThenLifts()
        {
            var dir = new LocalUserDirectory();
            var center = new NotificationCenter(new InMemoryNotificationStore());
            var mod = new ModerationService(new InMemoryModerationStore(), center, dir);
            dir.Register(new UserRef("admin", "Ada", UserRole.Admin));
            dir.Register(new UserRef("u", "Cy", UserRole.Player));

            Assert.IsTrue(mod.BanGlobally("admin", "u", "spam"));
            Assert.IsFalse(mod.CanJoin("u", "any-session"));          // barred everywhere
            Assert.AreEqual(1, center.InboxFor("u").Count);
            Assert.AreEqual(NotificationKind.Moderation, center.InboxFor("u")[0].Kind);

            Assert.IsTrue(mod.LiftGlobalBan("admin", "u"));
            Assert.IsTrue(mod.CanJoin("u", "any-session"));
        }

        [Test]
        public void SessionAdminBanIsScopedAndReversible()
        {
            var dir = new LocalUserDirectory();
            var mod = new ModerationService(new InMemoryModerationStore(),
                new NotificationCenter(new InMemoryNotificationStore()), dir);
            dir.Register(new UserRef("smod", "Sam", UserRole.SessionAdmin));
            dir.Register(new UserRef("u", "Cy", UserRole.Player));

            Assert.IsTrue(mod.BanFromSession("smod", "u", "s1", "trolling"));
            Assert.IsFalse(mod.CanJoin("u", "s1"));
            Assert.IsTrue(mod.CanJoin("u", "s2"));                    // only that session
            Assert.IsTrue(mod.AllowInSession("smod", "u", "s1"));     // allow back in
            Assert.IsTrue(mod.CanJoin("u", "s1"));
        }

        [Test]
        public void RolesAreEnforced()
        {
            var dir = new LocalUserDirectory();
            var mod = new ModerationService(new InMemoryModerationStore(),
                new NotificationCenter(new InMemoryNotificationStore()), dir);
            dir.Register(new UserRef("player", "Pat", UserRole.Player));
            dir.Register(new UserRef("smod", "Sam", UserRole.SessionAdmin));
            dir.Register(new UserRef("u", "Cy", UserRole.Player));

            Assert.IsFalse(mod.BanGlobally("player", "u", null));     // players can't ban
            Assert.IsFalse(mod.BanFromSession("player", "u", "s1", null));
            Assert.IsFalse(mod.BanGlobally("smod", "u", null));       // session-admins can't ban globally
            Assert.IsTrue(mod.CanJoin("u", "s1"));                    // nothing applied
            Assert.IsTrue(mod.BanFromSession("smod", "u", "s1", null)); // but can ban from a session
        }
    }
}
