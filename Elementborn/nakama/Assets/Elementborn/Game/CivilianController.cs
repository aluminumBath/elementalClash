using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A peaceful townsperson. Wanders near its spawn and flees from nearby Wild/Bandit fighters; never
    /// attacks. The faction rules already stop enemies from targeting civilians unless the civilian is
    /// hit, so this only handles wandering and running away. Needs a CharacterController + FactionMember
    /// (set to Civilian — the spawner does this).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(FactionMember))]
    public sealed class CivilianController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 1.6f;
        [SerializeField] private float fleeSpeed = 4.5f;
        [SerializeField] private float wanderRadius = 6f;
        [SerializeField] private float fleeRadius = 9f;
        [SerializeField] private float repathInterval = 3f;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController _cc;
        private Vector3 _home;
        private Vector3 _wanderTarget;
        private float _repathTimer;
        private float _verticalVelocity;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _home = transform.position;
            _wanderTarget = _home;
        }

        private void Update()
        {
            Vector3 horizontal = Vector3.zero;
            Transform threat = NearestThreat();

            if (threat != null)
            {
                Vector3 away = transform.position - threat.position;
                away.y = 0f;
                if (away.sqrMagnitude > 0.001f)
                {
                    Vector3 dir = away.normalized;
                    horizontal = dir * fleeSpeed;
                    transform.forward = dir;
                }
            }
            else
            {
                _repathTimer -= Time.deltaTime;
                Vector3 toTarget = _wanderTarget - transform.position;
                toTarget.y = 0f;

                if (_repathTimer <= 0f || toTarget.magnitude < 0.5f)
                {
                    Vector2 r = Random.insideUnitCircle * wanderRadius;
                    _wanderTarget = _home + new Vector3(r.x, 0f, r.y);
                    _repathTimer = repathInterval;
                }
                else
                {
                    Vector3 dir = toTarget.normalized;
                    horizontal = dir * walkSpeed;
                    transform.forward = dir;
                }
            }

            if (_cc.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -1f;
            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = horizontal;
            motion.y = _verticalVelocity;
            _cc.Move(motion * Time.deltaTime);
        }

        private Transform NearestThreat()
        {
            Transform best = null;
            float bestSqr = fleeRadius * fleeRadius;
            var all = FactionMember.All;
            Vector3 p = transform.position;

            for (int i = 0; i < all.Count; i++)
            {
                var m = all[i];
                if (m == null || !m.isActiveAndEnabled) continue;
                if (m.Faction != Faction.Wild && m.Faction != Faction.Bandit) continue;

                float d = (m.transform.position - p).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = m.transform; }
            }
            return best;
        }
    }
}
