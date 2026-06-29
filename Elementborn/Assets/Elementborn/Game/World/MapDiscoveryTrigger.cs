using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Add this to trigger volumes around docks, shrines, caves, reef areas, vendors, etc.
    /// When the player enters, the marker is discovered and added to the map.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MapDiscoveryTrigger : MonoBehaviour
    {
        [SerializeField] private MapMarkerType markerType = MapMarkerType.Unknown;
        [SerializeField] private string markerId = "";
        [SerializeField] private string label = "";
        [SerializeField] private string notes = "";
        [SerializeField] private Transform markerLocation;
        [SerializeField] private bool discoverOnce = true;
        [SerializeField] private bool persistent = true;
        [SerializeField] private float expiresInSeconds = -1f;

        private bool discovered;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (discoverOnce && discovered)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            discovered = true;
            Vector3 position = markerLocation != null ? markerLocation.position : transform.position;
            string resolvedLabel = string.IsNullOrWhiteSpace(label)
                ? PlayerMapMarkerTracker.GetDefaultLabel(markerType)
                : label;

            PlayerMapMarkerTracker.ReportOrUpdateMarker(
                markerId: string.IsNullOrWhiteSpace(markerId)
                    ? PlayerMapMarkerTracker.PositionMarkerId(markerType.ToString(), position)
                    : markerId,
                markerType: markerType,
                worldPosition: position,
                label: resolvedLabel,
                isPersistent: persistent,
                expiresInSeconds: expiresInSeconds,
                notes: notes);
        }
    }
}
