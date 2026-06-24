using UnityEngine;

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
        }

        private void OnDisable()
        {
            AnimationEventReceiver.AnyImpacted -= OnImpact;
            AnimationEventReceiver.AnyCastReleased -= OnCast;
        }

        private void OnImpact(Vector3 pos) => TransientLight.Flash(pos, new Color(1f, 0.95f, 0.8f), 3f, 4f, 0.18f);
        private void OnCast(Vector3 pos) => TransientLight.Flash(pos, new Color(0.5f, 0.9f, 1f), 5f, 7f, 0.3f);
    }
}
