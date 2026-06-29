using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Trigger-based boarding helper. Add to a child trigger collider on the boat.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class BoatBoardingStation : MonoBehaviour
    {
        [SerializeField] private BoatController boat;
        [SerializeField] private Transform disembarkPoint;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float ejectionCooldown = 0.45f;

        private Transform _nearbyPlayer;
        private Transform _pilotedPlayer;
        private Component[] _disabledMovement;
        private Damageable _pilotDamageable;
        private float _cooldownUntil;

        private void Reset()
        {
            boat = GetComponentInParent<BoatController>();
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            if (Time.time < _cooldownUntil) return;
            if (!InputBindings.Interact.WasPressedThisFrame()) return;

            if (boat != null && boat.HasPilot) Disembark(false, Vector3.zero);
            else if (_nearbyPlayer != null) Board(_nearbyPlayer);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag)) _nearbyPlayer = other.transform;
        }

        private void OnTriggerExit(Collider other)
        {
            if (_nearbyPlayer == other.transform) _nearbyPlayer = null;
        }

        private void Board(Transform player)
        {
            if (boat == null || player == null) return;

            boat.Board(player);
            _pilotedPlayer = player;
            player.SetParent(boat.PilotSeat, false);
            player.localPosition = Vector3.zero;
            player.localRotation = Quaternion.identity;

            _disabledMovement = new Component[]
            {
                player.GetComponent<FirstPersonRig>(),
                player.GetComponent<ThirdPersonRig>(),
                player.GetComponent<CharacterController>()
            };

            foreach (var component in _disabledMovement)
                SetComponentEnabled(component, false);

            _pilotDamageable = player.GetComponentInParent<Damageable>();
            if (_pilotDamageable != null && _pilotDamageable.Health != null)
                _pilotDamageable.Health.Damaged += OnPilotDamaged;

            _cooldownUntil = Time.time + 0.25f;
            GameHud.Instance?.Toast("Boarded boat");
        }

        private void OnPilotDamaged(DamageInfo info)
        {
            if (info.Amount < 1f) return;
            Vector3 impulse = boat != null ? boat.transform.right * 6f + Vector3.up * 2f : transform.right * 6f;
            ForceEject(impulse);
        }

        public void ForceEject(Vector3 impulse)
        {
            Disembark(true, impulse);
        }

        private void Disembark(bool launched, Vector3 impulse)
        {
            if (boat == null || boat.Pilot == null) return;

            Transform player = boat.Pilot;
            if (_pilotDamageable != null && _pilotDamageable.Health != null)
                _pilotDamageable.Health.Damaged -= OnPilotDamaged;
            _pilotDamageable = null;

            player.SetParent(null, true);
            Transform exit = disembarkPoint != null ? disembarkPoint : transform;
            player.position = exit.position;
            player.rotation = Quaternion.Euler(0f, boat.transform.eulerAngles.y, 0f);

            if (_disabledMovement != null)
                foreach (var component in _disabledMovement)
                    SetComponentEnabled(component, true);

            if (launched)
            {
                BoatSwimEntry.Launch(player, impulse, boat.WaterY);
                GameHud.Instance?.Toast("Knocked overboard!");
                _cooldownUntil = Time.time + ejectionCooldown;
            }
            else
            {
                GameHud.Instance?.Toast("Left boat");
                _cooldownUntil = Time.time + 0.25f;
            }

            boat.Disembark();
            _pilotedPlayer = null;
            _disabledMovement = null;
        }

        private static void SetComponentEnabled(Component component, bool enabled)
        {
            if (component == null) return;
            if (component is Behaviour behaviour) behaviour.enabled = enabled;
            else if (component is Collider collider) collider.enabled = enabled;
        }
    }
}
