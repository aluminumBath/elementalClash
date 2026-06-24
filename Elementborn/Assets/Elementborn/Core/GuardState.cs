namespace Elementborn.Core
{
    /// <summary>How an incoming hit met the player's guard.</summary>
    public enum GuardResult { None, Blocked, Parried }

    /// <summary>
    /// The player's guard: hold to raise it, and the first sliver of time after raising is a <em>parry window</em>.
    /// A hit landing in that window is parried (negated, and the game counter-staggers the attacker); after it, the
    /// raised guard merely blocks (a big damage cut). One parry per raise. Pure and UnityEngine-free so the timing
    /// rules are deterministic and unit-tested; <c>PlayerGuardController</c> drives it from input + the damage hook.
    /// </summary>
    public sealed class GuardState
    {
        private readonly float _parryWindow;
        private readonly float _blockReduction;
        private readonly float _blockPoiseFactor;
        private bool _guarding;
        private float _heldTime;
        private bool _parrySpent;

        public GuardState(float parryWindow, float blockReduction, float blockPoiseFactor)
        {
            _parryWindow = parryWindow < 0f ? 0f : parryWindow;
            _blockReduction = Clamp01(blockReduction);
            _blockPoiseFactor = Clamp01(blockPoiseFactor);
        }

        public bool IsGuarding => _guarding;
        public bool InParryWindow => _guarding && !_parrySpent && _heldTime <= _parryWindow;
        public float BlockReduction => _blockReduction;
        public float BlockPoiseFactor => _blockPoiseFactor;

        public void Raise()
        {
            if (_guarding) return;
            _guarding = true;
            _heldTime = 0f;
            _parrySpent = false;
        }

        public void Lower() => _guarding = false;

        public void Tick(float deltaTime)
        {
            if (_guarding && deltaTime > 0f) _heldTime += deltaTime;
        }

        /// <summary>Classify an incoming hit and update state. A parry is consumed once per raise.</summary>
        public GuardResult Resolve()
        {
            if (!_guarding) return GuardResult.None;
            if (!_parrySpent && _heldTime <= _parryWindow)
            {
                _parrySpent = true;
                return GuardResult.Parried;
            }
            return GuardResult.Blocked;
        }

        /// <summary>Damage multiplier for a result: parried negates, blocked reduces, none is full.</summary>
        public float DamageMultiplier(GuardResult result)
        {
            if (result == GuardResult.Parried) return 0f;
            if (result == GuardResult.Blocked) return 1f - _blockReduction;
            return 1f;
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
