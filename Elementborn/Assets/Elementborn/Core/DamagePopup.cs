namespace Elementborn.Core
{
    /// <summary>
    /// Pure presentation math for floating damage numbers: how to render the amount as text, and how a popup
    /// rises / fades / pops over its lifetime. UnityEngine-free and unit-tested so the look is deterministic and
    /// the runtime component stays a thin shell.
    /// </summary>
    public static class DamagePopup
    {
        /// <summary>The integer string shown for a hit. Rounds to nearest; any positive hit shows at least "1".</summary>
        public static string Format(float amount)
        {
            if (amount <= 0f) return "0";
            int n = (int)(amount + 0.5f);
            if (n < 1) n = 1;
            return n.ToString();
        }

        /// <summary>
        /// Lifetime curves for a popup. <paramref name="rise01"/> climbs 0→1 (ease-out), <paramref name="alpha"/>
        /// holds at 1 then fades to 0 over the last 40%, and <paramref name="scale"/> pops from 0.6→1 in the first
        /// 15% then holds. <paramref name="elapsed"/> is clamped to [0,lifetime].
        /// </summary>
        public static void Evaluate(float elapsed, float lifetime, out float rise01, out float alpha, out float scale)
        {
            if (lifetime <= 0f) { rise01 = 1f; alpha = 0f; scale = 1f; return; }
            float t = elapsed / lifetime;
            if (t < 0f) t = 0f; else if (t > 1f) t = 1f;

            float inv = 1f - t;
            rise01 = 1f - inv * inv;                 // ease-out climb

            alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
            if (alpha < 0f) alpha = 0f; else if (alpha > 1f) alpha = 1f;

            float p = t < 0.15f ? t / 0.15f : 1f;
            scale = 0.6f + 0.4f * p;
        }
    }
}
