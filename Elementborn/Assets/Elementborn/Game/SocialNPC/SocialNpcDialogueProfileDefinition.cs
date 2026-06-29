using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Social NPC/Dialogue Profile", fileName = "SocialNpcDialogueProfile")]
    public sealed class SocialNpcDialogueProfileDefinition : ScriptableObject
    {
        [SerializeField] private string npcId = "";
        [SerializeField] private string displayName = "NPC";
        [SerializeField] private NpcWorldEntryDefinition npc;
        [TextArea]
        [SerializeField] private string summary = "";
        [SerializeField] private List<SocialNpcDialogueCue> cues = new List<SocialNpcDialogueCue>();

        public string NpcId => string.IsNullOrWhiteSpace(npcId) && npc != null ? npc.NpcId : npcId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) && npc != null ? npc.DisplayName : displayName;
        public NpcWorldEntryDefinition Npc => npc;
        public string Summary => summary;
        public IReadOnlyList<SocialNpcDialogueCue> Cues => cues;

        public SocialNpcDialogueCue FindCue(string keywordOrCueId)
        {
            if (string.IsNullOrWhiteSpace(keywordOrCueId))
            {
                return cues.Count > 0 ? cues[0] : null;
            }

            string needle = keywordOrCueId.Trim().ToLowerInvariant();
            return cues.Find(c =>
                c != null &&
                ((c.CueId ?? "").ToLowerInvariant() == needle ||
                 (!string.IsNullOrWhiteSpace(c.TriggerKeyword) && needle.Contains(c.TriggerKeyword.ToLowerInvariant()))));
        }
    }
}
