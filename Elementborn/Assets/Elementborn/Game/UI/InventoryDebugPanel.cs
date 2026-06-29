using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Minimal debug inventory text panel.
    /// Add it to a canvas and assign a Text field to inspect inventory without a full UI.
    /// </summary>
    public sealed class InventoryDebugPanel : MonoBehaviour
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

            var inventory = PlayerInventoryTracker.Ensure();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Coins: {inventory.Currency}");
            sb.AppendLine("Inventory:");

            foreach (var stack in inventory.Stacks)
            {
                if (stack == null || stack.IsEmpty)
                {
                    continue;
                }

                sb.AppendLine($"- {stack.DisplayName} x{stack.Quantity}");
            }

            text.text = sb.ToString();
        }
    }
}
