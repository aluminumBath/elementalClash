namespace Elementborn.Core
{
    public enum LogSeverity { Info, Warning, Error }

    /// <summary>
    /// A fixed-capacity ring buffer of log lines (newest pushes out oldest). Pure and UnityEngine-free so the
    /// admin log overlay's storage is deterministic and unit-tested; the overlay just renders a snapshot.
    /// Indexing is oldest-first: <c>GetLine(0)</c> is the oldest retained line, <c>GetLine(Count-1)</c> the newest.
    /// </summary>
    public sealed class LogRing
    {
        private readonly string[] _lines;
        private readonly LogSeverity[] _sev;
        private int _start;
        private int _count;

        public LogRing(int capacity)
        {
            if (capacity < 1) capacity = 1;
            _lines = new string[capacity];
            _sev = new LogSeverity[capacity];
        }

        public int Capacity => _lines.Length;
        public int Count => _count;

        public void Add(string line, LogSeverity severity)
        {
            int slot = (_start + _count) % _lines.Length;
            if (_count < _lines.Length)
            {
                _count++;
            }
            else
            {
                // full: overwrite oldest and advance the window
                _start = (_start + 1) % _lines.Length;
            }
            _lines[slot] = line;
            _sev[slot] = severity;
        }

        public string GetLine(int index)
        {
            if (index < 0 || index >= _count) return null;
            return _lines[(_start + index) % _lines.Length];
        }

        public LogSeverity GetSeverity(int index)
        {
            if (index < 0 || index >= _count) return LogSeverity.Info;
            return _sev[(_start + index) % _lines.Length];
        }

        public void Clear()
        {
            _start = 0;
            _count = 0;
        }
    }
}
