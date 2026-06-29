using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Attach to important NPCs once the player has met/discovered them.
    /// This is useful for Kram, vendors, trainers, healers, faction leaders, etc.
    /// </summary>
    public sealed class NpcMapMarkerReporter : MonoBehaviour
    {
        [SerializeField] private MapMarkerType markerType = MapMarkerType.GuideNpc;
        [SerializeField] private string npcDisplayName = "Guide";
        [SerializeField] private bool reportOnStart = true;
        [SerializeField] private bool updateEveryFrame = false;

        private string MarkerId => markerType.ToString().ToLowerInvariant() + "_" + PlayerMapMarkerTracker.SafeId(npcDisplayName);

        private void Start()
        {
            if (reportOnStart)
            {
                Report();
            }
        }

        private void Update()
        {
            if (updateEveryFrame)
            {
                Report();
            }
        }

        public void Report()
        {
            PlayerMapMarkerTracker.ReportOrUpdateMarker(
                MarkerId,
                markerType,
                transform.position,
                npcDisplayName,
                isPersistent: true);
        }
    }
}
