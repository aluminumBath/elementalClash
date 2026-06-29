using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class HiddenChannelerSecretTracker : MonoBehaviour
    {
        public static HiddenChannelerSecretTracker Instance { get; private set; }

        [SerializeField] private List<string> discoveredSecretNpcIds = new List<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static HiddenChannelerSecretTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(HiddenChannelerSecretTracker));
            return go.AddComponent<HiddenChannelerSecretTracker>();
        }

        public bool IsKnown(string npcId)
        {
            return discoveredSecretNpcIds.Contains(npcId);
        }

        public void RevealSecret(NpcWorldEntryDefinition npc, string reason)
        {
            if (npc == null)
            {
                return;
            }

            if (!discoveredSecretNpcIds.Contains(npc.NpcId))
            {
                discoveredSecretNpcIds.Add(npc.NpcId);
            }

            NotificationFeed.Post($"{npc.DisplayName}'s secret was revealed: {reason}", NotificationType.Quest);
            PlayerJournalTracker.AddOrUpdateEntry(
                "secret_channeler_" + PlayerJournalTracker.Safe(npc.NpcId),
                JournalEntryType.Character,
                npc.DisplayName + " — Hidden Channeler",
                reason + "\n\n" + npc.Notes,
                npc.Region,
                npc.NpcId);
        }
    }
}
