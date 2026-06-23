using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    /// <summary>One chat message in a channel (a direct pair, or a session).</summary>
    public sealed class ChatMessage
    {
        public string Id { get; }
        public string ChannelId { get; }
        public string SenderId { get; }
        public string Text { get; }
        public long SentAtUtcTicks { get; }

        public ChatMessage(string id, string channelId, string senderId, string text, long sentAtUtcTicks)
        {
            Id = id; ChannelId = channelId; SenderId = senderId; Text = text; SentAtUtcTicks = sentAtUtcTicks;
        }
    }

    /// <summary>The seam over message send/receive/history. <see cref="InMemoryMessageTransport"/> is offline;
    /// a Nakama adapter sends over a channel and raises <see cref="Received"/> from the socket.</summary>
    public interface IMessageTransport
    {
        void Send(ChatMessage message);
        IReadOnlyList<ChatMessage> History(string channelId, int max = 50);
        event System.Action<ChatMessage> Received;
    }

    public sealed class InMemoryMessageTransport : IMessageTransport
    {
        private readonly Dictionary<string, List<ChatMessage>> _byChannel = new Dictionary<string, List<ChatMessage>>();
        public event System.Action<ChatMessage> Received;

        public void Send(ChatMessage message)
        {
            if (!_byChannel.TryGetValue(message.ChannelId, out var list))
            {
                list = new List<ChatMessage>();
                _byChannel[message.ChannelId] = list;
            }
            list.Add(message);
            Received?.Invoke(message);
        }

        public IReadOnlyList<ChatMessage> History(string channelId, int max = 50)
        {
            if (!_byChannel.TryGetValue(channelId, out var list)) return new List<ChatMessage>();
            int start = list.Count > max ? list.Count - max : 0;
            return list.GetRange(start, list.Count - start);
        }
    }

    /// <summary>The app-facing chat API: direct messages between two users and a per-session channel, plus
    /// history. Channel ids are derived so both ends of a DM resolve the same channel. Forwards incoming
    /// messages on <see cref="MessageReceived"/> for a chat window. Storage/transport is the
    /// <see cref="IMessageTransport"/> seam.</summary>
    public sealed class ChatService
    {
        private readonly IMessageTransport _transport;
        public event System.Action<ChatMessage> MessageReceived;

        public ChatService(IMessageTransport transport)
        {
            _transport = transport;
            _transport.Received += m => MessageReceived?.Invoke(m);
        }

        public static string DirectChannelId(string a, string b) =>
            string.CompareOrdinal(a, b) <= 0 ? "dm\u0001" + a + "\u0001" + b : "dm\u0001" + b + "\u0001" + a;

        public static string SessionChannelId(string sessionId) => "ses\u0001" + sessionId;

        public ChatMessage SendDirect(string fromId, string toId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var m = new ChatMessage(System.Guid.NewGuid().ToString("N"), DirectChannelId(fromId, toId),
                                    fromId, text, System.DateTime.UtcNow.Ticks);
            _transport.Send(m);
            return m;
        }

        public ChatMessage SendToSession(string fromId, string sessionId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var m = new ChatMessage(System.Guid.NewGuid().ToString("N"), SessionChannelId(sessionId),
                                    fromId, text, System.DateTime.UtcNow.Ticks);
            _transport.Send(m);
            return m;
        }

        public IReadOnlyList<ChatMessage> DirectHistory(string a, string b, int max = 50) =>
            _transport.History(DirectChannelId(a, b), max);

        public IReadOnlyList<ChatMessage> SessionHistory(string sessionId, int max = 50) =>
            _transport.History(SessionChannelId(sessionId), max);
    }
}
