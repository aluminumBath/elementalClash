using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Runtime IRandomSource backed by UnityEngine.Random (for non-deterministic in-game rolls).</summary>
    public sealed class UnityRandomSource : IRandomSource
    {
        public double NextUnit() => UnityEngine.Random.value;
    }
}
