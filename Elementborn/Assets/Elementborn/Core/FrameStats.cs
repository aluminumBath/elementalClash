namespace Elementborn.Core
{
    /// <summary>A pure, engine-free rolling window of recent frame durations (in milliseconds) that yields the live
    /// average, min, peak, and FPS — the math behind the on-screen performance overlay and any automated frame-budget
    /// checks. The window is a fixed-size ring, so it's allocation-free after construction and deterministic, which
    /// makes it unit-testable away from the engine; <c>PerformanceHud</c> feeds it <c>unscaledDeltaTime</c> each
    /// frame.</summary>
    public sealed class FrameStats
    {
        private readonly double[] _ms;
        private int _count;
        private int _head;

        public FrameStats(int window = 120) { _ms = new double[window < 1 ? 1 : window]; }

        public int Capacity => _ms.Length;
        public int Samples => _count;
        public bool HasData => _count > 0;

        public void Push(double frameMs)
        {
            _ms[_head] = frameMs;
            _head = (_head + 1) % _ms.Length;
            if (_count < _ms.Length) _count++;
        }

        public double AverageMs
        {
            get
            {
                if (_count == 0) return 0.0;
                double sum = 0.0;
                for (int i = 0; i < _count; i++) sum += _ms[i];
                return sum / _count;
            }
        }

        public double MaxMs
        {
            get
            {
                double m = 0.0;
                for (int i = 0; i < _count; i++) if (_ms[i] > m) m = _ms[i];
                return m;
            }
        }

        public double MinMs
        {
            get
            {
                if (_count == 0) return 0.0;
                double m = double.MaxValue;
                for (int i = 0; i < _count; i++) if (_ms[i] < m) m = _ms[i];
                return m;
            }
        }

        public double Fps => AverageMs > 0.0001 ? 1000.0 / AverageMs : 0.0;

        public void Clear() { _count = 0; _head = 0; }
    }
}
