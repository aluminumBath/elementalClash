using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>
    /// Backs a chat window: send a direct message or a message to your session, read history, and receive
    /// incoming messages via <see cref="MessageReceived"/>. Wires onto <see cref="SocialHub"/>.
    /// </summary>
    public sealed class ChatController : MonoBehaviour
    {
        public event System.Action<ChatMessage> MessageReceived;

        private void OnEnable()
        {
            if (SocialHub.Instance != null) SocialHub.Instance.Chat.MessageReceived += Forward;
        }

        private void OnDisable()
        {
            if (SocialHub.Instance != null) SocialHub.Instance.Chat.MessageReceived -= Forward;
        }

        private void Forward(ChatMessage m) => MessageReceived?.Invoke(m);

        public ChatMessage SendDirect(string toUserId, string text)
        {
            var hub = SocialHub.Instance;
            return hub != null ? hub.Chat.SendDirect(hub.CurrentUser.Id, toUserId, text) : null;
        }

        public ChatMessage SendToSession(string text)
        {
            var hub = SocialHub.Instance;
            return hub != null ? hub.Chat.SendToSession(hub.CurrentUser.Id, hub.CurrentSessionId, text) : null;
        }

        public IReadOnlyList<ChatMessage> DirectHistory(string otherUserId)
        {
            var hub = SocialHub.Instance;
            return hub != null ? hub.Chat.DirectHistory(hub.CurrentUser.Id, otherUserId) : new List<ChatMessage>();
        }

        public IReadOnlyList<ChatMessage> SessionHistory()
        {
            var hub = SocialHub.Instance;
            return hub != null ? hub.Chat.SessionHistory(hub.CurrentSessionId) : new List<ChatMessage>();
        }
    }
}
