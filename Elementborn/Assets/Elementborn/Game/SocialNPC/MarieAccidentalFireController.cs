using UnityEngine;

namespace Elementborn.Game
{
    public sealed class MarieAccidentalFireController : MonoBehaviour
    {
        [SerializeField] private float flareIntervalSeconds = 18f;
        [SerializeField] private bool flaresEnabled = true;
        [SerializeField] private bool currentlyOnFire;
        [SerializeField] private int unrestDelta = 3;

        private float nextFlareTime;

        private void OnEnable()
        {
            nextFlareTime = Time.time + flareIntervalSeconds;
        }

        private void Update()
        {
            if (!flaresEnabled || currentlyOnFire || Time.time < nextFlareTime)
            {
                return;
            }

            TriggerAccidentalFlare();
        }

        public void TriggerAccidentalFlare()
        {
            currentlyOnFire = true;
            nextFlareTime = Time.time + flareIntervalSeconds;
            NotificationFeed.Post("Marie Conflag nods off, flirts in her sleep, and accidentally lights something on fire.", NotificationType.Quest);
            CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.Unrest, unrestDelta, "Marie accidentally started another small fire.");
        }

        public void ExtinguishFlare()
        {
            if (!currentlyOnFire)
            {
                return;
            }

            currentlyOnFire = false;
            NotificationFeed.Post("The fire around Marie is put out. She insists it was deliberate ambiance.", NotificationType.Info);
            CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.Unrest, -unrestDelta, "The player helped put out Marie's accidental fire.");
        }
    }
}
