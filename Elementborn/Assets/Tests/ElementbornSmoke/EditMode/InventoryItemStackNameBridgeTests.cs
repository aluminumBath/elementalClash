using NUnit.Framework;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public sealed class InventoryItemStackNameBridgeTests
    {
        [Test]
        public void DisplayName_ForLegacyId_UsesCatalogName()
        {
            var stack = new InventoryItemStack("iron_helm", 1);
            Assert.That(stack.DisplayName, Is.EqualTo(ItemCatalog.Get("iron_helm").Name));
            Assert.That(stack.DisplayName, Is.Not.EqualTo("iron_helm"));
        }

        [Test]
        public void DisplayName_ForUnknownId_FallsBackToRawId()
        {
            var stack = new InventoryItemStack("not_a_catalog_item", 3);
            Assert.That(stack.DisplayName, Is.EqualTo("not_a_catalog_item"));
        }
    }
}
