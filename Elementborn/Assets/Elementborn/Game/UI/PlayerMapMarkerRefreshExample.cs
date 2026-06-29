using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Example adapter for wiring the tracker into your real map UI.
    /// This logs what should be shown. Replace AddOrUpdateMarkerInUi / HideMarkerInUi
    /// with your actual marker creation/update logic.
    /// </summary>
    public sealed class PlayerMapMarkerRefreshExample : MonoBehaviour
    {
        [SerializeField] private MapMarkerIconLibrary iconLibrary;

        public void RefreshFromTracker()
        {
            IReadOnlyList<TrackedMapMarkerRecord> markers = PlayerMapMarkerTracker.GetAllMarkers();
            if (markers == null)
            {
                return;
            }

            foreach (var marker in markers)
            {
                if (marker == null)
                {
                    continue;
                }

                if (!marker.IsVisible || !marker.IsDiscovered)
                {
                    HideMarkerInUi(marker.MarkerId);
                    continue;
                }

                var icon = iconLibrary != null ? iconLibrary.Resolve(marker) : null;
                AddOrUpdateMarkerInUi(marker, icon);
            }
        }

        private void AddOrUpdateMarkerInUi(TrackedMapMarkerRecord marker, Sprite icon)
        {
            string iconName = icon != null ? icon.name : "(no icon)";
            Debug.Log($"[Map marker example] Show '{marker.MarkerId}' / '{marker.Label}' / {marker.MarkerType} / icon={iconName} at {marker.WorldPosition}");
        }

        private void HideMarkerInUi(string markerId)
        {
            Debug.Log($"[Map marker example] Hide '{markerId}'");
        }
    }
}
