using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class AbilityDebugPanel : MonoBehaviour
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

            var tracker = PlayerAbilityTracker.Ensure();
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"Level: {tracker.PlayerLevel}");
            sb.AppendLine($"Skill Points: {tracker.AvailableSkillPoints}");
            sb.AppendLine("Loadout:");

            foreach (var slot in tracker.Loadout)
            {
                if (slot != null)
                {
                    sb.AppendLine($"- {slot.SlotType}: {slot.AbilityId}");
                }
            }

            sb.AppendLine("Unlocked:");

            foreach (var ability in tracker.UnlockedAbilities)
            {
                if (ability != null)
                {
                    sb.AppendLine($"- {ability.AbilityId} | {ability.UnlockSource} | Used {ability.TimesUsed}");
                }
            }

            text.text = sb.ToString();
        }
    }
}
