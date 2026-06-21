using System.Collections.Generic;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    /// <summary>Test double that returns a predetermined sequence of unit values.</summary>
    public sealed class ScriptedRandomSource : IRandomSource
    {
        private readonly Queue<double> _values;
        private readonly double _fallback;

        public ScriptedRandomSource(double fallback, params double[] values)
        {
            _fallback = fallback;
            _values = new Queue<double>(values);
        }

        public double NextUnit() => _values.Count > 0 ? _values.Dequeue() : _fallback;
    }
}
