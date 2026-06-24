using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Where a drained batch of session events goes. The default prints a one-line summary to the console; a
    /// Neon-backed sink — routed through the server so the database connection string never ships in the client —
    /// can be installed at startup via <see cref="GameEventLogger.SetSink"/>. The batch is already serialized JSON.
    /// </summary>
    public interface IEventSink
    {
        void Send(string sessionId, string batchJson, int count);
    }

    /// <summary>Dev default: one concise line per flush (never per-event spam). Swap in a real sink for storage.</summary>
    public sealed class ConsoleEventSink : IEventSink
    {
        private readonly bool _verbose;

        public ConsoleEventSink(bool verbose = false) { _verbose = verbose; }

        public void Send(string sessionId, string batchJson, int count)
        {
            Debug.Log($"[events] session {sessionId}: flushed {count} event(s)");
            if (_verbose) Debug.Log("[events] " + batchJson);
        }
    }
}
