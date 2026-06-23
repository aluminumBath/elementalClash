using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The evolution game mode on the player: you begin with one element and, at a milestone, take a second —
    /// the pair grants a specialty (plant, blood, steam &amp; healing, metal, lava, or flight). Each change
    /// rebuilds the loadout (via <see cref="ElementalEvolution.ToLoadout"/>) and applies it to the player, so the
    /// new element and specialty are immediately live in combat. Put on the player rig for an evolution-mode run.
    /// </summary>
    public sealed class EvolutionController : MonoBehaviour
    {
        [SerializeField] private Element startingElement = Element.Fire;

        private ElementalEvolution _evolution;
        private PlayerCombatController _combat;

        public ElementalEvolution Evolution => _evolution;
        public SubArt Specialty => _evolution?.Specialty ?? SubArt.None;
        public string SpecialtyName => Specialties.NameOf(Specialty);

        private void Awake()
        {
            _combat = GetComponentInParent<PlayerCombatController>();
            if (_combat == null) _combat = FindObjectOfType<PlayerCombatController>();
        }

        private void Start() => Begin(startingElement);

        /// <summary>Begin a run with a single starting element.</summary>
        public void Begin(Element primary)
        {
            _evolution = new ElementalEvolution(primary);
            Apply();
        }

        /// <summary>Take a second element, unlocking its specialty. Returns false if already evolved or invalid.</summary>
        public bool Evolve(Element second)
        {
            if (_evolution == null) return false;
            bool ok = _evolution.Evolve(second);
            if (ok) Apply();
            return ok;
        }

        private void Apply()
        {
            if (_evolution == null) return;
            var loadout = _evolution.ToLoadout();
            if (PlayerInventory.Instance != null) PlayerInventory.Instance.Loadout = loadout;
            _combat?.SetLoadout(loadout);
        }
    }
}
