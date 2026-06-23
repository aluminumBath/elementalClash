using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A spider's web. Roots enemies caught in it (a Control status, re-applied while it lasts) and is
    /// flammable: if any trapped enemy is on fire, the web ignites — bursting fire damage to everyone
    /// inside, then burning away. Enemies are found via the faction registry, so no colliders needed.
    /// Built in code by the spider companion (placeholder visual until art).
    /// </summary>
    public sealed class WebTrap : MonoBehaviour
    {
        private float _radius = 4f;
        private float _rootDuration = 1.3f;
        private float _igniteDamage = 35f;
        private float _lifetime = 6f;

        private float _age;
        private float _rootTimer;
        private bool _ignited;

        public void Configure(float radius, float rootDuration, float igniteDamage, float lifetime)
        {
            _radius = radius;
            _rootDuration = rootDuration;
            _igniteDamage = igniteDamage;
            _lifetime = lifetime;
        }

        private void Start()
        {
            BuildVisual();
            Root();
        }

        private void Update()
        {
            _age += Time.deltaTime;

            _rootTimer -= Time.deltaTime;
            if (_rootTimer <= 0f) { Root(); _rootTimer = 1f; }

            if (!_ignited && AnyTrappedBurning()) Ignite();
            else if (_age >= _lifetime) Destroy(gameObject);
        }

        private void Root()
        {
            float r2 = _radius * _radius;
            Vector3 p = transform.position;
            var all = FactionMember.All;
            for (int i = 0; i < all.Count; i++)
            {
                var m = all[i];
                if (m == null || !m.isActiveAndEnabled) continue;
                if (m.Faction != Faction.Wild && m.Faction != Faction.Bandit) continue;
                if ((m.transform.position - p).sqrMagnitude > r2) continue;

                var d = m.GetComponent<Damageable>();
                d?.ApplyStatus(new StatusEffect(StatusKind.Control, 1f, _rootDuration));
            }
        }

        private bool AnyTrappedBurning()
        {
            float r2 = _radius * _radius;
            Vector3 p = transform.position;
            var all = FactionMember.All;
            for (int i = 0; i < all.Count; i++)
            {
                var m = all[i];
                if (m == null || !m.isActiveAndEnabled) continue;
                if (m.Faction != Faction.Wild && m.Faction != Faction.Bandit) continue;
                if ((m.transform.position - p).sqrMagnitude > r2) continue;

                var d = m.GetComponent<Damageable>();
                if (d != null && d.Status != null && d.Status.BurnDamagePerSecond > 0f) return true;
            }
            return false;
        }

        private void Ignite()
        {
            _ignited = true;
            float r2 = _radius * _radius;
            Vector3 p = transform.position;
            var all = FactionMember.All;
            for (int i = 0; i < all.Count; i++)
            {
                var m = all[i];
                if (m == null || !m.isActiveAndEnabled) continue;
                if (m.Faction != Faction.Wild && m.Faction != Faction.Bandit) continue;
                if ((m.transform.position - p).sqrMagnitude > r2) continue;

                var d = m.GetComponent<Damageable>();
                if (d == null) continue;
                d.Apply(new DamageInfo(_igniteDamage, Element.Fire, AbilityVariant.Magmacraft));
                d.ApplyStatus(new StatusEffect(StatusKind.Burn, 6f, 2f));
            }
            Destroy(gameObject);
        }

        private void BuildVisual()
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.transform.SetParent(transform, false);
            disc.transform.localScale = new Vector3(_radius * 2f, 0.04f, _radius * 2f);
            var col = disc.GetComponent<Collider>();
            if (col != null) Destroy(col);
            disc.GetComponent<MeshRenderer>().sharedMaterial = ToonPalette.Tinted(new Color(0.92f, 0.95f, 0.98f, 1f));
        }
    }
}
