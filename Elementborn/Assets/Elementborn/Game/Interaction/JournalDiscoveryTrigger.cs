using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Add this to trigger volumes around discoveries to unlock journal/codex entries.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class JournalDiscoveryTrigger : MonoBehaviour
    {
        [SerializeField] private JournalEntryType entryType = JournalEntryType.Discovery;
        [SerializeField] private string entryId = "";
        [SerializeField] private string title = "Discovery";
        [TextArea]
        [SerializeField] private string body = "";
        [SerializeField] private string region = "";
        [SerializeField] private string relatedId = "";
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
            PlayerJournalTracker.AddOrUpdateEntry(entryId, entryType, title, body, region, relatedId);
        }
    }
}
