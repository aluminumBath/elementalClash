using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ResourceRespawnEventNode : MonoBehaviour
    {
        [SerializeField] private string eventId = "";
        [SerializeField] private GameObject resourceVisual;
        [SerializeField] private Collider resourceCollider;
        [SerializeField] private bool available = true;

        private void Awake()
        {
            Refresh();
        }

        private void Update()
        {
            if (!available && !string.IsNullOrWhiteSpace(eventId) && WorldEventTracker.IsActive(eventId))
            {
                Respawn();
            }
        }

        public void Harvest()
        {
            available = false;
            Refresh();
        }

        public void Respawn()
        {
            available = true;
            Refresh();
            NotificationFeed.Post("A resource node has respawned.", NotificationType.Map);
        }

        private void Refresh()
        {
            if (resourceVisual != null) resourceVisual.SetActive(available);
            if (resourceCollider != null) resourceCollider.enabled = available;
        }
    }
}
