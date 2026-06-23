namespace Elementborn.Core
{
    /// <summary>
    /// The non-VR "power modifier" mapping. While the modifier is held, the three offensive cast buttons become
    /// the advanced moves — Primary→Heavy, Secondary→Sweep, Defend→Signature; everything else (Dash, etc.) is
    /// unchanged. Pure, so it can be unit-tested and reused; <c>FlatInputProvider</c> reads the live modifier
    /// state and routes each emit through here. (VR triggers these via gestures instead.)
    /// </summary>
    public static class ExtendedCast
    {
        public static IntentType Remap(IntentType baseIntent, bool modifierHeld)
        {
            if (!modifierHeld) return baseIntent;
            switch (baseIntent)
            {
                case IntentType.PrimaryCast:   return IntentType.Heavy;
                case IntentType.SecondaryCast: return IntentType.Sweep;
                case IntentType.Defend:        return IntentType.Signature;
                default:                        return baseIntent;
            }
        }
    }
}
