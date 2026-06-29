using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Lightweight global wind source for sailing. It does not push rigidbodies by itself, so flying
    /// creatures remain unaffected unless a component explicitly reads this controller.
    /// </summary>
    public sealed class WorldWindController : MonoBehaviour
    {
        public static WorldWindController Instance { get; private set; }

        [Header("Wind")]
        [Tooltip("Direction the wind is blowing toward, in world space.")]
        [SerializeField] private Vector3 windDirection = new Vector3(0.65f, 0f, 0.35f);
        [SerializeField] private float baseSpeed = 9f;
        [SerializeField] private float gustStrength = 0.25f;
        [SerializeField] private float gustFrequency = 0.08f;

        public Vector3 Direction => Flatten(windDirection).normalized;
        public float Speed
        {
            get
            {
                float gust = 1f + Mathf.Sin(Time.time * Mathf.PI * 2f * gustFrequency) * gustStrength;
                return Mathf.Max(0f, baseSpeed * gust);
            }
        }

        public Vector3 Velocity => Direction * Speed;

        public static Vector3 ActiveVelocity => Instance != null ? Instance.Velocity : Vector3.forward * 9f;
        public static Vector3 ActiveDirection => Instance != null ? Instance.Direction : Vector3.forward;
        public static float ActiveSpeed => Instance != null ? Instance.Speed : 9f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetWind(Vector3 direction, float speed)
        {
            windDirection = Flatten(direction).sqrMagnitude > 0.001f ? Flatten(direction).normalized : Vector3.forward;
            baseSpeed = Mathf.Max(0f, speed);
        }

        public void RotateWind(float degrees)
        {
            windDirection = Quaternion.Euler(0f, degrees, 0f) * Direction;
        }

        private static Vector3 Flatten(Vector3 value) => new Vector3(value.x, 0f, value.z);

        private void OnDrawGizmosSelected()
        {
            Vector3 dir = Flatten(windDirection).sqrMagnitude > 0.001f ? Flatten(windDirection).normalized : Vector3.forward;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, dir * 4f);
        }
    }
}
