using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class CombatDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private SimpleCombatHealth watchedHealth;
        [SerializeField] private StatusEffectController watchedStatus;
        [SerializeField] private CombatResistanceProfile watchedResistances;
        [SerializeField] private bool refreshEveryFrame = true;

        private void Reset()
        {
            text = GetComponentInChildren<Text>();
        }

        private void Update()
        {
            if (refreshEveryFrame) Refresh();
        }

        public void Refresh()
        {
            if (text == null) return;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Combat");
            if (watchedHealth != null) sb.AppendLine($"HP: {watchedHealth.CurrentHealth:0.#}/{watchedHealth.MaxHealth:0.#}");
            if (watchedStatus != null)
            {
                sb.AppendLine($"Burn: {watchedStatus.HasStatus(StatusEffectType.Burn)}");
                sb.AppendLine($"Wet: {watchedStatus.HasStatus(StatusEffectType.Wet)}");
                sb.AppendLine($"Chilled: {watchedStatus.HasStatus(StatusEffectType.Chilled)}");
                sb.AppendLine($"Rooted: {watchedStatus.HasStatus(StatusEffectType.Rooted)}");
                sb.AppendLine($"Shocked: {watchedStatus.HasStatus(StatusEffectType.Shocked)}");
            }
            if (watchedResistances != null)
            {
                sb.AppendLine("Resistances:");
                foreach (var entry in watchedResistances.Entries)
                {
                    if (entry != null) sb.AppendLine($"- {entry.Element}: {entry.Percent:0.#}%");
                }
            }
            text.text = sb.ToString();
        }
    }
}
