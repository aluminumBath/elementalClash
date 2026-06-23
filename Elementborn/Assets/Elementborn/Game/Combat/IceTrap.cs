using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// A frozen patch of water — the Water user's freeze. Inside it, actors are slowed (and non-water users
    /// can't breathe: the ice suffocates them — see <see cref="UnderwaterController"/>). It melts on a timer,
    /// and Fire users thaw it faster (<see cref="ThawNear"/>, called when a fire cast lands). Registered
    /// statically for membership/thaw tests.
    /// </summary>
    public sealed class IceTrap : MonoBehaviour
    {
        private static readonly List<IceTrap> All = new List<IceTrap>();

        [SerializeField] private float radius = 3f;
        [SerializeField] private float lifeSeconds = 8f;
        [Tooltip("Speed reduction (0-1) applied to anyone caught in the ice.")]
        [SerializeField] private float slowMagnitude = 0.8f;

        private const float ApplyInterval = 0.4f;
        private float _applyTimer;
        private static readonly Collider[] _hits = new Collider[16];
        private readonly HashSet<Damageable> _seen = new HashSet<Damageable>();

        public float Radius => radius;
        public float Remaining => lifeSeconds;

        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() => All.Remove(this);

        private void Update()
        {
            lifeSeconds -= Time.deltaTime;
            if (lifeSeconds <= 0f) { Destroy(gameObject); return; }

            _applyTimer -= Time.deltaTime;
            if (_applyTimer <= 0f) { TrapOccupants(); _applyTimer = ApplyInterval; }
        }

        private void TrapOccupants()
        {
            int n = Physics.OverlapSphereNonAlloc(transform.position, radius, _hits);
            _seen.Clear();
            for (int i = 0; i < n; i++)
            {
                var d = _hits[i].GetComponentInParent<Damageable>();
                if (d == null || !_seen.Add(d)) continue;
                var uw = d.GetComponent<UnderwaterController>();
                if (uw != null && uw.IsWaterUser) continue; // a water user isn't trapped by their own element
                d.ApplyStatus(new StatusEffect(StatusKind.Slow, slowMagnitude, ApplyInterval + 0.2f));
                d.ApplyStatus(new StatusEffect(StatusKind.Control, 1f, ApplyInterval + 0.1f)); // frozen in place
            }
        }

        public bool Has(Vector3 p) => (p - transform.position).sqrMagnitude <= radius * radius;
        public void Thaw(float seconds) => lifeSeconds -= Mathf.Max(0f, seconds);

        public static bool Contains(Vector3 p)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i] != null && All[i].Has(p)) return true;
            return false;
        }

        /// <summary>Fire's underwater job: melt nearby ice faster.</summary>
        public static void ThawNear(Vector3 p, float radius, float amount)
        {
            float r2 = radius * radius;
            for (int i = 0; i < All.Count; i++)
                if (All[i] != null && (All[i].transform.position - p).sqrMagnitude <= r2)
                    All[i].Thaw(amount);
        }

        public static IceTrap Freeze(Vector3 position, float radius, float lifeSeconds)
        {
            var go = new GameObject("IceTrap") { transform = { position = position } };
            var t = go.AddComponent<IceTrap>();
            t.radius = radius;
            t.lifeSeconds = lifeSeconds;
            return t;
        }
    }
}
