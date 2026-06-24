using System;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ModelBindingsTests
    {
        [Test]
        public void EveryGuideNpcResolvesToANonEmptyModel()
        {
            foreach (GuideNpcId id in Enum.GetValues(typeof(GuideNpcId)))
            {
                string path = NpcModelNames.ResourcePath(id);
                StringAssert.StartsWith(NpcModelNames.ResourceRoot, path);
                Assert.IsFalse(string.IsNullOrEmpty(NpcModelNames.ResourceName(id)), $"{id} has no model name");
            }
        }

        [Test]
        public void MappedSidekicksHaveModels_UnmappedFallBackToEnumName()
        {
            Assert.AreEqual("Moss_Wolf/Moss_Wolf", SidekickModelNames.ResourceName(WillowSidekick.Gunnar));
            // Chameleon is intentionally unmapped — it falls back to the enum name (→ primitive at runtime).
            Assert.AreEqual("Chameleon", SidekickModelNames.ResourceName(WillowSidekick.Chameleon));
            foreach (var kv in SidekickModelNames.Aliases)
                Assert.IsFalse(string.IsNullOrEmpty(kv.Value), $"sidekick alias for {kv.Key} is empty");
        }

        [Test]
        public void MappedWeaponsHaveModels_UnmappedFallBack()
        {
            Assert.AreEqual("Models/Weapons/Emberblade/Emberblade", WeaponModelNames.ResourcePath(WeaponType.Sword));
            Assert.AreEqual("Models/Weapons/Gilded_Arc_Bow/Gilded_Arc_Bow", WeaponModelNames.ResourcePath(WeaponType.LongBow));
            // Dagger has no batch model — falls back to the enum name.
            Assert.AreEqual("Dagger", WeaponModelNames.ResourceName(WeaponType.Dagger));
        }

        [Test]
        public void PlayerModelIsConfigured()
        {
            Assert.IsFalse(string.IsNullOrEmpty(PlayerModelNames.Model));
            StringAssert.StartsWith(PlayerModelNames.ResourceRoot, PlayerModelNames.ResourcePath());
        }

        [Test]
        public void PropCatalogResolvesKnownKeysAndRejectsUnknown()
        {
            Assert.AreEqual("Models/Props/Azure_Arc_Portal", PropCatalog.ResourcePath("rift_portal"));
            Assert.IsNull(PropCatalog.ResourcePath("not_a_prop"));
        }

        [Test]
        public void MappedItemsHaveModels_UnmappedFallBackToTheId()
        {
            Assert.AreEqual("Models/Items/Emberstone_Gem/Emberstone_Gem", ItemModelNames.ResourcePath("ember_shard"));
            Assert.AreEqual("Models/Items/Pearl_Oyster/Pearl_Oyster", ItemModelNames.ResourcePath("river_pearl"));
            // An unmapped item (a food) falls back to its id → no model file → primitive at runtime.
            Assert.AreEqual("Models/Items/deep_jelly", ItemModelNames.ResourcePath("deep_jelly"));
            Assert.IsNull(ItemModelNames.ResourcePath(null));
        }
    }
}
