using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The Gleamlily's fruit — silver-skinned, blood-red within. Touched by a person or creature, it heals a
    /// chunk and cures their active statuses, then is spent. <see cref="ApplyTo"/> exposes the same effect for
    /// deliberate use on a chosen target (e.g. feeding a creature) once inventory hooks exist. Needs a trigger
    /// collider.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Heartfruit : MonoBehaviour
    {
        [SerializeField] private float heal = PlantTuning.HeartfruitHeal;

        private void Awake()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            bool isPersonOrCreature = other.GetComponentInParent<PlayerCombatController>() != null
                                      || other.GetComponentInParent<CreatureController>() != null;
            if (!isPersonOrCreature) return;

            var target = other.GetComponentInParent<Damageable>();
            if (target == null) return;
            ApplyTo(target);
            Destroy(gameObject);
        }

        /// <summary>Heal and cure a target.</summary>
        public void ApplyTo(Damageable target)
        {
            if (target == null) return;
            target.Health?.Heal(heal);
            target.Status?.Clear();
            AudioController.Instance?.PlayImpact(Element.Water, target.transform.position);
            // feeding a creature is an act of care (Kiana's law notices)
            if (target.GetComponentInParent<CreatureController>() != null)
                FindObjectOfType<CreatureCareController>()?.CareForCreature();
        }
    }
}
