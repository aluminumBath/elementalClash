namespace Elementborn.Core
{
    /// <summary>
    /// A stagger meter ("poise"). Hits add to it; when it reaches <see cref="Max"/> the target's poise breaks
    /// (the meter resets and the caller staggers them). It regenerates after a short lull since the last hit.
    /// Pure and UnityEngine-free so the break/regen rules are deterministic and unit-tested.
    /// </summary>
    public sealed class Poise
    {
        public float Max { get; }
        public float Current { get; private set; }

        private readonly float _regenPerSecond;
        private readonly float _regenDelay;
        private float _sinceHit;

        public Poise(float max, float regenPerSecond, float regenDelay)
        {
            Max = max > 0f ? max : 1f;
            _regenPerSecond = regenPerSecond < 0f ? 0f : regenPerSecond;
            _regenDelay = regenDelay < 0f ? 0f : regenDelay;
            _sinceHit = _regenDelay; // start rested
        }

        public float Fraction => Current / Max;

        /// <summary>Add poise damage from a hit. Returns true if this broke poise (meter reset → stagger).</summary>
        public bool AddHit(float poiseDamage)
        {
            _sinceHit = 0f;
            if (poiseDamage > 0f) Current += poiseDamage;
            if (Current >= Max) { Current = 0f; return true; }
            return false;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            _sinceHit += deltaTime;
            if (_sinceHit < _regenDelay) return;
            Current -= _regenPerSecond * deltaTime;
            if (Current < 0f) Current = 0f;
        }
    }
}
