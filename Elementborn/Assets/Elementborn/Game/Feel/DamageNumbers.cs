using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// Self-bootstrapping global service that spawns a floating damage number at every real combat hit, listening
    /// on <see cref="CombatFeedback.Hit"/>. Numbers are tinted by element (<see cref="ElementColor"/>) and grow a
    /// little with the hit's strength. No scene wiring and no assets — spawning goes through
    /// <see cref="FloatingText"/>. Burn/DoT chip ticks never reach here: they apply below the <c>amount &gt;= 1</c>
    /// threshold that raises <see cref="CombatFeedback.Hit"/>.
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
            float intensity = HitFeedback.Intensity01(amount, 50f);
            FloatingText.Spawn(pos, DamagePopup.Format(amount), ElementColor.For(element), 36f, 0.1f * (1f + 0.5f * intensity));
        }
    }
}
