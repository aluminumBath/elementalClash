using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// Tracks active status effects and derives their combined gameplay impact. Pure logic
    /// with an explicit <see cref="Tick"/>, so enemy AI can drive it and tests can advance
    /// time without Unity.
    /// </summary>
    public sealed class StatusController
    {
        private readonly List<StatusEffect> _active = new List<StatusEffect>();
        private readonly List<float> _remaining = new List<float>();

        public bool IsStunned { get; private set; }
        public bool IsControlled { get; private set; }
        /// <summary>Movement multiplier in [0,1]; 1 = unhindered, 0 = frozen/stunned/controlled.</summary>
        public float SpeedMultiplier { get; private set; } = 1f;
        public float BurnDamagePerSecond { get; private set; }
        public int ActiveCount => _active.Count;

        public void Add(StatusEffect status)
        {
            if (status.IsEmpty) return;
            _active.Add(status);
            _remaining.Add(status.Duration);
            Recalculate();
        }

        public void Tick(float deltaTime)
        {
            bool changed = false;
            for (int i = _remaining.Count - 1; i >= 0; i--)
            {
                _remaining[i] -= deltaTime;
                if (_remaining[i] <= 0f)
                {
                    _remaining.RemoveAt(i);
                    _active.RemoveAt(i);
                    changed = true;
                }
            }
            if (changed) Recalculate();
        }

        private void Recalculate()
        {
            bool stunned = false, controlled = false;
            float slow = 1f, burn = 0f;

            foreach (var s in _active)
            {
                switch (s.Kind)
                {
                    case StatusKind.Stun: stunned = true; break;
                    case StatusKind.Control: controlled = true; break;
                    case StatusKind.Slow: slow = Mathf.Min(slow, Mathf.Clamp01(1f - s.Magnitude)); break;
                    case StatusKind.Burn: burn += s.Magnitude; break;
                }
            }

            IsStunned = stunned;
            IsControlled = controlled;
            SpeedMultiplier = (stunned || controlled) ? 0f : slow;
            BurnDamagePerSecond = burn;
        }
    }
}
