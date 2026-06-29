using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class CreatureBondDebugPanel : MonoBehaviour
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

            var tracker = CreatureBondingTracker.Ensure();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Owned Creatures:");

            foreach (var creature in tracker.OwnedCreatures)
            {
                if (creature == null)
                {
                    continue;
                }

                sb.AppendLine($"- {creature.NameForDisplay} | {creature.TraversalType} | {creature.State} | {creature.BondStage} ({creature.BondXp} XP)");
            }

            text.text = sb.ToString();
        }
    }
}
