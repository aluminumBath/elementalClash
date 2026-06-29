
using NUnit.Framework;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class CreatureOrphanageRecoveryTests
    {
        [SetUp]
        public void SetUp() => ElementbornEditModeTestUtility.ResetAll();

        [TearDown]
        public void TearDown() => ElementbornEditModeTestUtility.ResetAll();

        [Test]
        public void AdmitCreature_SetsExpectedRecoveryState()
        {
            CreatureOrphanageRecoveryRegistry registry = new UnityEngine.GameObject("Orphanage").AddComponent<CreatureOrphanageRecoveryRegistry>();
            CreatureOrphanageResidentRecord record = registry.AdmitCreature("emberfox", "Emberfox", CreatureOrphanageDepartureReason.Mistreatment, "Recovered for test.");

            Assert.NotNull(record);
            Assert.AreEqual(CreatureOrphanageResidentState.AvailableToLureBack, record.State);
            Assert.Greater(record.CareDebt, 0);
            Assert.Greater(record.TrustPenalty, 0);
            Assert.IsTrue(registry.LureBack("emberfox"));
            Assert.AreEqual(CreatureOrphanageResidentState.ReturnedToPlayer, registry.Find("emberfox").State);
        }
    }
}
