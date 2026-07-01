using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A wild phoenix's death: a fire blast that damages everything nearby (falloff via <see cref="Blast"/>), then the
    /// bird scatters to embers that drift up and fade — and after a cooldown it reforms at a fresh spot near its haunt
    /// at full health. It reuses its own object (no respawn factory) and takes over death so the body isn't destroyed.
    /// Auto-attached to Phoenix creatures by <see cref="CreatureController"/>.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public sealed class PhoenixDeathBurst : MonoBehaviour
    {
        [SerializeField] private float blastRadius = 5f;
        [SerializeField] private float blastDamage = 40f;
        [SerializeField] private float respawnDelay = 12f;
        [SerializeField] private float wanderRadius = 18f;

        private Damageable _self;
        private Vector3 _home;
        private bool _homeSet;
        private float _respawnAt = -1f;

        private void Awake()
        {
            _self = GetComponent<Damageable>();
            if (_self != null && _self.Health != null)
            {
                _self.DestroyOnDeath = false;          // we handle death (blast + ash + reform)
                _self.Health.Died -= OnDied;
                _self.Health.Died += OnDied;
            }
        }

        private void Start()
        {
            _home = transform.position;
            _homeSet = true;
        }

        private void OnDestroy()
        {
            if (_self != null && _self.Health != null) _self.Health.Died -= OnDied;
        }

        private void OnDied()
        {
            Vector3 at = transform.position;
            FireBlast(at);
            ScatterToAsh(at);
            SetBodyActive(false);
            _respawnAt = Time.time + respawnDelay;
        }

        private void Update()
        {
            if (_respawnAt >= 0f && Time.time >= _respawnAt)
            {
                _respawnAt = -1f;
                Reform();
            }
        }

        private void FireBlast(Vector3 center)
        {
            AudioController.Instance?.PlayImpact(Element.Fire, center);
            foreach (var col in Physics.OverlapSphere(center, blastRadius))
            {
                var target = col.GetComponentInParent<IDamageable>();
                if (target == null || ReferenceEquals(target, _self)) continue;   // don't burn its own remains
                float dist = Vector3.Distance(center, col.ClosestPoint(center));
                float dmg = Blast.DamageAt(blastDamage, dist, blastRadius);
                if (dmg > 0f) target.Apply(new DamageInfo(dmg, Element.Fire));
            }
        }

        private void ScatterToAsh(Vector3 center)
        {
            for (int i = 0; i < 10; i++)
            {
                var ember = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var col = ember.GetComponent<Collider>();
                if (col != null) Destroy(col);
                ember.transform.position = center + new Vector3(
                    Random.Range(-0.4f, 0.4f), Random.Range(0.2f, 1.2f), Random.Range(-0.4f, 0.4f));
                ember.transform.localScale = Vector3.one * Random.Range(0.05f, 0.14f);
                var mr = ember.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(ElementColor.For(Element.Fire));
                ember.AddComponent<AshMote>().Init(
                    new Vector3(Random.Range(-1f, 1f), Random.Range(1.5f, 3f), Random.Range(-1f, 1f)), 1.6f);
            }
        }

        private void SetBodyActive(bool on)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = on;
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = on;
            var creature = GetComponent<CreatureController>();
            if (creature != null) creature.enabled = on;   // Start() ran once, so re-enabling won't re-subscribe
        }

        private void Reform()
        {
            Vector3 origin = _homeSet ? _home : transform.position;
            Vector3 spot = origin + new Vector3(
                Random.Range(-wanderRadius, wanderRadius), 0f, Random.Range(-wanderRadius, wanderRadius));
            if (Physics.Raycast(spot + Vector3.up * 30f, Vector3.down, out var hit, 60f)) spot = hit.point;
            transform.position = spot;
            if (_self != null && _self.Health != null) _self.Health.Revive();
            SetBodyActive(true);
        }
    }

    /// <summary>A single drifting ember/ash mote: rises, slows, and shrinks away, then despawns.</summary>
    public sealed class AshMote : MonoBehaviour
    {
        private Vector3 _vel;
        private float _life = 1.6f;
        private float _t;

        public void Init(Vector3 velocity, float life) { _vel = velocity; _life = life; }

        private void Update()
        {
            _t += Time.deltaTime;
            transform.position += _vel * Time.deltaTime;
            _vel *= 0.96f;                              // air drag — drifts and settles
            float s = Mathf.Max(0f, 1f - _t / _life);
            transform.localScale = Vector3.one * (0.14f * s);
            if (_t >= _life) Destroy(gameObject);
        }
    }
}
