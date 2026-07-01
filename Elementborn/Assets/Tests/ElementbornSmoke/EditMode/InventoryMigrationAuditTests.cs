using System.Collections.Generic;
using NUnit.Framework;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public sealed class InventoryMigrationAuditTests
    {
        [Test]
        public void Compute_WithNoDefinitions_ReportsEveryLegacyItemMissing()
        {
            var result = InventoryMigrationAudit.Compute(new List<string>());

            Assert.That(result.LegacyCount, Is.GreaterThan(0));
            Assert.That(result.DefinitionCount, Is.EqualTo(0));
            Assert.That(result.CoveredIds, Is.Empty);
            Assert.That(result.OrphanDefIds, Is.Empty);
            Assert.That(result.MissingIds.Count, Is.EqualTo(result.LegacyCount));
            Assert.That(result.MissingIds, Contains.Item("iron_helm"));
        }

        [Test]
        public void Compute_SplitsCoveredMissingAndOrphans()
        {
            var defs = new List<string> { "iron_helm", "fire_arrow", "not_a_real_item" };
            var result = InventoryMigrationAudit.Compute(defs);

            Assert.That(result.CoveredIds, Contains.Item("iron_helm"));
            Assert.That(result.CoveredIds, Contains.Item("fire_arrow"));
            Assert.That(result.MissingIds, Does.Not.Contain("iron_helm"));
            Assert.That(result.OrphanDefIds, Contains.Item("not_a_real_item"));
            Assert.That(result.CoveredIds.Count + result.MissingIds.Count, Is.EqualTo(result.LegacyCount));
        }
    }
}
