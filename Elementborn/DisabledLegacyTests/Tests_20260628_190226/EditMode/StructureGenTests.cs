using NUnit.Framework;
using System;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class StructureGenTests
    {
        private static BuildingPlan Plan(StructureKind k, int seed = 1) =>
            StructureGenerator.Generate(k, k.ToString(), new SystemRandomSource(seed), new StructureGenConfig());

        [Test]
        public void EveryPoiTypeMapsToAStructure()
        {
            foreach (PoiType t in Enum.GetValues(typeof(PoiType)))
                Assert.IsTrue(Enum.IsDefined(typeof(StructureKind), StructureGenerator.KindFor(t)));
        }

        [Test]
        public void EveryKindProducesParts()
        {
            foreach (StructureKind k in Enum.GetValues(typeof(StructureKind)))
            {
                var plan = Plan(k);
                Assert.IsNotEmpty(plan.Parts, $"{k} produced no parts");
                Assert.Greater(plan.FootprintRadius, 0f);
            }
        }

        [Test]
        public void IsDeterministicForSameSeed()
        {
            foreach (StructureKind k in Enum.GetValues(typeof(StructureKind)))
            {
                var a = Plan(k, 5);
                var b = Plan(k, 5);
                Assert.AreEqual(a.Parts.Count, b.Parts.Count);
                for (int i = 0; i < a.Parts.Count; i++)
                {
                    Assert.AreEqual(a.Parts[i].Shape, b.Parts[i].Shape);
                    Assert.AreEqual(a.Parts[i].LocalPosition, b.Parts[i].LocalPosition);
                    Assert.AreEqual(a.Parts[i].Size, b.Parts[i].Size);
                }
            }
        }

        [Test]
        public void ScaleMultipliesGeometry()
        {
            var one = StructureGenerator.Generate(StructureKind.Cottage, "c", new SystemRandomSource(3), new StructureGenConfig { Scale = 1f });
            var two = StructureGenerator.Generate(StructureKind.Cottage, "c", new SystemRandomSource(3), new StructureGenConfig { Scale = 2f });
            Assert.AreEqual(one.Parts.Count, two.Parts.Count);
            for (int i = 0; i < one.Parts.Count; i++)
                Assert.AreEqual(one.Parts[i].Size * 2f, two.Parts[i].Size);
        }

        [Test]
        public void GeneratesFromPoi()
        {
            var poi = new PointOfInterest("p1", "Test Temple", PoiType.Temple, UnityEngine.Vector2.zero, 0, false);
            var plan = StructureGenerator.Generate(poi, new SystemRandomSource(1), new StructureGenConfig());
            Assert.AreEqual(StructureKind.Temple, plan.Kind);
            Assert.IsNotEmpty(plan.Parts);
        }
    }
}
