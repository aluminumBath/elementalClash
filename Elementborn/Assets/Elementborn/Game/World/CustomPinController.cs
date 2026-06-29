using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Code-side utility for player-created pins.
    /// UI can call AddPin / RemovePin from map click handlers.
    /// </summary>
    public sealed class CustomPinController : MonoBehaviour
    {
        [SerializeField] private int maxPins = 50;

        public void AddPin(Vector3 worldPosition, string label = "Pinned Location", string notes = "")
        {
            var existing = PlayerMapMarkerTracker.GetAllMarkers();
            int pinCount = 0;
            foreach (var marker in existing)
            {
                if (marker.MarkerType == Elementborn.Core.MapMarkerType.CustomPin)
                {
                    pinCount++;
                }
            }

            if (pinCount >= maxPins)
            {
                Debug.LogWarning($"Cannot add custom map pin. Max pins reached: {maxPins}");
                return;
            }

            string id = PlayerMapMarkerTracker.PositionMarkerId("custom_pin", worldPosition);
            PlayerMapMarkerTracker.ReportCustomPin(id, worldPosition, label, notes);
        }

        public void RemovePin(string markerId)
        {
            PlayerMapMarkerTracker.RemoveMarker(markerId);
        }
    }
}
