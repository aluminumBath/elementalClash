namespace Elementborn.Core
{
    /// <summary>
    /// Pure helper for a defeat "shatter": evenly spreads N shards outward (around a ring, with an upward bias)
    /// with a touch of deterministic jitter so the burst looks organic but reproducible. UnityEngine-free and
    /// unit-tested; the runtime poof just turns these unit directions into velocities.
    /// </summary>
    public static class ShardBurst
    {
        /// <summary>
        /// Outward+up unit direction for shard <paramref name="index"/> of <paramref name="count"/>.
        /// <paramref name="upBias"/> lifts the whole burst (0 = flat ring, higher = more upward).
        /// </summary>
        public static void Direction(int index, int count, float upBias, out float x, out float y, out float z)
        {
            if (count < 1) count = 1;
            float ang = (index / (float)count) * 2f * (float)System.Math.PI;
            // deterministic per-index jitter in [-0.5, 0.5], scaled to a small angular wobble
            float jitter = (((index * 9301 + 49297) % 233280) / 233280f) - 0.5f;
            ang += jitter * 0.6f;

            x = (float)System.Math.Cos(ang);
            z = (float)System.Math.Sin(ang);
            y = upBias < 0f ? 0f : upBias;

            float len = (float)System.Math.Sqrt(x * x + y * y + z * z);
            if (len > 1e-5f) { x /= len; y /= len; z /= len; }
        }
    }
}
