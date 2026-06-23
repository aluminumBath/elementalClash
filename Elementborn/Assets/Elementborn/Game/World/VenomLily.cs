using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A poisonous look-alike of the <see cref="GleamLily"/> — nearly identical, but purple-sparkled instead of
    /// gold-and-pink, and harmful to the touch. There's no fruit to harvest (trying to tend it does nothing,
    /// since it isn't a Gleamlily); brushing it poisons the toucher with contact damage and a sickly slow, so a
    /// careless healer can be fooled. Needs a trigger collider; the bloom mesh + purple sparkle are placeholders.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class VenomLily : MonoBehaviour
    {
        [SerializeField] private float contactDamage = PlantTuning.VenomContactDamage;
        [SerializeField] private float tickInterval = PlantTuning.VenomTickInterval;
        [SerializeField] private float slow = PlantTuning.VenomSlow;
        [SerializeField] private float slowDuration = PlantTuning.VenomSlowDuration;

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

        private void OnTriggerStay(Collider other)
        {
            if (_cd > 0f) return;
            bool isPersonOrCreature = other.GetComponentInParent<PlayerCombatController>() != null
                                      || other.GetComponentInParent<CreatureController>() != null;
            if (!isPersonOrCreature) return;

            var d = other.GetComponentInParent<IDamageable>();
            if (d == null) return;
            d.Apply(new DamageInfo(contactDamage, Element.Earth));               // poison bite
            d.ApplyStatus(new StatusEffect(StatusKind.Slow, slow, slowDuration)); // sickly slow
            _cd = tickInterval;
        }
    }
}
