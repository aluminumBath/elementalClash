using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Optional helper for quickly adding NPC/world knowledge in scenes.
    /// Attach to NPCs, region controllers, tavern boards, or quest objects.
    /// </summary>
    public sealed class NpcMemorySeeder : MonoBehaviour
    {
        [SerializeField] private bool seedOnStart = true;
        [SerializeField] private DialogueMemoryType memoryType = DialogueMemoryType.WorldFact;
        [SerializeField] private string subject = "World";
        [TextArea]
        [SerializeField] private string value = "";
        [SerializeField] private string source = "";
        [SerializeField] private string region = "";
        [SerializeField] private string relatedQuestId = "";
        [SerializeField] private bool important = false;

        private void Start()
        {
            if (seedOnStart)
            {
                Seed();
            }
        }

        [ContextMenu("Seed Memory")]
        public void Seed()
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            DialogueMemoryTracker.Remember(
                memoryType,
                subject,
                value,
                string.IsNullOrWhiteSpace(source) ? name : source,
                region,
                relatedQuestId,
                important);
        }
    }
}
