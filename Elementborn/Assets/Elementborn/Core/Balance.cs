namespace Elementborn.Core
{
    /// <summary>
    /// The one place to tune Elementborn's balance. Two kinds of knob live here:
    ///
    ///  (1) <b>Global difficulty dials</b> — multipliers that scale a whole system at once (all default
    ///      <c>1.0</c>, i.e. no change). Systems migrate onto these incrementally; until a system reads its dial
    ///      the constant simply documents the intended knob, and the QA suite keeps every dial sane.
    ///
    ///  (2) <b>Sanity bounds</b> — the limits the automated QA suite (<c>BalanceSanityTests</c>) enforces against
    ///      the live content, so a careless tuning edit (a fat-fingered extra zero on a damage value, a gacha rate
    ///      that drifts out of "rare") fails a test instead of shipping. Edit the policy here, once.
    ///
    /// Pure and engine-free, so it is fully unit-testable and safe to read from any layer.
    /// </summary>
    public static class Balance
    {
        // ---- Global difficulty dials (default 1.0 = unchanged) ----------------------------------------------
        // Raise/lower to retune the whole game without hunting through systems. Wiring is incremental: a system
        // reads its dial when it migrates; the value here is the single source of truth for that knob.
        public const float EnemyHealthScale  = 1.0f; // every enemy/boss max-health
        public const float EnemyDamageScale  = 1.0f; // damage dealt to the player
        public const float PlayerDamageScale = 1.0f; // damage the player deals
        public const float RewardScale       = 1.0f; // silver / shard payouts
        public const float DropRateScale     = 1.0f; // loot-table weight bias toward drops
        public const float XpScale           = 1.0f; // experience gain
        public const float GachaRateScale    = 1.0f; // rate-up generosity

        // ---- Sanity bounds the QA suite enforces ------------------------------------------------------------
        public const float MinAbilityBaseDamage = 1f;    // no zero-damage attack ships
        public const float MaxAbilityBaseDamage = 200f;  // catches a fat-fingered extra zero
        public const float MaxDamageMultiplier  = 5f;    // a "bonus" multiplier this large is surely a typo
        public const double MaxLegendaryRate    = 0.05;  // a legendary gacha pull must stay genuinely rare
        public const double MaxEpicRate         = 0.30;  // epic stays uncommon
        public const int    MaxSingleSummonCost = 1000;  // sanity ceiling on the price of one pull
        public const float  MaxScale            = 10f;   // a difficulty dial above this is almost certainly wrong

        /// <summary>A difficulty dial is valid when it is positive and not absurdly large.</summary>
        public static bool ScaleIsSane(float scale) => scale > 0f && scale <= MaxScale;

        // ---- Scaling helpers (the one place the dials are actually applied) ---------------------------------
        // Systems call these where a value is produced or consumed, so the dial and its rounding live in one
        // tested spot. With every dial at 1.0 these are identities; a designer retunes by editing a dial above.
        public static float ScaledEnemyHealth(float raw)  => raw * EnemyHealthScale;
        public static float ScaledEnemyDamage(float raw)  => raw * EnemyDamageScale;
        public static float ScaledPlayerDamage(float raw) => raw * PlayerDamageScale;
        public static int   ScaledReward(int raw)         => RoundCount(raw * RewardScale);
        public static int   ScaledXp(int raw)             => RoundCount(raw * XpScale);
        public static int   ScaledDropWeight(int raw)     => RoundCount(raw * DropRateScale);

        // Rewards, XP, and weights are whole numbers: round to nearest, never let a positive input scale away to
        // nothing (a 1-reward must not vanish), and never go negative.
        private static int RoundCount(float scaled)
        {
            if (scaled <= 0f) return 0;
            int r = (int)System.Math.Round((double)scaled);
            return r < 1 ? 1 : r;
        }
    }
}
