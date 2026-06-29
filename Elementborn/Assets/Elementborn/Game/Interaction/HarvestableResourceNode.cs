using UnityEngine;

namespace Elementborn.Game
{
    public sealed class HarvestableResourceNode : BaseInteractable
    {
        [SerializeField] private ResourceNodeDefinition nodeDefinition;
        [SerializeField] private HarvestNodeState state = HarvestNodeState.Available;
        [SerializeField] private int harvestsRemaining = -1;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private float respawnAtTime = -1f;
        [SerializeField] private string respawnWorldEventId = "";
        [SerializeField] private GameObject availableVisual;
        [SerializeField] private GameObject depletedVisual;
        [SerializeField] private Collider harvestCollider;

        public string RuntimeNodeId => nodeDefinition != null ? nodeDefinition.NodeId + "_" + Mathf.RoundToInt(transform.position.x) + "_" + Mathf.RoundToInt(transform.position.z) : name;
        public HarvestNodeState State => state;

        private void Awake()
        {
            if (harvestsRemaining < 0 && nodeDefinition != null)
            {
                harvestsRemaining = nodeDefinition.MaxHarvestsBeforeDepleted;
            }

            if (harvestCollider == null)
            {
                harvestCollider = GetComponent<Collider>();
            }

            RefreshVisuals();
        }

        private void Start()
        {
            if (nodeDefinition != null && nodeDefinition.AddMapMarker)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "resource_node_" + PlayerJournalTracker.Safe(RuntimeNodeId),
                    nodeDefinition.MarkerType,
                    transform.position,
                    nodeDefinition.DisplayName,
                    isPersistent: true,
                    notes: nodeDefinition.Description);
            }
        }

        private void Update()
        {
            if (state != HarvestNodeState.Depleted)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(respawnWorldEventId) && WorldEventTracker.IsActive(respawnWorldEventId))
            {
                Respawn();
                return;
            }

            if (nodeDefinition == null || nodeDefinition.RespawnSeconds < 0f || respawnAtTime < 0f)
            {
                return;
            }

            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            if (now >= respawnAtTime)
            {
                Respawn();
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = nodeDefinition != null ? nodeDefinition.DisplayName : "Resource";
            string action = state == HarvestNodeState.Available ? "Harvest" : state.ToString();
            return InteractionPromptData.Simple(label, action);
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && state == HarvestNodeState.Available && nodeDefinition != null;
        }

        public override void Interact(GameObject interactor)
        {
            Harvest();
            base.Interact(interactor);
        }

        public HarvestRollResult Harvest()
        {
            var result = new HarvestRollResult();

            if (nodeDefinition == null)
            {
                result.Message = "No resource definition assigned.";
                NotificationFeed.Post(result.Message, NotificationType.Warning);
                return result;
            }

            if (state != HarvestNodeState.Available)
            {
                result.Message = $"{nodeDefinition.DisplayName} is not available.";
                NotificationFeed.Post(result.Message, NotificationType.Warning);
                return result;
            }

            if (!nodeDefinition.Requirement.IsMet(out string reason))
            {
                result.Message = reason;
                NotificationFeed.Post(reason, NotificationType.Warning);
                return result;
            }

            foreach (var yield in nodeDefinition.Yields)
            {
                if (yield == null || string.IsNullOrWhiteSpace(yield.ItemId))
                {
                    continue;
                }

                float bonus = yield.RareYield ? nodeDefinition.RareYieldBonus : 0f;
                if (!yield.ShouldDrop(bonus))
                {
                    continue;
                }

                int quantity = yield.RollQuantity();
                InventoryTransactionResult add = yield.Item != null
                    ? PlayerInventoryTracker.AddItem(yield.Item, quantity)
                    : PlayerInventoryTracker.AddItemId(yield.ItemId, quantity);

                if (add.Moved > 0)
                {
                    result.AnyYield = true;
                    result.AnyRareYield |= yield.RareYield;
                    result.AddedItems.Add(add);
                }
            }

            if (!result.AnyYield)
            {
                result.Message = $"Nothing useful harvested from {nodeDefinition.DisplayName}.";
                NotificationFeed.Post(result.Message, NotificationType.Info);
            }
            else
            {
                result.Message = $"Harvested {nodeDefinition.DisplayName}.";
                NotificationFeed.Post(result.Message, result.AnyRareYield ? NotificationType.Journal : NotificationType.Inventory);
            }

            ResourceHarvestingTracker.RecordHarvest(nodeDefinition, result.AnyRareYield);

            harvestsRemaining--;
            if (harvestsRemaining <= 0)
            {
                Deplete();
            }

            return result;
        }

        public void Deplete()
        {
            state = HarvestNodeState.Depleted;
            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            respawnAtTime = nodeDefinition != null && nodeDefinition.RespawnSeconds >= 0f
                ? now + nodeDefinition.RespawnSeconds
                : -1f;

            RefreshVisuals();
        }

        public void Respawn()
        {
            state = HarvestNodeState.Available;
            harvestsRemaining = nodeDefinition != null ? nodeDefinition.MaxHarvestsBeforeDepleted : 1;
            respawnAtTime = -1f;
            RefreshVisuals();

            if (nodeDefinition != null)
            {
                NotificationFeed.Post($"{nodeDefinition.DisplayName} has regrown.", NotificationType.Map);
            }
        }

        public void SetRespawnWorldEventId(string eventId)
        {
            respawnWorldEventId = eventId ?? "";
        }

        public void ImportState(HarvestNodeState importedState, int importedRemaining, float importedRespawnAt)
        {
            state = importedState;
            harvestsRemaining = importedRemaining;
            respawnAtTime = importedRespawnAt;
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            bool available = state == HarvestNodeState.Available;

            if (availableVisual != null)
            {
                availableVisual.SetActive(available);
            }

            if (depletedVisual != null)
            {
                depletedVisual.SetActive(!available);
            }

            if (harvestCollider != null)
            {
                harvestCollider.enabled = state != HarvestNodeState.Hidden;
            }
        }
    }
}
