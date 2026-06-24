namespace Elementborn.Core
{
    /// <summary>
    /// Turns distance travelled into footsteps: emits one step per <see cref="StrideLength"/> metres, so the
    /// faster you move the quicker the steps — and alternates feet. Pure and unit-tested; the Game-layer
    /// <c>ProceduralFootsteps</c> feeds it movement so footsteps work even before any animation clips carry
    /// authored events.
    /// </summary>
    public sealed class FootstepCadence
    {
        public float StrideLength { get; }
        private float _accum;
        private bool _leftNext = true;

        public FootstepCadence(float strideLength = 1.6f)
        {
            StrideLength = strideLength > 0.01f ? strideLength : 0.01f;
        }

        /// <summary>Add distance travelled this frame; returns how many steps to emit (usually 0 or 1).</summary>
        public int Accumulate(float distance)
        {
            if (distance <= 0f) return 0;
            _accum += distance;
            int steps = 0;
            while (_accum >= StrideLength) { _accum -= StrideLength; steps++; }
            return steps;
        }

        /// <summary>Which foot the next emitted step is; flips on each call.</summary>
        public bool NextIsLeft()
        {
            bool left = _leftNext;
            _leftNext = !_leftNext;
            return left;
        }

        public void Reset() { _accum = 0f; _leftNext = true; }
    }
}
