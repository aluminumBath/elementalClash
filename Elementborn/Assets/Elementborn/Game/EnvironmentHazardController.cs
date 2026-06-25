using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Applies the pure <see cref="EnvironmentHazards"/> model to the player on a tick: climb too high
    /// and the cold drains health; dive too deep and the pressure does. Exemptions come straight from the
    /// systems already in place — the channeler element (<see cref="PlayerCombatController.Loadout"/>) and the
    /// chest/helmet enchants (<see cref="EquipmentController"/>). Dormant on the flat surface world; it only
    /// bites once the vertical heights and the deep water exist. Spawned by the bootstrap scene.</summary>
    public sealed class EnvironmentHazardController : MonoBehaviour
    {
        [Tooltip("Seconds between hazard damage applications.")]
        [SerializeField] private float tickInterval = 1f;

        private PlayerCombatController _player;
        private Damageable _body;
        private float _tick;
        private bool _coldWarned;
        private bool _pressureWarned;

        private void Update()
        {
            if (_player == null)
            {
                _player = FindObjectOfType<PlayerCombatController>();
                if (_player == null) return;
                _body = _player.GetComponentInParent<Damageable>();
            }
            if (_body == null || _body.Health == null || _body.Health.IsDead) return;

            _tick += Time.deltaTime;
            if (_tick < tickInterval) return;
            float dt = _tick;
            _tick = 0f;

            float y = _player.transform.position.y;
            float altitude = y;                 // above sea level (water baseline)
            // Depth comes from whichever body of water actually contains the player: the ocean at sea
            // level, or a flooded interior overhead. Dry ground reports 0 — no pressure, as before.
            float depth = WaterBody.SubmersionDepth(_player.transform.position);

            var loadout = _player.Loadout;
            bool channeler = loadout != null && loadout.IsChanneler;
            Element primary = channeler ? loadout.Elements[0] : Element.Fire; // only read when channeler

            var gear = EquipmentController.Instance != null ? EquipmentController.Instance.Loadout : null;
            Element? chest = gear != null ? gear.EnchantIn(EquipSlot.Chest) : (Element?)null;
            Element? helmet = gear != null ? gear.EnchantIn(EquipSlot.Helmet) : (Element?)null;

            float cold = EnvironmentHazards.ImmuneToCold(channeler, primary, chest)
                ? 0f : EnvironmentHazards.ColdDamagePerSecond(altitude);
            float pressure = EnvironmentHazards.ImmuneToPressure(channeler, primary, chest, helmet)
                ? 0f : EnvironmentHazards.PressureDamagePerSecond(depth);

            if (cold > 0f)
            {
                _body.Health.Apply(new DamageInfo(cold * dt, Element.Air));
                if (!_coldWarned) { _coldWarned = true; GameHud.Instance?.Toast("The freezing heights sap your strength — descend, or attune to Air/Fire."); }
            }
            else _coldWarned = false;

            if (pressure > 0f)
            {
                _body.Health.Apply(new DamageInfo(pressure * dt, Element.Water));
                if (!_pressureWarned) { _pressureWarned = true; GameHud.Instance?.Toast("The crushing depths press in — surface, or attune to Water/Earth."); }
            }
            else _pressureWarned = false;
        }
    }
}
