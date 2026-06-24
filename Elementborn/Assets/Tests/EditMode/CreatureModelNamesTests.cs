using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class CreatureModelNamesTests
    {
        [Test]
        public void AliasedKindUsesTheAliasName()
        {
            // Phoenix is wired to the Fire_Phoenix model (kept in its own folder with its textures).
            Assert.AreEqual("Fire_Phoenix/Fire_Phoenix", CreatureModelNames.ResourceName(CreatureKind.Phoenix));
            Assert.AreEqual("Models/Creatures/Fire_Phoenix/Fire_Phoenix", CreatureModelNames.ResourcePath(CreatureKind.Phoenix));
        }

        [Test]
        public void ResourcePathIsRootPlusName()
        {
            // Tiger is wired to the Tigris_Prowler model.
            Assert.AreEqual("Models/Creatures/Tigris_Prowler/Tigris_Prowler", CreatureModelNames.ResourcePath(CreatureKind.Tiger));
            StringAssert.StartsWith(CreatureModelNames.ResourceRoot, CreatureModelNames.ResourcePath(CreatureKind.Tiger));
        }

        [Test]
        public void EveryKindIsAliasedToAModel()
        {
            // The full roster has been generated — every kind maps to a real model folder (no enum-name fallback).
            foreach (CreatureKind k in System.Enum.GetValues(typeof(CreatureKind)))
                Assert.IsTrue(CreatureModelNames.Aliases.ContainsKey(k), k + " is missing a model alias");
        }

        [Test]
        public void EveryKindResolvesToANonEmptyFileName()
        {
            foreach (CreatureKind k in System.Enum.GetValues(typeof(CreatureKind)))
            {
                string path = CreatureModelNames.ResourcePath(k);
                Assert.IsFalse(string.IsNullOrEmpty(path), k + " has no model path");
                Assert.IsFalse(path.EndsWith("/"), k + " resolved to an empty file name");
            }
        }

        [Test]
        public void AliasesWhenPresentAreNonEmpty()
        {
            // Guards against an alias entry that maps a kind to "" (which would collapse to an empty file name).
            foreach (var kv in CreatureModelNames.Aliases)
                Assert.IsFalse(string.IsNullOrEmpty(kv.Value), "alias for " + kv.Key + " is empty");
        }
    }
}
