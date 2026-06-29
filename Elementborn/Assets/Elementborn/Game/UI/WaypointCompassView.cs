using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Minimal compass arrow. Put this on a UI arrow RectTransform and assign the player/camera transform.
    /// </summary>
    public sealed class WaypointCompassView : MonoBehaviour
    {
        [SerializeField] private Transform playerOrCamera;
        [SerializeField] private RectTransform arrow;
        [SerializeField] private Text distanceText;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Reset()
        {
            arrow = transform as RectTransform;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            var tracker = WaypointTracker.Instance;
            if (tracker == null || !tracker.HasWaypoint || playerOrCamera == null || arrow == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            Vector3 toWaypoint = tracker.WaypointWorldPosition - playerOrCamera.position;
            toWaypoint.y = 0f;

            if (toWaypoint.sqrMagnitude > 0.01f)
            {
                Vector3 local = playerOrCamera.InverseTransformDirection(toWaypoint.normalized);
                float angle = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
                arrow.localRotation = Quaternion.Euler(0f, 0f, -angle);
            }

            if (distanceText != null)
            {
                distanceText.text = $"{tracker.WaypointLabel} {toWaypoint.magnitude:0}m";
            }
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
    }
}
