using System.Collections;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class HitPauseController : MonoBehaviour
    {
        public static HitPauseController Instance { get; private set; }

        [SerializeField] private bool enabledHitPause = true;
        [SerializeField] private float minimumGapSeconds = 0.08f;

        private float lastPauseAt;
        private Coroutine routine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static HitPauseController Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(HitPauseController));
            return go.AddComponent<HitPauseController>();
        }

        public static void Pulse(float seconds)
        {
            Ensure().DoPulse(seconds);
        }

        public void DoPulse(float seconds)
        {
            if (!enabledHitPause || seconds <= 0f || Time.unscaledTime < lastPauseAt + minimumGapSeconds)
            {
                return;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(PauseRoutine(seconds));
        }

        private IEnumerator PauseRoutine(float seconds)
        {
            lastPauseAt = Time.unscaledTime;
            float previousScale = Time.timeScale;
            Time.timeScale = Mathf.Min(previousScale, 0.05f);
            yield return new WaitForSecondsRealtime(Mathf.Max(0.005f, seconds));
            Time.timeScale = previousScale;
            routine = null;
        }
    }
}
