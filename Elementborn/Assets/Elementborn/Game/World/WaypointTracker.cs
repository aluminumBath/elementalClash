using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Simple current waypoint tracker for compass/minimap arrows.
    /// Can point at current objective, custom pins, boat, camp, etc.
    /// </summary>
    public sealed class WaypointTracker : MonoBehaviour
    {
        public static WaypointTracker Instance { get; private set; }

        [SerializeField] private bool hasWaypoint;
        [SerializeField] private Vector3 waypointWorldPosition;
        [SerializeField] private string waypointLabel = "";

        public bool HasWaypoint => hasWaypoint;
        public Vector3 WaypointWorldPosition => waypointWorldPosition;
        public string WaypointLabel => waypointLabel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static WaypointTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(WaypointTracker));
            return go.AddComponent<WaypointTracker>();
        }

        public static void SetWaypoint(Vector3 worldPosition, string label = "Waypoint")
        {
            var tracker = Ensure();
            tracker.hasWaypoint = true;
            tracker.waypointWorldPosition = worldPosition;
            tracker.waypointLabel = label ?? "Waypoint";
            NotificationFeed.Post($"Waypoint set: {tracker.waypointLabel}", NotificationType.Map);
        }


        public static void SetWaypoint(Vector3 worldPosition, string label, string contextId)
        {
            SetWaypoint(worldPosition, label);
        }

        public static void SetWaypointToMarker(string markerId)
        {
            foreach (var marker in PlayerMapMarkerTracker.GetAllMarkers())
            {
                if (marker != null && marker.MarkerId == markerId)
                {
                    SetWaypoint(marker.WorldPosition, marker.Label);
                    return;
                }
            }

            NotificationFeed.Post("Could not find that map marker.", NotificationType.Warning);
        }

        public static void ClearWaypoint()
        {
            var tracker = Ensure();
            tracker.hasWaypoint = false;
            tracker.waypointLabel = "";
            NotificationFeed.Post("Waypoint cleared.", NotificationType.Map);
        }
    }
}
