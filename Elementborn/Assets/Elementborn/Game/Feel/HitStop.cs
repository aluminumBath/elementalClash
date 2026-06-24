using System.Collections;
using UnityEngine;
using Elementborn.Core;

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

        private void OnEnable() { AnimationEventReceiver.AnyImpacted += OnImpact; CombatFeedback.Hit += OnCombatHit; }
        private void OnDisable() { AnimationEventReceiver.AnyImpacted -= OnImpact; CombatFeedback.Hit -= OnCombatHit; }

        private void OnImpact(Vector3 _) => Freeze(0.06f, 0.10f);

        // Only heavy hits near the camera punch time, so routine chip damage and distant fights don't stutter.
        private void OnCombatHit(Vector3 pos, float amount, Element _)
        {
            if (Time.timeScale <= 0.01f) return; // don't fight a pause
            var cam = Camera.main;
            if (cam != null && (pos - cam.transform.position).sqrMagnitude > 144f) return;
            if (HitFeedback.Intensity01(amount, 55f) < 0.5f) return;
            Freeze(0.12f, 0.05f);
        }

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
