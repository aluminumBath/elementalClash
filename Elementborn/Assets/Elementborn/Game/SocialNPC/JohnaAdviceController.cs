using UnityEngine;

namespace Elementborn.Game
{
    public sealed class JohnaAdviceController : MonoBehaviour
    {
        [SerializeField] private string[] adviceLines =
        {
            "Smoke rises because it has somewhere else to be. People do too.",
            "A capital does not fall in a day. It practices first.",
            "When everyone is shouting prophecy, listen for the person sweeping glass."
        };

        public void GiveAdvice()
        {
            string line = adviceLines.Length > 0 ? adviceLines[Random.Range(0, adviceLines.Length)] : "Breathe first. Decide second.";
            NotificationFeed.Post("Johna Rold: " + line, NotificationType.Info);
            CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.WindCapital, CapitalPressureType.Unrest, -2, "Johna's calm advice steadies the neighborhood.");
        }
    }
}
