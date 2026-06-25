using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Summon something to ride with one toggle (M by default, or a VR button). Prefers a tamed rideable
    /// creature; if you own none but own a vehicle, it summons the vehicle instead. Toggling again dismisses
    /// it. Reuses the shared creature/vehicle prefabs as placeholders until per-asset art exists. Put this
    /// on the player rig.
    /// </summary>
    public sealed class MountSummoner : MonoBehaviour
    {
        [SerializeField] private GameObject creaturePrefab;
        [SerializeField] private GameObject vehiclePrefab;
        [SerializeField] private InputActionReference summonAction;
        [SerializeField] private float spawnDistance = 2.5f;

        private MountController _mount;
        private GameObject _summoned;

        private void Update()
        {
            if (Pressed()) Toggle();
        }

        public void Toggle()
        {
            if (_mount != null) { Dismiss(); return; }

            var inv = PlayerInventory.Instance;
            if (inv == null) return;

            CreatureKind? ride = FirstRideable(inv);
            if (ride.HasValue) { SummonCreature(ride.Value); return; }

            VehicleKind? craft = FirstVehicle(inv);
            if (craft.HasValue) { SummonVehicle(craft.Value); return; }

            Debug.Log("[Elementborn] Nothing rideable owned.");
        }

        private void SummonCreature(CreatureKind kind)
        {
            if (creaturePrefab == null) { Debug.Log("[Elementborn] No creature prefab assigned."); return; }

            Vector3 at = transform.position + transform.forward * spawnDistance;
            _summoned = Instantiate(creaturePrefab, at, Quaternion.identity);

            // It's a controlled mount, not a wild beast: stop roaming and taming.
            var creature = _summoned.GetComponent<CreatureController>();
            if (creature != null) creature.enabled = false;
            var tameable = _summoned.GetComponent<Tameable>();
            if (tameable != null) tameable.enabled = false;

            var info = CreatureCatalog.For(kind);
            var fm = _summoned.GetComponent<FactionMember>();
            if (fm == null) fm = _summoned.AddComponent<FactionMember>();
            fm.Configure(Faction.Ally, info.Element);

            _mount = _summoned.GetComponent<MountController>();
            if (_mount == null) _mount = _summoned.AddComponent<MountController>();
            _mount.Configure(Locomotion.For(kind), false, WorldWater.SeaLevelY, 1f);
            _mount.Mount(gameObject);
            CreatureModelLibrary.Attach(kind, _summoned); // its CreatureController is disabled, so attach here
        }

        private void SummonVehicle(VehicleKind kind)
        {
            var prefab = vehiclePrefab != null ? vehiclePrefab : creaturePrefab;
            if (prefab == null) { Debug.Log("[Elementborn] No vehicle prefab assigned."); return; }

            var info = VehicleCatalog.For(kind);
            Vector3 at = transform.position + transform.forward * spawnDistance;
            _summoned = Instantiate(prefab, at, Quaternion.identity);

            // If the placeholder is a creature prefab, silence its wild behaviour.
            var creature = _summoned.GetComponent<CreatureController>();
            if (creature != null) creature.enabled = false;
            var tameable = _summoned.GetComponent<Tameable>();
            if (tameable != null) tameable.enabled = false;

            var fm = _summoned.GetComponent<FactionMember>();
            if (fm == null) fm = _summoned.AddComponent<FactionMember>();
            fm.Configure(Faction.Ally, info.RequiredElement);

            _mount = _summoned.GetComponent<MountController>();
            if (_mount == null) _mount = _summoned.AddComponent<MountController>();
            _mount.Configure(info.Locomotion, false, WorldWater.SeaLevelY, 1f);
            _mount.Mount(gameObject);
        }

        private void Dismiss()
        {
            if (_mount != null) _mount.Dismount();
            if (_summoned != null) Destroy(_summoned);
            _mount = null;
            _summoned = null;
        }

        private static CreatureKind? FirstRideable(PlayerInventory inv)
        {
            foreach (var k in inv.Owned)
                if (CreatureCatalog.For(k).Rideable) return k;
            return null;
        }

        private static VehicleKind? FirstVehicle(PlayerInventory inv)
        {
            foreach (var v in inv.OwnedVehicles) return v;
            return null;
        }

        private bool Pressed()
        {
            if (summonAction != null && summonAction.action != null && summonAction.action.enabled)
                return summonAction.action.WasPressedThisFrame();
            return InputBindings.Mount.WasPressedThisFrame();
        }
    }
}
