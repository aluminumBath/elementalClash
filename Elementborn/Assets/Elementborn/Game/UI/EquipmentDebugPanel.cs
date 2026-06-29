using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class EquipmentDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private bool refreshEveryFrame = true;

        private void Reset()
        {
            text = GetComponentInChildren<Text>();
        }

        private void Update()
        {
            if (refreshEveryFrame)
            {
                Refresh();
            }
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (text == null)
            {
                return;
            }

            var tracker = PlayerEquipmentTracker.Ensure();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Equipment:");

            foreach (var item in tracker.Equipped)
            {
                if (item == null || item.Slot == EquipmentSlotType.None)
                {
                    continue;
                }

                string value = string.IsNullOrWhiteSpace(item.DisplayName) ? "(empty)" : item.DisplayName;
                sb.AppendLine($"- {item.Slot}: {value}");
            }

            sb.AppendLine("Bonuses:");
            foreach (GearStatType stat in System.Enum.GetValues(typeof(GearStatType)))
            {
                float flat = PlayerEquipmentTracker.GetFlatBonus(stat);
                float pct = PlayerEquipmentTracker.GetPercentBonus(stat);
                if (Mathf.Abs(flat) > 0.001f || Mathf.Abs(pct) > 0.001f)
                {
                    sb.AppendLine($"- {stat}: +{flat:0.#}, +{pct:0.#}%");
                }
            }

            text.text = sb.ToString();
        }
    }
}
