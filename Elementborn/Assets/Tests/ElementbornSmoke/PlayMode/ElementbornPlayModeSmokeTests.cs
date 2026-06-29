using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Elementborn.Tests.PlayMode
{
    public sealed class ElementbornPlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator PlayModeTestAssembly_IsDiscoveredAndCanAdvanceAFrame()
        {
            yield return null;
            Assert.That(Time.frameCount, Is.GreaterThanOrEqualTo(0));
        }

        [UnityTest]
        public IEnumerator PlayerAttunementHud_CanBeCreatedWithoutSerializedReferences()
        {
            Type hudType = FindType("Elementborn.Game.PlayerAttunementHud");
            Assert.That(hudType, Is.Not.Null);

            GameObject go = new GameObject("Smoke_PlayerAttunementHud");
            Component component = go.AddComponent(hudType);
            Assert.That(component, Is.Not.Null);

            yield return null;

            UnityEngine.Object.Destroy(go);
            yield return null;
        }

        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType(fullName, false))
                .FirstOrDefault(type => type != null);
        }
    }
}
