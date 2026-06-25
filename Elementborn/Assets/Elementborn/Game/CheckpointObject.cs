using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// An in-world respawn shrine. Standing in range offers an Interact to set it as your respawn anchor (through
    /// <see cref="CheckpointState"/>). Spawned by <see cref="CheckpointSpawner"/> from the canonical
    /// <see cref="WorldMapLayout"/> list, so it offers its interaction through the shared <see cref="InteractionArbiter"/>.
    /// </summary>
    public sealed class CheckpointObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string checkpointId;
        [SerializeField] private string checkpointName;
        [SerializeField] private float interactRange = 5f;

        public void Configure(Checkpoint cp)
        {
            checkpointId = cp.Id;
            checkpointName = cp.Name;
            transform.position = cp.World;
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > interactRange) return false;
            bool active = CheckpointState.Instance != null && CheckpointState.Instance.IsActive(checkpointId);
            interaction = new Interaction(d, 0, active ? "Respawn set here" : "Set respawn point", Activate);
            return true;
        }

        private void Activate()
        {
            if (CheckpointState.Instance != null && CheckpointState.Instance.Activate(checkpointId))
            {
                GameHud.Instance?.Toast("Respawn point set — " + checkpointName);
                AudioController.Instance?.Confirm();
                GameEventLogger.Instance?.LogSpawn(checkpointName);
            }
        }
    }
}
