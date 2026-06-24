namespace Elementborn.Core
{
    public enum QteAction { North, South, East, West }
    public enum QtePress { Ignored, Correct, Wrong, Completed }

    /// <summary>
    /// A quick-time "complex move": a randomly generated series of direction/face-button presses
    /// (<see cref="QteAction"/>), each of which must be hit within a reaction-time window. Pure and UnityEngine-free
    /// so the rules are deterministic and unit-tested; the runtime controller maps device buttons onto the actions,
    /// ticks the clock, and fires the special move when the sequence completes.
    /// </summary>
    public sealed class QuickTimeSequence
    {
        /// <summary>Default per-step window, a touch above average human reaction time so it's demanding but fair.</summary>
        public const float DefaultWindowSeconds = 0.6f;

        private readonly QteAction[] _steps;
        private int _index;
        private float _timeInStep;

        public float WindowSeconds { get; }
        public int Length => _steps.Length;
        public int Index => _index;
        public bool Failed { get; private set; }
        public bool IsComplete => _index >= _steps.Length;
        public QteAction Current => _steps[_index < _steps.Length ? _index : _steps.Length - 1];

        public QuickTimeSequence(QteAction[] steps, float windowSeconds)
        {
            _steps = steps != null && steps.Length > 0 ? steps : new[] { QteAction.North };
            WindowSeconds = windowSeconds > 0.01f ? windowSeconds : 0.01f;
        }

        /// <summary>A random sequence of <paramref name="length"/> actions (no immediate repeats) via the seam RNG.</summary>
        public static QuickTimeSequence Generate(int length, float windowSeconds, IRandomSource rng)
        {
            if (length < 1) length = 1;
            var steps = new QteAction[length];
            int prev = -1;
            for (int i = 0; i < length; i++)
            {
                int pick = Roll(rng, prev);
                steps[i] = (QteAction)pick;
                prev = pick;
            }
            return new QuickTimeSequence(steps, windowSeconds);
        }

        private static int Roll(IRandomSource rng, int avoid)
        {
            double u = rng != null ? rng.NextUnit() : 0.0;
            if (u < 0d) u = 0d; else if (u >= 1d) u = 0.9999999d;
            int pick = (int)(u * 4d); // 0..3
            if (pick == avoid) pick = (pick + 1) % 4; // never repeat the previous action
            return pick;
        }

        /// <summary>0 (step just started) → 1 (window fully elapsed).</summary>
        public float Progress01
        {
            get
            {
                if (WindowSeconds <= 0f) return 1f;
                float p = _timeInStep / WindowSeconds;
                return p < 0f ? 0f : (p > 1f ? 1f : p);
            }
        }

        /// <summary>Seconds left in the current step's window.</summary>
        public float TimeRemaining
        {
            get { float r = WindowSeconds - _timeInStep; return r < 0f ? 0f : r; }
        }

        /// <summary>Advance the clock; returns true on the frame the window just expired (a failure).</summary>
        public bool Tick(float deltaTime)
        {
            if (IsComplete || Failed) return false;
            _timeInStep += deltaTime;
            if (_timeInStep >= WindowSeconds)
            {
                Failed = true;
                return true;
            }
            return false;
        }

        /// <summary>Register a button press against the current step.</summary>
        public QtePress Press(QteAction action)
        {
            if (IsComplete || Failed) return QtePress.Ignored;
            if (action != Current) { Failed = true; return QtePress.Wrong; }
            _index++;
            _timeInStep = 0f;
            return IsComplete ? QtePress.Completed : QtePress.Correct;
        }
    }
}
