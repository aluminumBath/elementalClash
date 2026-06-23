using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A patch of grasping vines. Non-plant-users who blunder in <em>trip</em> (a brief stumble + slow); plant
    /// users pass freely. While inside, holding the climb input (Dash) lets anyone climb it. A plant user can
    /// <see cref="Snare"/> it (via <see cref="PlantControlController"/>) to seize and hold nearby foes. Needs a
    /// trigger collider marking the patch. (Full hand-over-hand / VR climbing is a locomotion follow-up; this is
    /// a simple ascend assist.)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class VinePatch : MonoBehaviour
    {
        [SerializeField] private bool climbable = true;
        [SerializeField] private float snareReach = 3.5f;

        private Transform _player;
        private PlayerCombatController _playerCombat;
        private CharacterController _playerController;
        private bool _playerInside;

        private void Awake()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            _player = p.transform;
            _playerCombat = p.GetComponentInParent<PlayerCombatController>();
            _playerController = p.GetComponentInParent<CharacterController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerCombatController>() == null) return;
            _playerInside = true;
            if (!PlantControl.IsPlantUser(LoadoutOf())) // the unwary stumble; plant users walk through
            {
                var d = other.GetComponentInParent<IDamageable>();
                d?.ApplyStatus(new StatusEffect(StatusKind.Stun, 1f, PlantTuning.VineTripStun));
                d?.ApplyStatus(new StatusEffect(StatusKind.Slow, PlantTuning.VineSlow, PlantTuning.VineSlowDuration));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerCombatController>() != null) _playerInside = false;
        }

        private void Update()
        {
            if (climbable && _playerInside && InputBindings.Dash.IsPressed())
            {
                Vector3 up = Vector3.up * (PlantTuning.VineClimbSpeed * Time.deltaTime);
                if (_playerController != null) _playerController.Move(up);
                else if (_player != null) _player.position += up;
            }
        }

        /// <summary>Plant-user command: lash the vines, seizing and holding nearby foes.</summary>
        public void Snare()
        {
            var hits = Physics.OverlapSphere(transform.position, snareReach);
            foreach (var h in hits)
            {
                if (h.GetComponentInParent<PlayerCombatController>() != null) continue; // not the caster
                var d = h.GetComponentInParent<IDamageable>();
                d?.ApplyStatus(new StatusEffect(StatusKind.Control, 1f, PlantTuning.VineSnareHold));
                d?.ApplyStatus(new StatusEffect(StatusKind.Slow, PlantTuning.VineSlow, PlantTuning.VineSlowDuration));
            }
        }

        private ChannelerLoadout LoadoutOf() => _playerCombat != null ? _playerCombat.Loadout : null;
    }
}
