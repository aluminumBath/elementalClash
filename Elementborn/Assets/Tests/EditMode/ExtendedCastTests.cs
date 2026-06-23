using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ExtendedCastTests
    {
        [Test]
        public void ModifierRemapsCastsToAdvancedMoves()
        {
            Assert.AreEqual(IntentType.Heavy,     ExtendedCast.Remap(IntentType.PrimaryCast, true));
            Assert.AreEqual(IntentType.Sweep,     ExtendedCast.Remap(IntentType.SecondaryCast, true));
            Assert.AreEqual(IntentType.Signature, ExtendedCast.Remap(IntentType.Defend, true));
        }

        [Test]
        public void WithoutModifierCastsAreUnchanged()
        {
            Assert.AreEqual(IntentType.PrimaryCast,   ExtendedCast.Remap(IntentType.PrimaryCast, false));
            Assert.AreEqual(IntentType.SecondaryCast, ExtendedCast.Remap(IntentType.SecondaryCast, false));
            Assert.AreEqual(IntentType.Defend,        ExtendedCast.Remap(IntentType.Defend, false));
        }

        [Test]
        public void DashIsNeverRemapped()
        {
            Assert.AreEqual(IntentType.Dash, ExtendedCast.Remap(IntentType.Dash, true));
            Assert.AreEqual(IntentType.Dash, ExtendedCast.Remap(IntentType.Dash, false));
        }
    }
}
