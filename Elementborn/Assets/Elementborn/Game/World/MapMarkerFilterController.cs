using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class MapMarkerFilterController : MonoBehaviour
    {
        [SerializeField] private MapMarkerFilterState filters = new MapMarkerFilterState();

        public MapMarkerFilterState Filters => filters;

        public IEnumerable<TrackedMapMarkerRecord> GetVisibleMarkers()
        {
            foreach (var marker in PlayerMapMarkerTracker.GetAllMarkers())
            {
                if (marker == null || !marker.IsVisible || !marker.IsDiscovered)
                {
                    continue;
                }

                if (!filters.Allows(marker.MarkerType))
                {
                    continue;
                }

                yield return marker;
            }
        }

        public void SetShowPlayer(bool value) => filters.ShowPlayer = value;
        public void SetShowOwned(bool value) => filters.ShowOwned = value;
        public void SetShowItems(bool value) => filters.ShowItems = value;
        public void SetShowNpcs(bool value) => filters.ShowNpcs = value;
        public void SetShowThreats(bool value) => filters.ShowThreats = value;
        public void SetShowPlaces(bool value) => filters.ShowPlaces = value;
        public void SetShowCustomPins(bool value) => filters.ShowCustomPins = value;
        public void SetShowCompleted(bool value) => filters.ShowCompleted = value;
    }
}
