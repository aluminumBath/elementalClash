using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EnemyPatrolRoute : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointReachDistance = 1f;
        [SerializeField] private bool loop = true;

        private int currentIndex;

        public bool HasRoute => waypoints != null && waypoints.Length > 0;

        public Vector3 CurrentWaypoint
        {
            get
            {
                if (!HasRoute || waypoints[currentIndex] == null)
                {
                    return transform.position;
                }

                return waypoints[currentIndex].position;
            }
        }

        public void AdvanceIfReached(Vector3 position)
        {
            if (!HasRoute)
            {
                return;
            }

            if (Vector3.Distance(position, CurrentWaypoint) > waypointReachDistance)
            {
                return;
            }

            currentIndex++;
            if (currentIndex >= waypoints.Length)
            {
                currentIndex = loop ? 0 : waypoints.Length - 1;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                {
                    continue;
                }

                Gizmos.DrawWireSphere(waypoints[i].position, waypointReachDistance);
                if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}
