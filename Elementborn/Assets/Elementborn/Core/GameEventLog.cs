using System;
using System.Collections.Generic;
using System.Text;

namespace Elementborn.Core
{
    /// <summary>The category of a logged event. Kept coarse so a session reads as a clear timeline.</summary>
    public enum GameEventKind { SessionStart, Login, Action, Math, Status, Spawn, Respawn, Leaderboard, Error }

    /// <summary>One entry in a session log: an ordered, timestamped record of something that happened.</summary>
    public readonly struct GameEvent
    {
        public readonly long Seq;        // monotonic within the session, starting at 1
        public readonly long UtcTicks;   // when it happened
        public readonly GameEventKind Kind;
        public readonly string Name;     // short verb ("cast", "respawn", "login")
        public readonly string Detail;   // free-form "k=v;k=v" payload (never secrets)

        public GameEvent(long seq, long utcTicks, GameEventKind kind, string name, string detail)
        {
            Seq = seq;
            UtcTicks = utcTicks;
            Kind = kind;
            Name = name ?? "";
            Detail = detail ?? "";
        }
    }

    /// <summary>
    /// A per-session, append-only event log: every action / function call recorded in order with a monotonic
    /// sequence number, so a whole session can be followed and debugged. Pure — the caller supplies timestamps —
    /// and unit-tested; the Game-layer <c>GameEventLogger</c> feeds it and an <c>IEventSink</c> ships drained
    /// batches to storage (Neon Postgres). It never holds secrets: callers pass user ids / names, never passwords.
    /// </summary>
    public sealed class SessionEventLog
    {
        private readonly List<GameEvent> _pending = new List<GameEvent>();
        private long _seq;

        public string SessionId { get; }

        public SessionEventLog(string sessionId) { SessionId = sessionId ?? ""; }

        public int PendingCount => _pending.Count;
        public long LastSeq => _seq;

        /// <summary>Append an event; returns its assigned sequence number.</summary>
        public long Record(GameEventKind kind, string name, string detail, long utcTicks)
        {
            long s = ++_seq;
            _pending.Add(new GameEvent(s, utcTicks, kind, name, detail));
            return s;
        }

        /// <summary>Take all pending events and clear the buffer (for a flush). Sequence numbering continues.</summary>
        public IReadOnlyList<GameEvent> Drain()
        {
            var batch = _pending.ToArray();
            _pending.Clear();
            return batch;
        }

        /// <summary>Serialize a batch to a JSON array, ready to POST or hand to a Postgres <c>jsonb</c> insert.</summary>
        public static string ToJsonBatch(string sessionId, IEnumerable<GameEvent> events)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            bool first = true;
            foreach (var e in events)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('{')
                  .Append("\"session\":").Append(JsonString(sessionId)).Append(',')
                  .Append("\"seq\":").Append(e.Seq).Append(',')
                  .Append("\"ts\":").Append(JsonString(new DateTime(e.UtcTicks, DateTimeKind.Utc).ToString("o"))).Append(',')
                  .Append("\"kind\":").Append(JsonString(e.Kind.ToString())).Append(',')
                  .Append("\"name\":").Append(JsonString(e.Name)).Append(',')
                  .Append("\"detail\":").Append(JsonString(e.Detail))
                  .Append('}');
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static string JsonString(string s)
        {
            if (s == null) return "null";
            var sb = new StringBuilder(s.Length + 2);
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
