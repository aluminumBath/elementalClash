using System;

namespace Elementborn.Core
{
    /// <summary>
    /// Arcade scoring with a decaying combo multiplier. Pure logic: feed it kills and ticks, read the
    /// score/combo, subscribe to the change events. No Unity dependency, so it unit-tests directly.
    /// </summary>
    public sealed class ScoreSystem
    {
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int Combo { get; private set; }
        public float ComboWindow { get; set; } = 4f;
        public int MaxCombo { get; set; } = 8;

        public event Action<int> ScoreChanged;
        public event Action<int> ComboChanged;

        private float _sinceKill;

        /// <summary>Current points multiplier — 1x at no/low combo, up to MaxCombo.</summary>
        public int Multiplier => Math.Max(1, Math.Min(Combo, MaxCombo));

        /// <summary>Award a kill worth <paramref name="basePoints"/>; returns the points actually gained.</summary>
        public int AddKill(int basePoints)
        {
            Combo = Math.Min(Combo + 1, MaxCombo);
            _sinceKill = 0f;
            int gained = Math.Max(0, basePoints) * Multiplier;
            Score += gained;
            if (Score > HighScore) HighScore = Score;
            ComboChanged?.Invoke(Combo);
            ScoreChanged?.Invoke(Score);
            return gained;
        }

        /// <summary>Advance the combo timer; the combo resets after ComboWindow seconds without a kill.</summary>
        public void Tick(float deltaTime)
        {
            if (Combo <= 0) return;
            _sinceKill += deltaTime;
            if (_sinceKill >= ComboWindow)
            {
                Combo = 0;
                ComboChanged?.Invoke(Combo);
            }
        }

        public void ResetCombo()
        {
            if (Combo == 0) return;
            Combo = 0;
            ComboChanged?.Invoke(Combo);
        }

        public void Reset()
        {
            Score = 0;
            Combo = 0;
            _sinceKill = 0f;
            ScoreChanged?.Invoke(Score);
            ComboChanged?.Invoke(Combo);
        }
    }
}
