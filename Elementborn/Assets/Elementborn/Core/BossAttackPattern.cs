namespace Elementborn.Core
{
    public enum BossAttackPhase { Cooldown, Telegraph }

    /// <summary>Cadence for a boss's telegraphed special: it charges for a cooldown, winds up through a telegraph
    /// (so the player can read and dodge it), then strikes for a single tick and resets. Engine-free + testable;
    /// the Game-layer <c>BossController</c> renders the wind-up tell and applies the hit when <see cref="Tick"/>
    /// reports a strike.</summary>
    public sealed class BossAttackPattern
    {
        private readonly float _cooldown;
        private readonly float _telegraph;
        private float _timer;
        private BossAttackPhase _phase = BossAttackPhase.Cooldown;

        public BossAttackPattern(float cooldownSeconds = 7f, float telegraphSeconds = 1.3f)
        {
            _cooldown = cooldownSeconds > 0.05f ? cooldownSeconds : 0.05f;
            _telegraph = telegraphSeconds > 0.05f ? telegraphSeconds : 0.05f;
            _timer = _cooldown;
        }

        public BossAttackPhase Phase => _phase;

        /// <summary>0..1 progress through the current wind-up (0 when not telegraphing).</summary>
        public float TelegraphProgress =>
            _phase == BossAttackPhase.Telegraph ? Clamp01(1f - _timer / _telegraph) : 0f;

        /// <summary>Advance by <paramref name="dt"/>. Returns true on the single tick the special strikes.</summary>
        public bool Tick(float dt)
        {
            _timer -= dt;
            if (_phase == BossAttackPhase.Cooldown)
            {
                if (_timer <= 0f) { _phase = BossAttackPhase.Telegraph; _timer = _telegraph; }
                return false;
            }

            if (_timer <= 0f)
            {
                _phase = BossAttackPhase.Cooldown;
                _timer = _cooldown;
                return true;
            }
            return false;
        }

        /// <summary>Back to a fresh cooldown with no pending wind-up (e.g. when the target leaves range).</summary>
        public void Reset()
        {
            _phase = BossAttackPhase.Cooldown;
            _timer = _cooldown;
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
