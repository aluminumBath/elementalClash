using UnityEngine;

namespace Elementborn.Game
{
    public sealed class BossRewardChest : BaseInteractable
    {
        [SerializeField] private BossController boss; [SerializeField] private LootDropTableDefinition bonusLootTable; [SerializeField] private int bonusCurrency; [SerializeField] private bool requireBossDefeated = true; [SerializeField] private bool opened;
        public override InteractionPromptData GetPrompt(GameObject interactor){ return InteractionPromptData.Simple("Boss Reward Chest", opened ? "Opened" : "Open"); }
        public override bool CanInteract(GameObject interactor){ if(!base.CanInteract(interactor) || opened) return false; return !requireBossDefeated || boss == null || boss.State == BossState.Defeated; }
        public override void Interact(GameObject interactor){ if(!CanInteract(interactor)){ NotificationFeed.Post("The chest is sealed.", NotificationType.Warning); return; } opened=true; if(bonusCurrency>0) PlayerInventoryTracker.AddCurrency(bonusCurrency); if(bonusLootTable!=null){ foreach(var entry in bonusLootTable.Entries){ if(entry==null || string.IsNullOrWhiteSpace(entry.ItemId) || !entry.RollDrop()) continue; int qty=entry.RollQuantity(); if(entry.Item!=null) PlayerInventoryTracker.AddItem(entry.Item,qty); else PlayerInventoryTracker.AddItemId(entry.ItemId,qty); }} NotificationFeed.Post("Boss reward chest opened.", NotificationType.Inventory); base.Interact(interactor); }
    }
}
