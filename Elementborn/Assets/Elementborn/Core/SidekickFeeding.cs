using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// Tracks which of Willow's companions have been fed and when. Once every one has been fed and the feedings
    /// fall within the window (a couple of days), she'll share a hint toward a hidden ability. Pure — feed with
    /// explicit timestamps in tests.
    /// </summary>
    public sealed class SidekickFeedingTracker
    {
        private readonly Dictionary<WillowSidekick, double> _fed = new Dictionary<WillowSidekick, double>();

        public void Feed(WillowSidekick s, double atSeconds) => _fed[s] = atSeconds;
        public bool HasFed(WillowSidekick s) => _fed.ContainsKey(s);
        public int FedCount => _fed.Count;
        public bool AllFed => _fed.Count >= WillowSidekicks.All.Length;

        /// <summary>True once all companions are fed and the span between the first and last feeding is within
        /// <paramref name="windowSeconds"/> (e.g. two days), so it reflects sustained, recent care.</summary>
        public bool AllFedWithin(double windowSeconds)
        {
            if (!AllFed) return false;
            double earliest = double.MaxValue, latest = double.MinValue;
            foreach (var t in _fed.Values)
            {
                if (t < earliest) earliest = t;
                if (t > latest) latest = t;
            }
            return (latest - earliest) <= windowSeconds;
        }
    }

    /// <summary>The hint Willow gives once you've earned it — a nudge toward the hidden signature moves.</summary>
    public static class HiddenAbilityHint
    {
        public static string Text =>
            "You've a hidden move in you — water's spinning rush, earth's stone skin, air's whirlwind, fire's breath. " +
            "Find your element's secret gesture and let it loose.";
    }

    /// <summary>
    /// Parfa's two frogs bicker endlessly (one of air, one of water). Trick them into agreeing and he pays out a
    /// diamond — once. Pure state; the controller wires the trick and the reward.
    /// </summary>
    public sealed class FrogAccord
    {
        public bool Agreed { get; private set; }
        public bool RewardGiven { get; private set; }

        /// <summary>The trick lands — the frogs agree. Returns true the first time (reward is due).</summary>
        public bool Reconcile()
        {
            if (Agreed) return false;
            Agreed = true;
            return true;
        }

        public void MarkRewardGiven() => RewardGiven = true;
    }
}
