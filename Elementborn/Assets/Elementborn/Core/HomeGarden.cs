namespace Elementborn.Core
{
    /// <summary>
    /// The home Garden station: while built it slowly accrues silver that the player harvests from the home menu.
    /// Pure/UnityEngine-free (unit-testable); the Game layer ticks <see cref="Accrue"/> with frame time and grants
    /// the harvested silver. Accrual is capped so it can't run away while the player is idle or away.
    /// </summary>
    public sealed class HomeGarden
    {
        /// <summary>Silver produced per real hour while the Garden is built.</summary>
        public const int SilverPerHour = 24;
        /// <summary>Most silver the garden holds before it stops accruing (must be harvested to resume).</summary>
        public const int Cap = 240;

        private double _accrued;

        /// <summary>Silver ready to harvest right now (whole units, clamped to the cap).</summary>
        public int Ready
        {
            get
            {
                double v = _accrued < 0 ? 0 : (_accrued > Cap ? Cap : _accrued);
                return (int)System.Math.Floor(v);
            }
        }

        /// <summary>Advance the garden by the given real seconds (no-op for non-positive input).</summary>
        public void Accrue(double seconds)
        {
            if (seconds <= 0) return;
            _accrued += seconds * SilverPerHour / 3600.0;
            if (_accrued > Cap) _accrued = Cap;
        }

        /// <summary>Take everything ready; returns the silver granted and reduces the accrual by that much.</summary>
        public int Harvest()
        {
            int amount = Ready;
            _accrued -= amount;
            if (_accrued < 0) _accrued = 0;
            return amount;
        }

        public double Save() => _accrued;
        public void Restore(double v) => _accrued = v < 0 ? 0 : (v > Cap ? Cap : v);
    }
}
