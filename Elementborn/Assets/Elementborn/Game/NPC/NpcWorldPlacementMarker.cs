using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class NpcWorldPlacementMarker : MonoBehaviour
    {
        [SerializeField] private NpcWorldEntryDefinition npc;
        [SerializeField] private bool addMapMarkerOnStart = true;
        [SerializeField] private bool addJournalEntryOnStart = true;

        private void Start()
        {
            if (npc == null)
            {
                return;
            }

            gameObject.name = npc.DisplayName;

            if (addMapMarkerOnStart)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "npc_" + PlayerJournalTracker.Safe(npc.NpcId),
                    MapMarkerType.GuideNpc,
                    npc.WorldPosition,
                    npc.DisplayName,
                    true,
                    npc.LocationName);
            }

            if (addJournalEntryOnStart)
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "npc_" + PlayerJournalTracker.Safe(npc.NpcId),
                    JournalEntryType.Character,
                    npc.DisplayName,
                    $"{npc.TitleOrRank}\n{npc.Notes}",
                    npc.Region,
                    npc.NpcId);
            }
        }

        public void SetNpc(NpcWorldEntryDefinition value)
        {
            npc = value;
        }
    }
}
