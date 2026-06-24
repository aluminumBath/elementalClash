#if ELEMENTBORN_NAKAMA
using System;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;
using Elementborn.Game;

namespace Elementborn.Game.Social.NakamaNet
{
    /// <summary>
    /// Ships session-event batches to the server's <c>events_ingest</c> RPC, which writes them to the database
    /// Nakama is configured to use (point Nakama at Neon — see docs/EVENT_LOGGING.md). The Postgres connection
    /// string never lives in the client, only on the server. Compiled in only when <c>ELEMENTBORN_NAKAMA</c> is
    /// defined; otherwise the logger keeps the console sink. Sends are fire-and-forget so logging never blocks the
    /// game thread; if a send fails or we're offline the batch is dropped (telemetry, not gameplay state).
    /// </summary>
    public sealed class NeonEventSink : IEventSink
    {
        private readonly NakamaConnection _connection;

        public NeonEventSink(NakamaConnection connection) { _connection = connection; }

        public void Send(string sessionId, string batchJson, int count)
        {
            if (_connection == null || !_connection.Connected) return; // offline: drop this batch
            _ = SendAsync(batchJson);
        }

        private async Task SendAsync(string batchJson)
        {
            try { await _connection.Client.RpcAsync(_connection.Session, "events_ingest", batchJson); }
            catch (Exception e) { Debug.LogWarning("[events] Neon ingest failed: " + e.Message); }
        }
    }
}
#endif
