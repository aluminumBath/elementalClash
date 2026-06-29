using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ManonCleanlinessController : MonoBehaviour
    {
        [SerializeField] private int cleanlinessStress = 60;

        public void CleanUpChaos()
        {
            cleanlinessStress = Mathf.Clamp(cleanlinessStress - 15, 0, 100);
            NotificationFeed.Post("Manon restores order with terrifying efficiency.", NotificationType.Info);
            CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.WindCapital, CapitalPressureType.Unrest, -2, "Manon cleaned up a neighborhood mess.");
        }

        public void NoticeMess()
        {
            cleanlinessStress = Mathf.Clamp(cleanlinessStress + 10, 0, 100);
            NotificationFeed.Post("Manon notices a mess and becomes visibly more precise.", NotificationType.Info);
        }
    }
}
