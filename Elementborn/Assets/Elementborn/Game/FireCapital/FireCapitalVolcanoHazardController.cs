using UnityEngine;

namespace Elementborn.Game
{
    public sealed class FireCapitalVolcanoHazardController : MonoBehaviour
    {
        [SerializeField] private int pressurePerPulse = 3;
        [SerializeField] private float pulseIntervalSeconds = 45f;
        [SerializeField] private bool pulseAutomatically = false;

        private float nextPulseTime;

        private void OnEnable()
        {
            nextPulseTime = Time.time + pulseIntervalSeconds;
        }

        private void Update()
        {
            if (!pulseAutomatically || Time.time < nextPulseTime)
            {
                return;
            }

            nextPulseTime = Time.time + pulseIntervalSeconds;
            PulseVolcanoPressure();
        }

        public void PulseVolcanoPressure()
        {
            CapitalWorldStateTracker.Ensure().AddPressure(
                CapitalId.FireCapital,
                CapitalPressureType.HiddenThreat,
                pressurePerPulse,
                "The volcano under the Fire Capital surges.");
            NotificationFeed.Post("The Fire Capital volcano rumbles under the caldera.", NotificationType.Info);
        }

        public void CalmVolcano()
        {
            CapitalWorldStateTracker.Ensure().AddPressure(
                CapitalId.FireCapital,
                CapitalPressureType.HiddenThreat,
                -pressurePerPulse * 2,
                "Emergency vents calm the volcano.");
            NotificationFeed.Post("Emergency vents draw heat away from the caldera.", NotificationType.Quest);
        }
    }
}
