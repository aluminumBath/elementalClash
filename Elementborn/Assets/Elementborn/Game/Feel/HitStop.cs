using System.Collections;
using UnityEngine;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// Brief slow-motion "hit-stop" on impact for punchier combat. Self-bootstrapping global service: it listens
    /// for <see cref="AnimationEventReceiver"/>'s static impact event and dips <see cref="Time.timeScale"/> for a
    /// few real milliseconds, restoring it on unscaled time. Overlapping impacts just refresh the window.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitStop : MonoBehaviour
    {
        private static HitStop _instance;
        private Coroutine _running;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("HitStop");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<HitStop>();
        }

        private void OnEnable() { AnimationEventReceiver.AnyImpacted += OnImpact; }
        private void OnDisable() { AnimationEventReceiver.AnyImpacted -= OnImpact; }

        private void OnImpact(Vector3 _) => Freeze(0.06f, 0.10f);

        /// <summary>Drop time scale to <paramref name="scale"/> for <paramref name="seconds"/> real seconds.</summary>
        public void Freeze(float scale, float seconds)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(FreezeRoutine(Mathf.Clamp01(scale), Mathf.Max(0f, seconds)));
        }

        private IEnumerator FreezeRoutine(float scale, float seconds)
        {
            Time.timeScale = scale;
            float t = 0f;
            while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
            Time.timeScale = 1f;
            _running = null;
        }
    }
}
