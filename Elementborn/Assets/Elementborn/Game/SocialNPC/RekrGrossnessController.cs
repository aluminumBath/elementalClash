using UnityEngine;

namespace Elementborn.Game
{
    public sealed class RekrGrossnessController : MonoBehaviour
    {
        [SerializeField] private int grossnessLevel = 50;

        public int GrossnessLevel => grossnessLevel;

        public void CoughLavaSmoke()
        {
            grossnessLevel = Mathf.Clamp(grossnessLevel + 5, 0, 100);
            NotificationFeed.Post("Rekr Ap coughs a little lava-smoke into a handkerchief that was probably clean once.", NotificationType.Info);
            CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.WindCapital, CapitalPressureType.Unrest, 1, "Rekr's grossness causes minor neighborhood discomfort.");
        }

        public void ReceiveRemedy()
        {
            grossnessLevel = Mathf.Clamp(grossnessLevel - 20, 0, 100);
            NotificationFeed.Post("Rekr grudgingly accepts the remedy and becomes slightly less alarming.", NotificationType.Info);
        }
    }
}
