using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Trigger that changes reputation when the player enters it.
    /// Useful for quest milestones, trespassing zones, rescue events, faction territories, etc.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class FactionReputationTrigger : MonoBehaviour
    {
        [SerializeField] private ElementbornFactionId faction = ElementbornFactionId.Unknown;
        [SerializeField] private int amount = 1;
        [SerializeField] private string reason = "";
        [SerializeField] private bool once = true;

        private bool used;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (once && used)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            used = true;
            FactionReputationTracker.AddReputation(faction, amount, reason);
        }
    }
}
