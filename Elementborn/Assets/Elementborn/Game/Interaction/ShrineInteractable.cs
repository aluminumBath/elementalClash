using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ShrineInteractable : BaseInteractable
    {
        [SerializeField] private string shrineName = "Shrine";
        [SerializeField] private bool markAsFastTravel;
        [SerializeField] private bool healPlayer = true;

        private void Start()
        {
            PlayerMapMarkerTracker.ReportShrine(transform.position, shrineName);
            if (markAsFastTravel)
            {
                PlayerMapMarkerTracker.ReportFastTravel(transform.position, shrineName);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(shrineName, markAsFastTravel ? "Attune / Travel" : "Pray");
        }

        public override void Interact(GameObject interactor)
        {
            if (healPlayer)
            {
                Debug.Log($"{shrineName} restores the player. Hook this into Damageable/Inventory later.");
            }

            base.Interact(interactor);
        }
    }
}
