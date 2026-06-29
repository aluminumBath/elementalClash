using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class EnemyAiDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private EnemyCombatBrain brain;
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

            if (brain == null)
            {
                text.text = "No EnemyCombatBrain assigned.";
                return;
            }

            text.text =
                $"Enemy AI\n" +
                $"State: {brain.State}\n" +
                $"Profile: {(brain.Profile != null ? brain.Profile.DisplayName : "(none)")}\n" +
                $"Target: {(brain.Target != null ? brain.Target.name : "(none)")}";
        }
    }
}
