using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class WorldEventDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private bool refreshEveryFrame = true;

        private void Reset()
        {
            text = GetComponentInChildren<Text>();
        }

        private void Update()
        {
            if (refreshEveryFrame) Refresh();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (text == null) return;
            var tracker = WorldEventTracker.Ensure();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("World Events:");
            foreach (var record in tracker.Records)
            {
                if (record != null) sb.AppendLine($"- {record.DisplayName} | {record.EventType} | {record.State} | {record.Region}");
            }
            text.text = sb.ToString();
        }
    }
}
