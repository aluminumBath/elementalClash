using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>Optional debug/UX helper: rotates a small arrow object toward the active wind direction.</summary>
    public sealed class BoatWindHud : MonoBehaviour
    {
        [SerializeField] private Transform arrow;
        [SerializeField] private BoatController boat;

        private void Reset()
        {
            arrow = transform;
            boat = ElementbornFindUtility.FindFirst<BoatController>();
        }

        private void Update()
        {
            Transform target = arrow != null ? arrow : transform;
            Vector3 wind = WorldWindController.ActiveDirection;
            if (wind.sqrMagnitude > 0.001f)
                target.rotation = Quaternion.LookRotation(wind, Vector3.up);

            if (boat != null && boat.SailRaised)
            {
                float alignment = Vector3.Dot(boat.transform.forward, WorldWindController.ActiveDirection);
                // Hook for later UI: positive = tailwind, negative = headwind.
                _ = alignment;
            }
        }
    }
}
