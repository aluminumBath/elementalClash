using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Parfa's two frogs argue air vs water forever. The trick to make them agree: show up embodying <em>both</em>
    /// — a player whose loadout holds Air and Water is common ground they can't squabble with. Pull it off and
    /// Parfa pays a <see cref="Currency.Diamond"/>, once. Put on the frogs object near Parfa. Offers its
    /// interaction through the <see cref="InteractionArbiter"/>.
    /// </summary>
    public sealed class ParfaFrogController : MonoBehaviour, IInteractable
    {
        [SerializeField] private float reach = 3.5f;

        private readonly FrogAccord _accord = new FrogAccord();
        private PlayerCombatController _combat;

        public bool Agreed => _accord.Agreed;

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _combat = p.GetComponentInParent<PlayerCombatController>();
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            if (_accord.Agreed) return false;
            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > reach) return false;
            interaction = new Interaction(d, 0, "Talk to the frogs", Attempt);
            return true;
        }

        private void Attempt()
        {
            var loadout = _combat != null ? _combat.Loadout : null;
            bool harmonizesBoth = loadout != null && loadout.HasElement(Element.Air) && loadout.HasElement(Element.Water);
            if (harmonizesBoth) Trick();
            else Debug.Log("[Frogs] *the two frogs go right on bickering about air and water*");
        }

        /// <summary>The trick worked — the frogs agree and Parfa awards a diamond (once).</summary>
        public void Trick()
        {
            if (!_accord.Reconcile()) return;
            PlayerInventory.Instance?.AddCurrency(Currency.Diamond, 1);
            _accord.MarkRewardGiven();
            Debug.Log("[Parfa] You got those two to agree?! Astonishing. Here — a diamond. You've earned it.");
        }
    }
}
