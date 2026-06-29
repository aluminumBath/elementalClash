using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class NpcPlacementAdminTool : MonoBehaviour
    {
        [SerializeField] private NpcWorldEntryDefinition npc;
        [SerializeField] private bool createMarker = true;
        [SerializeField] private bool createJournalEntry = true;
        [SerializeField] private PrimitiveType placeholderPrimitive = PrimitiveType.Capsule;

        [ContextMenu("Spawn Placeholder For NPC")]
        public GameObject SpawnPlaceholder()
        {
            if (npc == null)
            {
                Debug.LogWarning("No NPC assigned to NpcPlacementAdminTool.");
                return null;
            }

            GameObject go = GameObject.CreatePrimitive(placeholderPrimitive);
            go.name = npc.DisplayName;
            go.transform.position = npc.WorldPosition == Vector3.zero ? transform.position : npc.WorldPosition;

            var marker = go.AddComponent<NpcWorldPlacementMarker>();
            marker.SetNpc(npc);

            var voice = go.AddComponent<NpcVoicePlaybackController>();
            voice.SetNpc(npc);

            var dialogue = go.AddComponent<NpcDialogueHookInteractable>();
            dialogue.SetNpc(npc);

            if (createMarker)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "npc_" + PlayerJournalTracker.Safe(npc.NpcId),
                    MapMarkerType.GuideNpc,
                    go.transform.position,
                    npc.DisplayName,
                    true,
                    npc.LocationName);
            }

            if (createJournalEntry)
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "npc_" + PlayerJournalTracker.Safe(npc.NpcId),
                    JournalEntryType.Character,
                    npc.DisplayName,
                    $"{npc.TitleOrRank}\n{npc.Notes}",
                    npc.Region,
                    npc.NpcId);
            }

            return go;
        }

        public void SetNpc(NpcWorldEntryDefinition value)
        {
            npc = value;
        }
    }
}
