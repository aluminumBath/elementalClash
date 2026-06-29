using System;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SessionEventLogTests
    {
        private static long Ticks(int s) => new DateTime(2026, 6, 24, 0, 0, s, DateTimeKind.Utc).Ticks;

        [Test]
        public void RecordAssignsMonotonicSequence()
        {
            var log = new SessionEventLog("sess1");
            Assert.AreEqual(1, log.Record(GameEventKind.SessionStart, "start", null, Ticks(0)));
            Assert.AreEqual(2, log.Record(GameEventKind.Action, "cast", "element=Fire", Ticks(1)));
            Assert.AreEqual(3, log.Record(GameEventKind.Respawn, "respawn", "point=spawn", Ticks(2)));
            Assert.AreEqual(3, log.PendingCount);
            Assert.AreEqual(3, log.LastSeq);
        }

        [Test]
        public void DrainEmptiesButKeepsNumbering()
        {
            var log = new SessionEventLog("sess1");
            log.Record(GameEventKind.Action, "a", null, Ticks(0));
            log.Record(GameEventKind.Action, "b", null, Ticks(1));

            var batch = log.Drain();
            Assert.AreEqual(2, batch.Count);
            Assert.AreEqual(0, log.PendingCount);

            // Sequence continues after a flush, so seq is unique across the whole session.
            Assert.AreEqual(3, log.Record(GameEventKind.Action, "c", null, Ticks(2)));
        }

        [Test]
        public void JsonBatchCarriesFieldsInOrder()
        {
            var log = new SessionEventLog("sess1");
            log.Record(GameEventKind.Login, "login", "user=u1;name=Willow", Ticks(0));
            log.Record(GameEventKind.Action, "cast", "element=Fire", Ticks(1));
            string json = SessionEventLog.ToJsonBatch(log.SessionId, log.Drain());

            StringAssert.Contains("\"session\":\"sess1\"", json);
            StringAssert.Contains("\"seq\":1", json);
            StringAssert.Contains("\"seq\":2", json);
            StringAssert.Contains("\"kind\":\"Login\"", json);
            StringAssert.Contains("\"name\":\"cast\"", json);
            StringAssert.Contains("element=Fire", json);
        }

        [Test]
        public void JsonEscapesSpecialCharacters()
        {
            var log = new SessionEventLog("s");
            log.Record(GameEventKind.Error, "boom", "msg=\"bad\"\nline\\two", Ticks(0));
            string json = SessionEventLog.ToJsonBatch(log.SessionId, log.Drain());

            StringAssert.Contains("\\\"bad\\\"", json);   // escaped quotes
            StringAssert.Contains("\\n", json);            // escaped newline
            StringAssert.Contains("\\\\two", json);        // escaped backslash
            Assert.IsFalse(json.Contains("\n"), "raw newlines must not leak into the JSON");
        }

        [Test]
        public void EmptyBatchIsAnEmptyArray()
        {
            Assert.AreEqual("[]", SessionEventLog.ToJsonBatch("s", Enumerable.Empty<GameEvent>()));
        }
    }
}
