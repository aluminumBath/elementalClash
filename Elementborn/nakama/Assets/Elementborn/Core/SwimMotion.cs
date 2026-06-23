using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// Pure swim velocity. Blends a world-space move direction with a vertical (ascend/descend) input, scales
    /// it by the underwater speed factor, and caps the magnitude for comfort. The locomotion component supplies
    /// the world-space vectors (from the headset/look + sticks); this stays engine-light so it unit-tests.
    /// </summary>
    public static class SwimMotion
    {
        public static Vector3 Velocity(Vector3 worldMove, float verticalInput, float baseSpeed,
            float speedScale, float maxSpeed)
        {
            Vector3 dir = worldMove;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            dir += Vector3.up * Mathf.Clamp(verticalInput, -1f, 1f);

            Vector3 v = dir * (baseSpeed * Mathf.Max(0f, speedScale));
            float max = Mathf.Max(0f, maxSpeed);
            if (v.magnitude > max) v = v.normalized * max;
            return v;
        }
    }
}
