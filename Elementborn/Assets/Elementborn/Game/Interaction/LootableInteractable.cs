using UnityEngine;

namespace Elementborn.Game
{
    public sealed class LootableInteractable : BaseInteractable
    {
        [SerializeField] private string lootableName = "Loot";
        [SerializeField] private LootTableDefinition lootTable;
        [SerializeField] private bool looted;
        [SerializeField] private bool removeMapMarkerOnLoot = true;

        private string markerId;

        private void Start()
        {
            markerId = PlayerMapMarkerTracker.PositionMarkerId("treasure", transform.position);
            PlayerMapMarkerTracker.ReportTreasure(transform.position, lootableName);
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(lootableName, looted ? "Empty" : "Loot");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && !looted;
        }

        public override void Interact(GameObject interactor)
        {
            if (looted)
            {
                return;
            }

            looted = true;

            if (lootTable != null)
            {
                foreach (var result in lootTable.Roll())
                {
                    PlayerInventoryTracker.AddItem(result.Item, result.Quantity);
                }
            }

            if (removeMapMarkerOnLoot)
            {
                PlayerMapMarkerTracker.RemoveMarker(markerId);
            }

            base.Interact(interactor);
        }
    }
}
