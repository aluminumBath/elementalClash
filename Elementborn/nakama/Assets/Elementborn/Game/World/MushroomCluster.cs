using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A cluster of haze-spore mushrooms. A plant user can <see cref="Puff"/> them (via
    /// <see cref="PlantControlController"/>) to disorient foes in a radius — a brief loss of control — while the
    /// plant user stays immune. A non-plant-user who brushes the cap sets it off on themselves. Needs a trigger
    /// collider for that brush. Disorient reuses the Control status (interrupt + pin) for now.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MushroomCluster : MonoBehaviour
    {
        [SerializeField] private float radius = PlantTuning.SporeRadius;
        [SerializeField] private float disorientSeconds = PlantTuning.SporeDisorient;
        [SerializeField] private float recharge = 4f;

        private float _cd;

        private void Awake()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void Update()
        {
            if (_cd > 0f) _cd -= Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_cd > 0f) return;
            var pc = other.GetComponentInParent<PlayerCombatController>();
            if (pc != null && !PlantControl.ResistsSpores(pc.Loadout)) Puff();
        }

        /// <summary>Release the spores: disorient everyone in radius except plant users.</summary>
        public void Puff()
        {
            _cd = recharge;
            var hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var h in hits)
            {
                var pc = h.GetComponentInParent<PlayerCombatController>();
                if (pc != null && PlantControl.ResistsSpores(pc.Loadout)) continue; // the plant user is unaffected
                var d = h.GetComponentInParent<IDamageable>();
                d?.ApplyStatus(new StatusEffect(StatusKind.Control, 1f, disorientSeconds));
            }
            AudioController.Instance?.PlayImpact(Element.Earth, transform.position);
        }
    }
}
