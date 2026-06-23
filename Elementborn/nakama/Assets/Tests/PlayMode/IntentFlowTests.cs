using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Tests.PlayMode
{
    public class IntentFlowTests
    {
        private sealed class FakeInputProvider : MonoBehaviour, IPlayerInputProvider
        {
            public event System.Action<ChannelingIntent> IntentProduced;
            public void Emit(ChannelingIntent intent) => IntentProduced?.Invoke(intent);
        }

        [UnityTest]
        public IEnumerator Intent_FlowsThroughControllerToOutcome()
        {
            // Build inactive so we can wire the provider before OnEnable subscribes.
            var go = new GameObject("Player");
            go.SetActive(false);

            var fake = go.AddComponent<FakeInputProvider>();
            var controller = go.AddComponent<PlayerCombatController>();

            typeof(PlayerCombatController)
                .GetField("inputProviderBehaviour", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, fake);

            AbilityOutcome captured = AbilityOutcome.Empty;
            controller.OutcomeReady += (outcome, _) => captured = outcome;

            go.SetActive(true); // Awake + OnEnable run now, with the provider wired
            yield return null;

            fake.Emit(new ChannelingIntent(IntentType.PrimaryCast, Vector3.forward, 0.5f));
            yield return null;

            Assert.AreEqual(OutcomeKind.Projectile, captured.Kind);
            Assert.AreEqual(Element.Fire, captured.Element);
            Assert.Greater(captured.Damage, 0f);

            Object.Destroy(go);
        }
    }
}
