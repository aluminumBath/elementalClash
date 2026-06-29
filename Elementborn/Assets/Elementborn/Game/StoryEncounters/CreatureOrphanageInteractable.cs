using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureOrphanageInteractable : BaseInteractable
    {
        [SerializeField] private CreatureOrphanageHealingService healingService;
        [SerializeField] private string prompt = "Visit the crab-sign creature orphanage";

        private void Awake()
        {
            if (healingService == null)
            {
                healingService = GetComponent<CreatureOrphanageHealingService>();
                if (healingService == null)
                {
                    healingService = gameObject.AddComponent<CreatureOrphanageHealingService>();
                }
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = prompt;
            return InteractionPromptData.Simple(title, "Visit");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return healingService != null;
        }

        public override void Interact(GameObject interactor)
        {
            healingService.HealRegisteredCreatures();
            PlayerJournalTracker.AddOrUpdateEntry(
                "crab_sign_orphanage",
                JournalEntryType.Location,
                "Crab-Sign Creature Orphanage",
                "Ella and Eloc run a hilarious, loving creature orphanage marked by a crab symbol. They heal creatures brought to them.",
                "Neritha Reefwood",
                "crab_sign_orphanage");
        }
    }
}
