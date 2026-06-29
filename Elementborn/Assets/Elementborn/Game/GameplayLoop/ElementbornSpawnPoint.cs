using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnId = "";
        [SerializeField] private ElementbornSpawnRole role = ElementbornSpawnRole.Unknown;
        [SerializeField] private Color gizmoColor = new Color(1f, 0.45f, 0.1f, 0.75f);
        [SerializeField] private float radius = 1.5f;

        public string SpawnId => string.IsNullOrWhiteSpace(spawnId) ? gameObject.name : spawnId;
        public ElementbornSpawnRole Role => role;

        public void Configure(string id, ElementbornSpawnRole spawnRole)
        {
            spawnId = id;
            role = spawnRole;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * radius * 1.5f);
        }
    }
}
