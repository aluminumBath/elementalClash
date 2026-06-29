using System.Collections.Generic;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ArmorResistTests
    {
        // Cycle (each beats the next): Fire > Earth > Air > Water > Fire.

        [Test]
        public void NoEnchantsIsNeutral()
        {
            Assert.AreEqual(1f, ArmorResist.IncomingMultiplier(new List<Element>(), Element.Fire), 1e-4f);
            Assert.AreEqual(1f, ArmorResist.IncomingMultiplier(null, Element.Fire), 1e-4f);
        }

        [Test]
        public void FirePieceResistsWhatItBeatsAndFearsItsPredator()
        {
            var fire = new List<Element> { Element.Fire };
            Assert.Less(ArmorResist.IncomingMultiplier(fire, Element.Earth), 1f);    // Fire beats Earth -> resist
            Assert.Greater(ArmorResist.IncomingMultiplier(fire, Element.Water), 1f); // Water beats Fire -> weak
            Assert.AreEqual(1f, ArmorResist.IncomingMultiplier(fire, Element.Air), 1e-4f); // neutral
        }

        [Test]
        public void PiecesStackAdditively()
        {
            var one  = new List<Element> { Element.Fire };
            var four = new List<Element> { Element.Fire, Element.Fire, Element.Fire, Element.Fire };
            Assert.Greater(ArmorResist.IncomingMultiplier(four, Element.Water),
                           ArmorResist.IncomingMultiplier(one, Element.Water));
            Assert.AreEqual(1f + 4f * ArmorResist.WeakStep, ArmorResist.IncomingMultiplier(four, Element.Water), 1e-4f);
        }

        [Test]
        public void MixedEnchantsCanCancel()
        {
            // vs Water: Fire is weak (+step); Air resists (Air beats Water, -step) -> net neutral.
            var mixed = new List<Element> { Element.Fire, Element.Air };
            Assert.AreEqual(1f, ArmorResist.IncomingMultiplier(mixed, Element.Water), 1e-4f);
        }

        [Test]
        public void ResistNeverGoesBelowFloor()
        {
            var manyAir = new List<Element>();
            for (int i = 0; i < 8; i++) manyAir.Add(Element.Air); // all resist Water; 8*0.15 > 1
            Assert.AreEqual(ArmorResist.Floor, ArmorResist.IncomingMultiplier(manyAir, Element.Water), 1e-4f);
        }
    }
}
