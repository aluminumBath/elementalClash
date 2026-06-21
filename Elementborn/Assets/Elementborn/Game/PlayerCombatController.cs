using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Bridges input -> logic -> presentation. Resolves each intent through the element
    /// <see cref="AbilitySystem"/> if the player bends, or through <see cref="Weapons"/> using the
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

        private void Awake() => SetLoadout(ChannelerLoadout.SingleElement(Element.Fire));

        private void OnEnable()
        {
            _input = inputProviderBehaviour as IPlayerInputProvider;
            if (_input == null)
            {
                Debug.LogError("PlayerCombatController: assigned provider does not implement IPlayerInputProvider.");
                return;
            }
            _input.IntentProduced += HandleIntent;
        }

        private void OnDisable()
        {
            if (_input != null) _input.IntentProduced -= HandleIntent;
        }

        private void HandleIntent(ChannelingIntent intent)
        {
            AbilityOutcome outcome;
            if (_loadout.IsChanneler)
                outcome = _abilities.Resolve(intent);
            else if (weaponHolder != null && weaponHolder.HasWeapon)
                outcome = Weapons.Resolve(weaponHolder.Current, intent);
            else
                outcome = AbilityOutcome.Empty; // unarmed: grab a weapon from the map

            if (outcome.IsEmpty) return;

            if (_loadout.IsChanneler && WeatherController.Instance != null)
                outcome = outcome.Scaled(WeatherController.Instance.ElementMultiplier(outcome.Element));

            Vector3 origin = castOrigin ? castOrigin.position : transform.position;
            // VFX binder, melee, dash, and Sanguine Grip controllers all listen to this.
            OutcomeReady?.Invoke(outcome, origin);
        }

        public void SetLoadout(ChannelerLoadout loadout)
        {
            _loadout = loadout;
            _abilities = new AbilitySystem(loadout);
        }
    }
}
