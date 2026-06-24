using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// Self-bootstrapping global service that pops a brief light at combat moments: a small warm spark on impact and
    /// a larger cyan burst on a channeled cast release. Subscribes to <see cref="AnimationEventReceiver"/>'s static
    /// events and uses <see cref="TransientLight"/>, so there are no assets to wire.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FlashFeedback : MonoBehaviour
    {
        private static FlashFeedback _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("FlashFeedback");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<FlashFeedback>();
        }

        private void OnEnable()
        {
            AnimationEventReceiver.AnyImpacted += OnImpact;
            AnimationEventReceiver.AnyCastReleased += OnCast;
            CombatFeedback.Hit += OnCombatHit;
            CombatFeedback.Defeated += OnDefeated;
        }

        private void OnDisable()
        {
            AnimationEventReceiver.AnyImpacted -= OnImpact;
            AnimationEventReceiver.AnyCastReleased -= OnCast;
            CombatFeedback.Hit -= OnCombatHit;
            CombatFeedback.Defeated -= OnDefeated;
        }

        private void OnImpact(Vector3 pos) => TransientLight.Flash(pos, new Color(1f, 0.95f, 0.8f), 3f, 4f, 0.18f);
        private void OnCast(Vector3 pos) => TransientLight.Flash(pos, new Color(0.5f, 0.9f, 1f), 5f, 7f, 0.3f);

        // Every real hit pops a small element-tinted spark; a defeat throws a bigger burst.
        private void OnCombatHit(Vector3 pos, float amount, Element element)
            => TransientLight.Flash(pos, ColorFor(element), 2.5f + Mathf.Min(amount, 60f) * 0.05f, 4f, 0.14f);

        private void OnDefeated(Vector3 pos, Element element)
            => TransientLight.Flash(pos, ColorFor(element), 10f, 8f, 0.35f);

        private static Color ColorFor(Element e)
        {
            switch (e)
            {
                case Element.Fire: return new Color(1f, 0.55f, 0.2f);
                case Element.Water: return new Color(0.3f, 0.6f, 1f);
                case Element.Earth: return new Color(0.55f, 0.8f, 0.35f);
                case Element.Air: return new Color(0.7f, 0.95f, 1f);
                default: return Color.white;
            }
        }
    }
}
