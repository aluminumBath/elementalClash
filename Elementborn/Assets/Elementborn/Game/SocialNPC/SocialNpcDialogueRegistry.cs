using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SocialNpcDialogueRegistry : MonoBehaviour
    {
        public static SocialNpcDialogueRegistry Instance { get; private set; }

        [SerializeField] private List<SocialNpcDialogueProfileDefinition> profiles = new List<SocialNpcDialogueProfileDefinition>();

        public IReadOnlyList<SocialNpcDialogueProfileDefinition> Profiles => profiles;

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

        public static SocialNpcDialogueRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(SocialNpcDialogueRegistry));
            return go.AddComponent<SocialNpcDialogueRegistry>();
        }

        public void SetProfiles(List<SocialNpcDialogueProfileDefinition> values)
        {
            profiles = values ?? new List<SocialNpcDialogueProfileDefinition>();
        }

        public SocialNpcDialogueProfileDefinition FindProfile(string npcId)
        {
            string needle = (npcId ?? "").Trim().ToLowerInvariant();
            return profiles.Find(p => p != null && (p.NpcId ?? "").ToLowerInvariant() == needle);
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Social NPC Dialogue Profiles");
            foreach (var profile in profiles)
            {
                if (profile != null)
                {
                    sb.AppendLine($"- {profile.DisplayName} ({profile.NpcId}): {profile.Cues.Count} cue(s)");
                }
            }

            if (profiles.Count == 0)
            {
                sb.AppendLine("- No social NPC dialogue profiles loaded.");
            }

            return sb.ToString();
        }
    }
}
