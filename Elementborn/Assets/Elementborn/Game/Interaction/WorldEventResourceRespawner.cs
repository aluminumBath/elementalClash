using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Respawns linked harvest nodes while a matching world event is active.
    /// </summary>
    public sealed class WorldEventResourceRespawner : MonoBehaviour
    {
        [SerializeField] private string worldEventId = "coral_resource_bloom";
        [SerializeField] private HarvestableResourceNode[] nodes;
        [SerializeField] private bool respawnOncePerActivation = true;

        private bool usedDuringCurrentActivation;

        private void Update()
        {
            bool active = WorldEventTracker.IsActive(worldEventId);
            if (!active)
            {
                usedDuringCurrentActivation = false;
                return;
            }

            if (respawnOncePerActivation && usedDuringCurrentActivation)
            {
                return;
            }

            RespawnAll();
            usedDuringCurrentActivation = true;
        }

        [ContextMenu("Respawn All")]
        public void RespawnAll()
        {
            if (nodes == null)
            {
                return;
            }

            foreach (var node in nodes)
            {
                if (node != null)
                {
                    node.Respawn();
                }
            }
        }
    }
}
