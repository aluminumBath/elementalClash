using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Poise/stagger for an enemy: real hits build a pure <see cref="Poise"/> meter; when it breaks, the enemy is
    /// stunned for a moment (a Stun <see cref="StatusEffect"/>, which <c>EnemyController</c> already freezes on) and
    /// flagged <see cref="IsStaggered"/> — a finisher opening that <see cref="FinisherController"/> reads via the
    /// static registry. Auto-required on every <see cref="Combat.EnemyController"/>.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    [DisallowMultipleComponent]
    public sealed class StaggerController : MonoBehaviour
    {
        private const float PoiseMax = 60f;
        private const float RegenPerSecond = 18f;
        private const float RegenDelay = 2.5f;
        private const float StaggerSeconds = 2.5f;

        private static readonly List<StaggerController> Registry = new List<StaggerController>(); // currently staggered
        private static readonly List<StaggerController> All = new List<StaggerController>();      // every live enemy

        private Damageable _dmg;
        private Poise _poise;
        private float _staggerTimer;

        public bool IsStaggered => _staggerTimer > 0f;
        public float PoiseFraction => _poise != null ? _poise.Fraction : 0f;
        public Damageable Target => _dmg;

        /// <summary>Force this enemy into a stagger now (e.g. a successful parry counter). Refreshes the window.</summary>
        public void ForceStagger() => Break();

        private void Awake()
        {
            _dmg = GetComponent<Damageable>();
            _poise = new Poise(PoiseMax, RegenPerSecond, RegenDelay);
        }

        private void OnEnable()
        {
            if (_dmg != null && _dmg.Health != null) _dmg.Health.Damaged += OnDamaged;
            if (!All.Contains(this)) All.Add(this);
        }

        private void OnDisable()
        {
            if (_dmg != null && _dmg.Health != null) _dmg.Health.Damaged -= OnDamaged;
            Registry.Remove(this);
            All.Remove(this);
            _staggerTimer = 0f;
        }

        private void OnDestroy() { Registry.Remove(this); All.Remove(this); }

        private void OnDamaged(DamageInfo info)
        {
            if (info.Amount < 1f) return; // ignore chip/DoT ticks
            if (IsStaggered) return;       // already broken; let the window play out
            if (_poise.AddHit(info.Amount)) Break();
        }

        private void Break()
        {
            _staggerTimer = StaggerSeconds;
            _dmg.ApplyStatus(new StatusEffect(StatusKind.Stun, 0f, StaggerSeconds));
            if (!Registry.Contains(this)) Registry.Add(this);

            // Readable break cue: a bright flash at the chest + a "STAGGER!" popup.
            Feel.TransientLight.Flash(transform.position + Vector3.up * 1.2f, new Color(1f, 0.95f, 0.55f), 8f, 6f, 0.25f);
            Feel.FloatingText.Spawn(transform.position, "STAGGER!", new Color(1f, 0.85f, 0.3f), 28f, 0.11f);
        }

        private void Update()
        {
            if (_staggerTimer > 0f)
            {
                _staggerTimer -= Time.deltaTime;
                if (_staggerTimer <= 0f) Registry.Remove(this);
            }
            else
            {
                _poise.Tick(Time.deltaTime);
            }
        }

        /// <summary>Nearest currently-staggered enemy within range of a point (for finisher targeting).</summary>
        public static StaggerController NearestStaggered(Vector3 point, float maxRange)
        {
            StaggerController best = null;
            float bestSq = maxRange * maxRange;
            for (int i = Registry.Count - 1; i >= 0; i--)
            {
                var s = Registry[i];
                if (s == null) { Registry.RemoveAt(i); continue; }
                float d = (s.transform.position - point).sqrMagnitude;
                if (d <= bestSq) { bestSq = d; best = s; }
            }
            return best;
        }

        /// <summary>Nearest live enemy within a frontal cone of an origin/forward (for parry counter-targeting).</summary>
        public static StaggerController NearestInFront(Vector3 origin, Vector3 forward, float maxRange, float minDot)
        {
            Vector3 fwd = forward; fwd.y = 0f;
            if (fwd.sqrMagnitude < 1e-4f) return null;
            fwd.Normalize();

            StaggerController best = null;
            float bestSq = maxRange * maxRange;
            for (int i = All.Count - 1; i >= 0; i--)
            {
                var s = All[i];
                if (s == null) { All.RemoveAt(i); continue; }
                Vector3 to = s.transform.position - origin;
                float d2 = to.sqrMagnitude;
                if (d2 > bestSq) continue;
                Vector3 flat = to; flat.y = 0f;
                if (flat.sqrMagnitude < 1e-4f) { bestSq = d2; best = s; continue; } // basically on top of us
                flat.Normalize();
                if (Vector3.Dot(fwd, flat) < minDot) continue; // outside the cone
                bestSq = d2; best = s;
            }
            return best;
        }

        /// <summary>Stagger every live enemy within radius of a point (mount impact skills).</summary>
        public static void StaggerNearby(Vector3 origin, float radius)
        {
            float r2 = radius * radius;
            for (int i = All.Count - 1; i >= 0; i--)
            {
                var s = All[i];
                if (s == null) { All.RemoveAt(i); continue; }
                if ((s.transform.position - origin).sqrMagnitude <= r2) s.ForceStagger();
            }
        }

        /// <summary>Collect the distinct affinities of live enemies within range of a point (attunement discovery).</summary>
        public static void CollectNearbyAffinities(Vector3 origin, float maxRange, List<Element> results)
        {
            if (results == null) return;
            float r2 = maxRange * maxRange;
            for (int i = All.Count - 1; i >= 0; i--)
            {
                var s = All[i];
                if (s == null) { All.RemoveAt(i); continue; }
                if ((s.transform.position - origin).sqrMagnitude > r2) continue;
                var aff = s.Target != null ? s.Target.Affinity : null;
                if (aff.HasValue && !results.Contains(aff.Value)) results.Add(aff.Value);
            }
        }
    }
}
