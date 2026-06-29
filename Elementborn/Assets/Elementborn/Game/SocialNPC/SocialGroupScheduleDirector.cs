using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SocialGroupScheduleDirector : MonoBehaviour
    {
        [SerializeField] private string groupId = "wind_lower_terrace_circle";
        [SerializeField] private string[] rotatingEventIds =
        {
            "wind_lower_hangout",
            "marie_sleeping_flare_social",
            "amy_rumor_drift_social",
            "johna_pipe_counsel_social",
            "manon_cleanup_crisis_social"
        };
        [SerializeField] private float intervalSeconds = 60f;
        [SerializeField] private bool autoRotate = false;

        private int index;
        private float nextTime;

        private void OnEnable()
        {
            nextTime = Time.time + intervalSeconds;
        }

        private void Update()
        {
            if (!autoRotate || Time.time < nextTime)
            {
                return;
            }

            nextTime = Time.time + intervalSeconds;
            ActivateNextEvent();
        }

        public void ActivateNextEvent()
        {
            if (rotatingEventIds == null || rotatingEventIds.Length == 0)
            {
                return;
            }

            string id = rotatingEventIds[index % rotatingEventIds.Length];
            index++;
            SocialGroupRegistry.Ensure().ActivateEvent(id);
        }
    }
}
