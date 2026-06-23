using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A weeping-willow gate. Its barrier parts only for a plant user who comes near, and closes again behind
    /// them — so to anyone else it's an impassable curtain of branches. The marshes are full of them. Assign the
    /// blocking <see cref="barrier"/> collider (defaults to this object's), and optionally a visual to hide while
    /// open.
    /// </summary>
    public sealed class WillowGate : MonoBehaviour
    {
        [SerializeField] private Collider barrier;
        [SerializeField] private GameObject visual;
        [SerializeField] private float openRange = PlantTuning.GateOpenRange;

        private Transform _player;
        private PlayerCombatController _playerCombat;
        private bool _open;

        private void Awake()
        {
            if (barrier == null) barrier = GetComponent<Collider>();
        }

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                _player = p.transform;
                _playerCombat = p.GetComponentInParent<PlayerCombatController>();
            }
            SetOpen(false);
        }

        private void Update()
        {
            if (_player == null) return;
            bool near = Vector3.Distance(_player.position, transform.position) <= openRange;
            var loadout = _playerCombat != null ? _playerCombat.Loadout : null;
            bool shouldOpen = near && PlantControl.CanOpenGate(loadout);
            if (shouldOpen != _open) SetOpen(shouldOpen);
        }

        private void SetOpen(bool open)
        {
            _open = open;
            if (barrier != null) barrier.enabled = !open; // passable when open
            if (visual != null) visual.SetActive(!open);
        }
    }
}
