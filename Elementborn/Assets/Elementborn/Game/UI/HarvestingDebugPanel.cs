using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class HarvestingDebugPanel : MonoBehaviour
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

            var tracker = ResourceHarvestingTracker.Ensure();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Harvesting");
            sb.AppendLine($"Total Harvests: {tracker.TotalHarvests}");
            sb.AppendLine($"Rare Harvests: {tracker.RareHarvests}");
            sb.AppendLine("Discovered Nodes:");
            foreach (string nodeId in tracker.DiscoveredNodeIds)
            {
                sb.AppendLine($"- {nodeId}");
            }

            text.text = sb.ToString();
        }
    }
}
