using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class CheckpointTests
    {
        [Test]
        public void NoActiveCheckpointByDefault()
        {
            var log = new CheckpointLog();
            Assert.IsNull(log.Active);
            Assert.IsFalse(log.IsActivated("waystone_n"));
        }

        [Test]
        public void ActivatingRecordsAndSetsActive()
        {
            var log = new CheckpointLog();
            Assert.IsTrue(log.Activate("waystone_n"));
            Assert.AreEqual("waystone_n", log.Active);
            Assert.IsTrue(log.IsActivated("waystone_n"));
            Assert.IsFalse(log.Activate("waystone_n")); // same anchor again -> no change
        }

        [Test]
        public void ActivatingADifferentCheckpointMovesTheAnchorButKeepsHistory()
        {
            var log = new CheckpointLog();
            log.Activate("waystone_n");
            Assert.IsTrue(log.Activate("waystone_s"));
            Assert.AreEqual("waystone_s", log.Active);
            Assert.IsTrue(log.IsActivated("waystone_n")); // earlier one stays recorded
            Assert.IsTrue(log.IsActivated("waystone_s"));
        }

        [Test]
        public void EmptyOrNullIdIsIgnored()
        {
            var log = new CheckpointLog();
            Assert.IsFalse(log.Activate(""));
            Assert.IsFalse(log.Activate(null));
            Assert.IsNull(log.Active);
        }

        [Test]
        public void LoadRestoresActivatedAndActive()
        {
            var log = new CheckpointLog();
            log.Load(new[] { "waystone_n", "waystone_e" }, "waystone_e");
            Assert.AreEqual("waystone_e", log.Active);
            Assert.IsTrue(log.IsActivated("waystone_n"));
            Assert.IsTrue(log.IsActivated("waystone_e"));
        }

        [Test]
        public void LoadTreatsTheActiveAnchorAsActivated()
        {
            var log = new CheckpointLog();
            log.Load(new string[0], "waystone_w"); // active given without being in the activated list
            Assert.AreEqual("waystone_w", log.Active);
            Assert.IsTrue(log.IsActivated("waystone_w"));
        }

        [Test]
        public void WorldMapLayoutDefinesDistinctCheckpointsApartFromRifts()
        {
            Assert.Greater(WorldMapLayout.Checkpoints.Count, 0);
            var ids = WorldMapLayout.Checkpoints.Select(c => c.Id).ToList();
            Assert.AreEqual(ids.Count, ids.Distinct().Count()); // unique ids
            var riftIds = new HashSet<string>(WorldMapLayout.Rifts.Select(r => r.Id));
            Assert.IsFalse(ids.Any(id => riftIds.Contains(id))); // checkpoints are not rifts
        }
    }
}
