using UnityEngine;

namespace Elementborn.Game
{
    public sealed class StorySystemsDebugDashboardInput : MonoBehaviour
    {
        [SerializeField] private StorySystemsDebugDashboard dashboard;
        [SerializeField] private KeyCode refreshKey = KeyCode.F10;
        [SerializeField] private KeyCode saveKey = KeyCode.F11;
        [SerializeField] private KeyCode loadKey = KeyCode.F12;

        private void Awake()
        {
            if (dashboard == null)
            {
                dashboard = GetComponent<StorySystemsDebugDashboard>();
            }
        }

        private void Update()
        {
            if (dashboard == null)
            {
                return;
            }

            if (Input.GetKeyDown(refreshKey))
            {
                dashboard.Refresh();
            }

            if (Input.GetKeyDown(saveKey))
            {
                dashboard.SaveNarrativeSlotZero();
            }

            if (Input.GetKeyDown(loadKey))
            {
                dashboard.LoadNarrativeSlotZero();
            }
        }
    }
}
