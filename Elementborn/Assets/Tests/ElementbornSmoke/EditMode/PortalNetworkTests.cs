using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;

namespace Elementborn.Tests.EditMode
{
    /// <summary>
    /// Coverage for the element-themed portal fast-travel model in Core (WorldMapLayout + FastTravelNetwork +
    /// PortalTheme). Reflection-based like the other Core tests, since this test assembly can't reference
    /// Assembly-CSharp directly. Verifies discovery gating, capital→city same-element routing, and the water
    /// portal's teal theme.
    /// </summary>
    public sealed class PortalNetworkTests
    {
        private static Type T(string fullName)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = a.GetType(fullName, false);
                if (t != null) return t;
            }
            Assert.Fail("Core type not found: " + fullName);
            return null;
        }

        private static object Element(string member) => Enum.Parse(T("Elementborn.Core.Element"), member);

        private static object BuildNetwork() =>
            T("Elementborn.Core.WorldMapLayout").GetMethod("BuildNetwork", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);

        private static object ICall(object inst, string method, params object[] args)
        {
            MethodInfo m = inst.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(m, Is.Not.Null, "method not found: " + method);
            return m.Invoke(inst, args);
        }

        private static int Count(object listResult) => ((ICollection)listResult).Count;

        [Test]
        public void Undiscovered_CityIsNotRoutable()
        {
            object net = BuildNetwork();
            Assert.That(Count(ICall(net, "DiscoveredCitiesOfElement", Element("Water"))), Is.EqualTo(0));
            Assert.That((bool)ICall(net, "CanRouteFromCapital", Element("Water"), "water_reefwood"), Is.False);
        }

        [Test]
        public void Discovering_AWaterCity_MakesItRoutableFromTheWaterCapital()
        {
            object net = BuildNetwork();
            Assert.That((bool)ICall(net, "Discover", "water_reefwood"), Is.True);
            Assert.That((bool)ICall(net, "CanRouteFromCapital", Element("Water"), "water_reefwood"), Is.True);
            Assert.That(Count(ICall(net, "DiscoveredCitiesOfElement", Element("Water"))), Is.EqualTo(1));
        }

        [Test]
        public void Routing_IsElementScopedAndCitiesOnly()
        {
            object net = BuildNetwork();
            ICall(net, "Discover", "water_reefwood");
            ICall(net, "Discover", "fire_ashmarket");

            Assert.That(Count(ICall(net, "DiscoveredCitiesOfElement", Element("Water"))), Is.EqualTo(1));
            Assert.That(Count(ICall(net, "DiscoveredCitiesOfElement", Element("Fire"))), Is.EqualTo(1));

            // A capital hub is never a travel target from the pool, even though it exists.
            Assert.That((bool)ICall(net, "CanRouteFromCapital", Element("Water"), "tide"), Is.False);
            // A different element's discovered city isn't reachable from the water pool.
            Assert.That((bool)ICall(net, "CanRouteFromCapital", Element("Water"), "fire_ashmarket"), Is.False);
        }

        [Test]
        public void WaterPortal_GlowsTeal()
        {
            object style = T("Elementborn.Core.PortalTheme")
                .GetMethod("For", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new[] { Element("Water") });
            object glow = style.GetType().GetField("Glow").GetValue(style);
            float r = (float)glow.GetType().GetField("r").GetValue(glow);
            float g = (float)glow.GetType().GetField("g").GetValue(glow);
            float b = (float)glow.GetType().GetField("b").GetValue(glow);
            Assert.That(g, Is.GreaterThan(0.5f), "teal is green-dominant");
            Assert.That(b, Is.GreaterThan(0.5f), "teal has strong blue");
            Assert.That(r, Is.LessThan(0.4f), "teal is low in red");
        }
    }
}
