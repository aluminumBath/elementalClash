using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Bridges input -> logic -> presentation. Resolves each intent through the element
    /// <see cref="AbilitySystem"/> if the player channels, or through <see cref="Weapons"/> using the
    /// equipped <see cref="WeaponHolder"/> if they don't, then raises the outcome for visuals.
    /// </summary>
    public sealed class PlayerCombatController : MonoBehaviour
    {
        [Tooltip("Must implement IPlayerInputProvider (VrInputProvider or FlatInputProvider).")]
        [SerializeField] private MonoBehaviour inputProviderBehaviour;
        [SerializeField] private Transform castOrigin;
        [Tooltip("Used when the player has no element. Leave null for channelers.")]
        [SerializeField] private WeaponHolder weaponHolder;

        public System.Action<AbilityOutcome, Vector3> OutcomeReady;

        private IPlayerInputProvider _input;
        private ChannelerLoadout _loadout;
        private AbilitySystem _abilities;
        private UnderwaterController _underwater;
        private BubbleVolume _bubble;
        private Damageable _body;
        private HiddenMoveController _hidden;

        /// <summary>The player's current loadout (so plant entities, etc. can check sub-arts like Verdancy).</summary>
        public ChannelerLoadout Loadout => _loadout;

        private void Awake() => SetLoadout(ChannelerLoadout.SingleElement(Element.Fire));

        private void OnEnable()
        {
            _input = inputProviderBehaviour as IPlayerInputProvider;
            if (_input == null)
            {
                Debug.LogError("PlayerCombatController: assigned provider does not implement IPlayerInputProvider.");
                return;
            }
            _underwater = GetComponent<UnderwaterController>();
            _body = GetComponentInParent<Damageable>();
            _hidden = GetComponent<HiddenMoveController>();
            _input.IntentProduced += HandleIntent;
        }

        private void OnDisable()
        {
            if (_input != null) _input.IntentProduced -= HandleIntent;
        }

        private void HandleIntent(ChannelingIntent intent)
        {
            // Grabbed (octopus), frozen (ice trap), or stunned (lightning): can't act.
            if (_body != null && (_body.IsStunned || _body.IsControlled)) return;

            // Ability ladder: some intents unlock with player level. No progression in the scene -> no gating
            // (standalone scenes like the Arena keep the full kit).
            var progression = ProgressionController.Instance;
            if (progression != null && !AbilityUnlocks.IsUnlocked(intent.Type, progression.Progression.Level))
            {
                GameHud.Instance?.Toast(AbilityUnlocks.DisplayName(intent.Type)
                    + " unlocks at Lv " + AbilityUnlocks.RequiredLevel(intent.Type));
                return;
            }

            // Hidden signature move (special gesture) — applied directly, outside the normal kit.
            if (intent.Type == IntentType.Signature)
            {
                if (_hidden != null && _loadout.IsChanneler)
                {
                    _hidden.Perform(_loadout.Elements[0], intent.Direction,
                        castOrigin ? castOrigin.position : transform.position);
                    QuestEvents.RaiseAbilityCast(_loadout.Elements[0].ToString(), IntentType.Signature.ToString());
                }
                return;
            }

            AbilityOutcome outcome;
            if (_loadout.IsChanneler)
                outcome = _abilities.Resolve(intent);
            else if (weaponHolder != null && weaponHolder.HasWeapon)
                outcome = Weapons.Resolve(weaponHolder.Current, intent);
            else
                outcome = AbilityOutcome.Empty; // unarmed: grab a weapon from the map

            if (outcome.IsEmpty) return;

            // Underwater: each element behaves differently below the surface.
            if (_underwater != null && _underwater.IsSubmerged)
            {
                Vector3 aim = transform.position + outcome.Direction;
                if (outcome.Element == Element.Fire)
                {
                    IceTrap.ThawNear(aim, UnderwaterTuning.FireThawRadius, UnderwaterTuning.FireThawAmount);
                }
                else if (outcome.Element == Element.Air && intent.Type == IntentType.PrimaryCast)
                {
                    if (_bubble == null) // air deploys a breathing bubble that follows you
                        _bubble = BubbleVolume.Deploy(transform.position, UnderwaterTuning.BubbleRadius,
                            UnderwaterTuning.BubbleLife, follow: transform);
                }
                else if (outcome.Element == Element.Water && outcome.Variant == AbilityVariant.Ice
                         && outcome.Kind == OutcomeKind.Projectile)
                {
                    Vector3 freezeAt = transform.position + outcome.Direction * UnderwaterTuning.IceTrapRange;
                    IceTrap.Freeze(freezeAt, UnderwaterTuning.IceTrapRadius, UnderwaterTuning.IceTrapLife);
                    BubbleVolume.FreezeNear(freezeAt, UnderwaterTuning.IceTrapRadius); // freeze an air bubble + its contents
                }

                if (!UnderwaterAbilityRules.AllowsCast(true, outcome.Element, outcome.Variant, outcome.Kind))
                    return; // out of its element (air primary just made the bubble; fire/earth fizzle)
            }

            // Stamina pacing: only when a StaminaController is present (the arena). Too winded = the cast fizzles.
            var stamina = StaminaController.Instance;
            if (stamina != null && !stamina.TrySpend(intent.Type)) return;

            if (_loadout.IsChanneler && WeatherController.Instance != null)
                outcome = outcome.Scaled(WeatherController.Instance.ElementMultiplier(outcome.Element));

            if (FactionMembership.Instance != null)
                outcome = outcome.Scaled(FactionMembership.Instance.OffenseMultiplier);

            Vector3 origin = castOrigin ? castOrigin.position : transform.position;
            // VFX binder, melee, dash, and Sanguine Grip controllers all listen to this.
            OutcomeReady?.Invoke(outcome, origin);

            if (_loadout.IsChanneler) // grimoire: reveal the Attacks entry for this element × intent
                QuestEvents.RaiseAbilityCast(outcome.Element.ToString(), intent.Type.ToString());
        }

        public void SetLoadout(ChannelerLoadout loadout)
        {
            _loadout = loadout;
            _abilities = new AbilitySystem(loadout);
        }
    }
}
