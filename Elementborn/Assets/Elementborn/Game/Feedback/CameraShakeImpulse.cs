using System.Collections;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CameraShakeImpulse : MonoBehaviour
    {
        public static CameraShakeImpulse Instance { get; private set; }

        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float duration = 0.16f;
        [SerializeField] private float frequency = 28f;

        private Vector3 baseLocalPosition;
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

        private void Start()
        {
            if (cameraTarget == null && Camera.main != null)
            {
                cameraTarget = Camera.main.transform;
            }

            if (cameraTarget != null)
            {
                baseLocalPosition = cameraTarget.localPosition;
            }
        }

        public static CameraShakeImpulse Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(CameraShakeImpulse));
            return go.AddComponent<CameraShakeImpulse>();
        }

        public static void Shake(float strength)
        {
            Ensure().DoShake(strength);
        }

        public void DoShake(float strength)
        {
            if (strength <= 0f)
            {
                return;
            }

            if (cameraTarget == null && Camera.main != null)
            {
                cameraTarget = Camera.main.transform;
                baseLocalPosition = cameraTarget.localPosition;
            }

            if (cameraTarget == null)
            {
                return;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(ShakeRoutine(strength));
        }

        private IEnumerator ShakeRoutine(float strength)
        {
            float start = Time.unscaledTime;
            while (Time.unscaledTime < start + duration)
            {
                float t = (Time.unscaledTime - start) / Mathf.Max(0.01f, duration);
                float falloff = 1f - t;
                Vector3 offset = new Vector3(
                    Mathf.Sin(Time.unscaledTime * frequency),
                    Mathf.Cos(Time.unscaledTime * frequency * 1.23f),
                    0f) * strength * falloff;
                cameraTarget.localPosition = baseLocalPosition + offset;
                yield return null;
            }

            cameraTarget.localPosition = baseLocalPosition;
            routine = null;
        }
    }
}
