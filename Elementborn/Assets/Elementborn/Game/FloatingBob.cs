using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// A cheap "alive" idle for placeholder landmark visuals: a gentle vertical bob plus a slow spin about the
    /// world vertical. The bob phase is seeded from world position so a field of them doesn't pulse in lockstep.
    /// Attach to the visual child (not the logic root). Purely cosmetic.
    /// </summary>
    public sealed class FloatingBob : MonoBehaviour
    {
        [SerializeField] private float bobAmplitude = 0.25f;          // metres
        [SerializeField] private float bobSpeed = 1.2f;               // radians/sec
        [SerializeField] private float spinDegreesPerSecond = 30f;

        private Vector3 _base;
        private float _phase;

        public void Configure(float amplitude, float bobsPerSecond, float spinDps)
        {
            bobAmplitude = amplitude;
            bobSpeed = bobsPerSecond;
            spinDegreesPerSecond = spinDps;
        }

        private void Start()
        {
            _base = transform.localPosition;
            _phase = (transform.position.x + transform.position.z) * 0.5f; // de-sync neighbours
        }

        private void Update()
        {
            float y = Mathf.Sin((Time.time + _phase) * bobSpeed) * bobAmplitude;
            transform.localPosition = _base + new Vector3(0f, y, 0f);
            if (spinDegreesPerSecond != 0f)
                transform.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
        }
    }
}
