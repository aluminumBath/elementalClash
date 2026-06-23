using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The player's joined faction, sourced from the moddable <see cref="FactionRegistry"/> so built-in and
    /// modded factions are joinable the same way. Joining applies the faction's passive perk: the offense
    /// multiplier is read by <see cref="PlayerCombatController"/> when scaling a cast, and the defense multiplier
    /// becomes a standing damage modifier on the player's <see cref="Damageable"/> (a strength cuts incoming
    /// damage, a weakness amplifies it). Put on the player rig.
    /// </summary>
    public sealed class FactionMembership : MonoBehaviour
    {
        public static FactionMembership Instance { get; private set; }

        [Tooltip("Pick a built-in faction here...")]
        [SerializeField] private FactionId startingFaction = FactionId.None;
        [Tooltip("...or a modded faction's id, which wins if set.")]
        [SerializeField] private string startingFactionId = "";

        private FactionDef _current;
        private Damageable _body;

        /// <summary>The joined faction's data, or null if unaligned.</summary>
        public FactionDef Current => _current;
        public string CurrentId => _current?.Id;
        public float OffenseMultiplier => _current?.Perk.OffenseMultiplier ?? 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _body = GetComponentInParent<Damageable>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(startingFactionId)) JoinById(startingFactionId);
            else if (startingFaction != FactionId.None) Join(startingFaction);
            else ApplyPerk();
        }

        public void Join(FactionId id) => JoinById(id == FactionId.None ? null : id.ToString());

        public void JoinById(string id)
        {
            _current = string.IsNullOrEmpty(id) ? null : FactionRegistry.Get(id);
            ApplyPerk();
        }

        public void Leave() { _current = null; ApplyPerk(); }

        private void ApplyPerk()
        {
            float defense = _current?.Perk.DefenseMultiplier ?? 1f;
            // Defense >1 reduces incoming damage; <1 amplifies it.
            _body?.SetDamageReduction(1f - 1f / Mathf.Max(0.01f, defense));
        }
    }
}
