using UnityEngine;
using TMPro;
using Elementborn.Core;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// Self-bootstrapping global service that spawns a floating <see cref="FloatingNumber"/> at every real combat
    /// hit, listening on <see cref="CombatFeedback.Hit"/>. Numbers are tinted by element (<see cref="ElementColor"/>)
    /// and grow a little with the hit's strength. No scene wiring and no assets: text uses TMP's default font
    /// (the same one the project's UI relies on); if that font is unavailable the service simply stays silent
    /// rather than rendering broken text. Burn/DoT chip ticks never reach here — they apply below the
    /// <c>amount &gt;= 1</c> threshold that raises <see cref="CombatFeedback.Hit"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DamageNumbers : MonoBehaviour
    {
        private static DamageNumbers _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("DamageNumbers");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DamageNumbers>();
        }

        private void OnEnable() { CombatFeedback.Hit += OnHit; }
        private void OnDisable() { CombatFeedback.Hit -= OnHit; }

        private void OnHit(Vector3 pos, float amount, Element element)
        {
            if (amount < 1f) return;
            var font = TMP_Settings.defaultFontAsset;
            if (font == null) return; // no font set up → skip rather than show broken glyphs

            var go = new GameObject("DamageNumber");
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = font;
            tmp.fontSize = 36f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = DamagePopup.Format(amount);
            tmp.color = ElementColor.For(element);

            float intensity = HitFeedback.Intensity01(amount, 50f);
            go.AddComponent<FloatingNumber>().Play(pos, 0.1f * (1f + 0.5f * intensity));
        }
    }
}
