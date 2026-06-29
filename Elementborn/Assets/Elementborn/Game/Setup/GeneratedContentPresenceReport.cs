using System.Text;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class GeneratedContentPresenceReport : MonoBehaviour
    {
        [SerializeField] private bool checkOnStart = true;

        private void Start()
        {
            if (checkOnStart)
            {
                Debug.Log(BuildReport());
            }
        }

        [ContextMenu("Print Generated Content Report")]
        public string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Elementborn Generated Content Presence");
            sb.AppendLine($"Runtime Bootstrap: {ElementbornFindUtility.FindFirst<ElementbornRuntimeBootstrap>() != null}");
            sb.AppendLine($"Quest UI Tracker: {ElementbornFindUtility.FindFirst<QuestUiTracker>() != null}");
            sb.AppendLine($"Notification Feed: {ElementbornFindUtility.FindFirst<NotificationFeed>() != null}");
            sb.AppendLine($"Player Inventory: {ElementbornFindUtility.FindFirst<PlayerInventoryTracker>() != null}");
            sb.AppendLine($"Ability Tracker: {ElementbornFindUtility.FindFirst<PlayerAbilityTracker>() != null}");
            sb.AppendLine($"Equipment Tracker: {ElementbornFindUtility.FindFirst<PlayerEquipmentTracker>() != null}");
            sb.AppendLine($"Spell Cooldown Tracker: {ElementbornFindUtility.FindFirst<SpellCooldownTracker>() != null}");
            sb.AppendLine($"Map Marker Tracker: {ElementbornFindUtility.FindFirst<PlayerMapMarkerTracker>() != null}");
            return sb.ToString();
        }
    }
}
