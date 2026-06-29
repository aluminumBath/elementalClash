using NUnit.Framework;
using System;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class WeatherTests
    {
        [Test]
        public void ClearLeavesEveryElementUnchanged()
        {
            foreach (Element e in Enum.GetValues(typeof(Element)))
                Assert.AreEqual(1f, WeatherEffects.ElementMultiplier(WeatherKind.Clear, e));
        }

        [Test]
        public void RainFavoursWaterAndDampensFire()
        {
            Assert.Greater(WeatherEffects.ElementMultiplier(WeatherKind.Rain, Element.Water), 1f);
            Assert.Less(WeatherEffects.ElementMultiplier(WeatherKind.Rain, Element.Fire), 1f);
        }

        [Test]
        public void HeatHazeFavoursFireAndDampensWater()
        {
            Assert.Greater(WeatherEffects.ElementMultiplier(WeatherKind.HeatHaze, Element.Fire), 1f);
            Assert.Less(WeatherEffects.ElementMultiplier(WeatherKind.HeatHaze, Element.Water), 1f);
        }

        [Test]
        public void EffectsStayWithinSlightBounds()
        {
            foreach (WeatherKind w in Enum.GetValues(typeof(WeatherKind)))
                foreach (Element e in Enum.GetValues(typeof(Element)))
                {
                    float m = WeatherEffects.ElementMultiplier(w, e);
                    Assert.GreaterOrEqual(m, 0.7f);
                    Assert.LessOrEqual(m, 1.3f);
                }
        }

        [Test]
        public void DesertCanProduceSandstormOrHeatHaze()
        {
            var severe = WeatherProfiles.Severe(BiomeType.Desert);
            CollectionAssert.Contains(severe, WeatherKind.Sandstorm);
            CollectionAssert.Contains(severe, WeatherKind.HeatHaze);
        }

        [Test]
        public void PickIsDeterministicAndValidForBiome()
        {
            var a = WeatherProfiles.Pick(BiomeType.Desert, new SystemRandomSource(3));
            var b = WeatherProfiles.Pick(BiomeType.Desert, new SystemRandomSource(3));
            Assert.AreEqual(a, b);

            for (int seed = 0; seed < 80; seed++)
            {
                var w = WeatherProfiles.Pick(BiomeType.Volcano, new SystemRandomSource(seed));
                Assert.IsTrue(w == WeatherKind.Clear || w == WeatherKind.HeatHaze, $"got {w}");
            }
        }
    }
}
