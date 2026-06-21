using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Marks a creature as tameable. Reads its <see cref="Damageable"/> to know how weakened it is and
    /// bridges a tame attempt to <see cref="PlayerInventory"/>. The actual interaction (a button when
    /// you're close, or a UI prompt) calls <see cref="TryTame"/>; the creature's own behaviour/visuals
    /// come with the bestiary. Lives on the same object as the creature's Damageable.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public sealed class Tameable : MonoBehaviour
    {
        [SerializeField] private CreatureKind kind = CreatureKind.Horse;

        private Damageable _self;
        private readonly UnityRandomSource _rng = new UnityRandomSource();

        public CreatureKind Kind => kind;
        public void SetKind(CreatureKind k) => kind = k;

        private void Awake() => _self = GetComponent<Damageable>();

        public float HealthFraction
        {
            get
            {
                var h = _self != null ? _self.Health : null;
                return (h != null && h.Max > 0f) ? h.Current / h.Max : 1f;
            }
        }

        /// <summary>True when the player could attempt a tame right now (weakened + has the lure).</summary>
        public bool CanTame(out string reason)
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) { reason = "No inventory"; return false; }
            if (inv.Owns(kind)) { reason = "Already owned"; return false; }
            if (!inv.CanUse(CreatureCatalog.For(kind))) { reason = "Wrong element"; return false; }
            return TamingRules.CanAttempt(CreatureCatalog.For(kind), HealthFraction, inv.Lures(kind) > 0, out reason);
        }

        /// <summary>Attempt to tame; on success the creature is added to the player's owned list.</summary>
        public TameOutcome TryTame()
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) return TameOutcome.Fail("No inventory");

            var outcome = inv.TryTame(kind, HealthFraction, _rng);
            if (outcome.Success) Destroy(gameObject); // it joins you; despawn the wild instance
            return outcome;
        }
    }
}
