namespace Elementborn.Core
{
    /// <summary>
    /// Abstraction over randomness so the roll can be tested deterministically.
    /// Production code uses <see cref="SystemRandomSource"/>; tests inject a scripted source.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>Returns a value in the range [0, 1).</summary>
        double NextUnit();
    }

    /// <summary>Default RNG backed by System.Random. Seedable for reproducibility.</summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly System.Random _random;

        public SystemRandomSource() => _random = new System.Random();
        public SystemRandomSource(int seed) => _random = new System.Random(seed);

        public double NextUnit() => _random.NextDouble();
    }
}
