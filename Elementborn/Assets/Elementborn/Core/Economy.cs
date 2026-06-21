using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>Currency denominations, low to high value. Silver is the base unit.</summary>
    public enum Currency { Silver, Ruby, Emerald, Sapphire, Diamond }

    /// <summary>
    /// A purse of mixed denominations. Internally everything is valued in silver, so prices are simple
    /// integers and spending "makes change" by re-minting the remainder into the fewest coins. Pure
    /// logic — no Unity dependency, unit-tested directly.
    /// </summary>
    public sealed class Wallet
    {
        private static readonly Currency[] HighToLow =
            { Currency.Diamond, Currency.Sapphire, Currency.Emerald, Currency.Ruby, Currency.Silver };

        private readonly Dictionary<Currency, int> _balances = new Dictionary<Currency, int>();

        /// <summary>Silver value of one coin of the given denomination (×5 ladder).</summary>
        public static long ValueOf(Currency c)
        {
            switch (c)
            {
                case Currency.Diamond: return 625;
                case Currency.Sapphire: return 125;
                case Currency.Emerald: return 25;
                case Currency.Ruby: return 5;
                default: return 1; // Silver
            }
        }

        public int CountOf(Currency c) => _balances.TryGetValue(c, out int n) ? n : 0;

        public long TotalValue
        {
            get
            {
                long t = 0;
                foreach (var kv in _balances) t += kv.Value * ValueOf(kv.Key);
                return t;
            }
        }

        public void Add(Currency c, int count)
        {
            if (count <= 0) return;
            _balances[c] = CountOf(c) + count;
        }

        /// <summary>Empty the purse (used before loading a save).</summary>
        public void Clear() => _balances.Clear();

        public bool CanAfford(long value) => TotalValue >= value;

        /// <summary>Spend a silver-equivalent value, re-minting the remainder into the fewest coins.
        /// Returns false (and changes nothing) if the purse can't cover it.</summary>
        public bool Spend(long value)
        {
            if (value < 0) return false;
            long total = TotalValue;
            if (total < value) return false;
            SetFromValue(total - value);
            return true;
        }

        private void SetFromValue(long value)
        {
            _balances.Clear();
            foreach (var c in HighToLow)
            {
                long v = ValueOf(c);
                if (value < v) continue;
                int n = (int)(value / v);
                if (n > 0) { _balances[c] = n; value -= n * v; }
            }
        }

        /// <summary>How a value breaks into coins (largest first), without touching any wallet.</summary>
        public static IReadOnlyDictionary<Currency, int> Breakdown(long value)
        {
            var d = new Dictionary<Currency, int>();
            foreach (var c in HighToLow)
            {
                long v = ValueOf(c);
                if (value < v) continue;
                int n = (int)(value / v);
                if (n > 0) { d[c] = n; value -= n * v; }
            }
            return d;
        }
    }
}
