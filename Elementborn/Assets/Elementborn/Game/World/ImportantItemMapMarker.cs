using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Attach to important dropped items, rare weapons, treasure chests, or quest objects.
    /// It reports itself to the map while active and removes itself when picked up/disabled.
    /// </summary>
    public sealed class ImportantItemMapMarker : MonoBehaviour
    {
        [SerializeField] private MapMarkerType markerType = MapMarkerType.QuestItem;
        [SerializeField] private string markerId = "";
        [SerializeField] private string label = "Important Item";
        [SerializeField] private bool persistent = true;
        [SerializeField] private bool removeWhenDisabled = true;

        private string resolvedMarkerId;

        private void OnEnable()
        {
            resolvedMarkerId = string.IsNullOrWhiteSpace(markerId)
                ? PlayerMapMarkerTracker.PositionMarkerId(markerType.ToString(), transform.position)
                : markerId;

            PlayerMapMarkerTracker.ReportOrUpdateMarker(
                resolvedMarkerId,
                markerType,
                transform.position,
                label,
                isPersistent: persistent);
        }

        private void OnDisable()
        {
            if (removeWhenDisabled && !string.IsNullOrWhiteSpace(resolvedMarkerId))
            {
                PlayerMapMarkerTracker.RemoveMarker(resolvedMarkerId);
            }
        }
    }
}
