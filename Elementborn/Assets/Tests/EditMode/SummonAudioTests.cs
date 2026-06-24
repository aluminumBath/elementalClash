using NUnit.Framework;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class SummonAudioTests
    {
        [Test]
        public void EachTierMapsToItsOwnRevealSting()
        {
            Assert.AreEqual(SfxKind.SummonRare, AudioController.SfxForSummon(SummonRarity.Rare));
            Assert.AreEqual(SfxKind.SummonEpic, AudioController.SfxForSummon(SummonRarity.Epic));
            Assert.AreEqual(SfxKind.SummonLegendary, AudioController.SfxForSummon(SummonRarity.Legendary));
        }

        [Test]
        public void TheThreeStingsAreDistinct()
        {
            var rare = AudioController.SfxForSummon(SummonRarity.Rare);
            var epic = AudioController.SfxForSummon(SummonRarity.Epic);
            var leg = AudioController.SfxForSummon(SummonRarity.Legendary);
            Assert.AreNotEqual(rare, epic);
            Assert.AreNotEqual(epic, leg);
            Assert.AreNotEqual(rare, leg);
        }
    }
}
