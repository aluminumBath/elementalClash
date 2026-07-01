namespace Elementborn.Core
{
    /// <summary>How a player is currently approaching a landmark threshold. The Game layer fills this from the
    /// player's height/depth, loadout (Flight, water-breathing) and inventory; the gate turns it into a verdict.</summary>
    public readonly struct LandmarkApproach
    {
        /// <summary>Metres above sea level — for the storm-eye descent.</summary>
        public readonly float Altitude;
        /// <summary>Metres below the surface — for the deep-dive approach.</summary>
        public readonly float Depth;
        /// <summary>The player can fly (Air / Flight sub-art), needed to reach and drop through the storm's eye.</summary>
        public readonly bool CanFly;
        /// <summary>The player can breathe underwater right now (a Water Channeler, or an active Tideglass boon).</summary>
        public readonly bool CanBreatheUnderwater;
        /// <summary>The player is physically at the threshold object (the cave-portal, the waterfall, the village).</summary>
        public readonly bool AtThreshold;
        /// <summary>The player carries a recognition relic (Stormwarden's Token / Keelwood Splinter) so the Veil eases.</summary>
        public readonly bool HasRecognitionToken;

        public LandmarkApproach(float altitude = 0f, float depth = 0f, bool canFly = false,
                                bool canBreatheUnderwater = false, bool atThreshold = false,
                                bool hasRecognitionToken = false)
        {
            Altitude = altitude;
            Depth = depth;
            CanFly = canFly;
            CanBreatheUnderwater = canBreatheUnderwater;
            AtThreshold = atThreshold;
            HasRecognitionToken = hasRecognitionToken;
        }
    }

    /// <summary>A gate verdict: whether entry is allowed, and a line explaining why (shown to the player).</summary>
    public readonly struct AccessResult
    {
        public readonly bool Allowed;
        public readonly string Reason;
        private AccessResult(bool allowed, string reason) { Allowed = allowed; Reason = reason; }
        public static AccessResult Allow(string reason) => new AccessResult(true, reason);
        public static AccessResult Deny(string reason) => new AccessResult(false, reason);
    }

    /// <summary>
    /// The entry rules for each hidden landmark, as pure testable logic. This is where the design contract lives:
    /// Thalen'Veyr admits only a flier dropping through the storm's eye or a diver rising from the crushing deep
    /// (a recognition relic eases the storm); the Ashwind cave-portal and the Ilyrath waterfall-portal just need you
    /// at the threshold; and the Tidecaller tubes drop into deep water, so a non-water-breather must take a Tideglass
    /// Draught first. The Game layer supplies the <see cref="LandmarkApproach"/> and acts on the verdict.
    /// </summary>
    public static class LandmarkAccessGate
    {
        /// <summary>Altitude a flier must reach to drop through the storm's eye onto Thalen'Veyr.</summary>
        public const float StormEyeAltitude = 120f;
        /// <summary>Depth a diver must reach to rise into Thalen'Veyr's shielded harbour from below.</summary>
        public const float DeepDiveDepth = 60f;
        /// <summary>A recognition relic makes the Veil "remember" you, halving the altitude/depth required.</summary>
        public const float RecognitionEase = 0.5f;

        public static AccessResult Evaluate(Landmark landmark, LandmarkApproach a)
        {
            switch (LandmarkCatalog.For(landmark).Access)
            {
                case LandmarkAccess.AerialDescentOrDeepDive:
                {
                    float ease = a.HasRecognitionToken ? RecognitionEase : 1f;
                    float eyeAltitude = StormEyeAltitude * ease;
                    float diveDepth = DeepDiveDepth * ease;

                    bool fromAbove = a.CanFly && a.Altitude >= eyeAltitude;
                    bool fromBelow = a.CanBreatheUnderwater && a.Depth >= diveDepth;
                    if (fromAbove)
                        return AccessResult.Allow("You drop through the eye of the storm and the isle opens below.");
                    if (fromBelow)
                        return AccessResult.Allow("You rise into the shielded harbour from the crushing deep.");

                    if (!a.CanFly && !a.CanBreatheUnderwater)
                        return AccessResult.Deny("The Veil of Tempests turns you away. Only a flier from far above, or a diver from far below, may pass.");
                    if (a.CanFly)
                        return AccessResult.Deny("Climb higher — you must drop through the eye of the storm itself.");
                    return AccessResult.Deny("Dive deeper — the way in lies far beneath the waves.");
                }

                case LandmarkAccess.VolcanicCavePortal:
                    if (!a.AtThreshold)
                        return AccessResult.Deny("Seek the portal hidden in a lava cave at the volcano's foot.");
                    return AccessResult.Allow("The cave-portal flares; you step through into the caldera.");

                case LandmarkAccess.WaterfallPortal:
                    if (!a.AtThreshold)
                        return AccessResult.Deny("Find the great waterfall; its curtain is the way in.");
                    return AccessResult.Allow("You step through the falling water into the hidden cavern-basin.");

                case LandmarkAccess.FloatingVillageTube:
                    if (!a.AtThreshold)
                        return AccessResult.Deny("Reach the floating village drifting on the open sea.");
                    if (!a.CanBreatheUnderwater)
                        return AccessResult.Deny("The tubes drop into deep water — take a breath of the deep (a Tideglass Draught) before you descend.");
                    return AccessResult.Allow("You slip into a shimmering tube and sink toward the bubble-city.");

                default:
                    return AccessResult.Deny("");
            }
        }

        /// <summary>Convenience: can this approach enter right now?</summary>
        public static bool CanEnter(Landmark landmark, LandmarkApproach approach) => Evaluate(landmark, approach).Allowed;
    }
}
