using NUnit.Framework;
using Elementborn.Core.Social;

namespace Elementborn.Tests.EditMode
{
    public class SocialBackendTests
    {
        [Test]
        public void DefaultBackendIsLocalAndComplete()
        {
            var b = SocialBackends.Create();
            Assert.IsInstanceOf<LocalSocialBackend>(b);
            Assert.NotNull(b.Directory);
            Assert.NotNull(b.Notifications);
            Assert.NotNull(b.Feedback);
            Assert.NotNull(b.Friends);
            Assert.NotNull(b.Invites);
            Assert.NotNull(b.Messages);
            Assert.NotNull(b.Moderation);
        }

        [Test]
        public void FactoryOverrideIsHonoredThenRestored()
        {
            var original = SocialBackends.Factory;
            try
            {
                var custom = new LocalSocialBackend();
                SocialBackends.Factory = () => custom;
                Assert.AreSame(custom, SocialBackends.Create());

                SocialBackends.Factory = null;            // null falls back to local
                Assert.IsInstanceOf<LocalSocialBackend>(SocialBackends.Create());
            }
            finally
            {
                SocialBackends.Factory = original;
            }
        }

        [Test]
        public void LocalBackendSeamsFunctionEndToEnd()
        {
            var b = new LocalSocialBackend();
            b.Directory.Register(new UserRef("u", "Uma", UserRole.Player));
            Assert.IsTrue(b.Directory.TryGet("u", out var u) && u.DisplayName == "Uma");

            string channel = ChatService.SessionChannelId("s1");
            b.Messages.Send(new ChatMessage("1", channel, "u", "hi", 0));
            Assert.AreEqual(1, b.Messages.History(channel).Count);
        }
    }
}
