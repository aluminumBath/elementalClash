namespace Elementborn.Core
{
    public enum CareVerdict { Cared, Neglected, Abused }

    /// <summary>
    /// How well a keeper is treating their creatures, as a 0..1 score. Kindness (feeding, rest) raises it,
    /// mistreatment lowers it; the verdict drives Kiana's response. Pure, so it unit-tests directly.
    /// </summary>
    public sealed class CareTracker
    {
        public const float AbuseThreshold = 0.25f;
        public const float NeglectThreshold = 0.5f;

        public float Score { get; private set; }

        public CareTracker(float start = 0.7f) => Score = Clamp01(start);

        public void CareFor(float amount = 0.2f) => Score = Clamp01(Score + amount);
        public void Mistreat(float amount = 0.35f) => Score = Clamp01(Score - amount);

        public CareVerdict Verdict =>
            Score < AbuseThreshold ? CareVerdict.Abused :
            Score < NeglectThreshold ? CareVerdict.Neglected : CareVerdict.Cared;

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
