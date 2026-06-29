using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class PlayerAbilityTracker : MonoBehaviour
    {
        public static PlayerAbilityTracker Instance { get; private set; }

        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int availableSkillPoints = 0;
        [SerializeField] private List<PlayerAbilityRecord> unlockedAbilities = new List<PlayerAbilityRecord>();
        [SerializeField] private List<AbilityLoadoutSlot> loadout = new List<AbilityLoadoutSlot>();

        public int PlayerLevel => Mathf.Max(1, playerLevel);
        public int AvailableSkillPoints => Mathf.Max(0, availableSkillPoints);
        public IReadOnlyList<PlayerAbilityRecord> UnlockedAbilities => unlockedAbilities;
        public IReadOnlyList<AbilityLoadoutSlot> Loadout => loadout;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureDefaultSlots();
        }

        public static PlayerAbilityTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(PlayerAbilityTracker));
            return go.AddComponent<PlayerAbilityTracker>();
        }

        public static bool HasUnlocked(string abilityId)
        {
            return Ensure().FindRecord(abilityId) != null;
        }

        public static PlayerAbilityRecord Unlock(AbilityDefinition ability, AbilityUnlockSource source = AbilityUnlockSource.Unknown, bool spendSkillPoints = true)
        {
            if (ability == null)
            {
                NotificationFeed.Post("No ability selected.", NotificationType.Warning);
                return null;
            }

            return Ensure().UnlockInternal(ability, source, spendSkillPoints);
        }

        public static PlayerAbilityRecord UnlockById(string abilityId, AbilityUnlockSource source = AbilityUnlockSource.Admin)
        {
            return Ensure().UnlockByIdInternal(abilityId, source);
        }

        public static void AddSkillPoints(int amount, string reason = "")
        {
            var tracker = Ensure();
            tracker.availableSkillPoints = Mathf.Max(0, tracker.availableSkillPoints + amount);
            NotificationFeed.Post($"Skill points: {tracker.availableSkillPoints}", NotificationType.Journal);

            if (!string.IsNullOrWhiteSpace(reason))
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "skill_points_" + PlayerJournalTracker.Safe(reason),
                    JournalEntryType.Tutorial,
                    "Skill Points",
                    reason);
            }
        }

        public static void SetPlayerLevel(int level)
        {
            var tracker = Ensure();
            tracker.playerLevel = Mathf.Max(1, level);
            NotificationFeed.Post($"Level set to {tracker.playerLevel}.", NotificationType.Journal);
        }

        public static bool Equip(string abilityId, AbilitySlotType slot)
        {
            return Ensure().EquipInternal(abilityId, slot);
        }

        public static string GetEquipped(AbilitySlotType slot)
        {
            var slotRecord = Ensure().loadout.Find(s => s.SlotType == slot);
            return slotRecord != null ? slotRecord.AbilityId : "";
        }

        public static void MarkUsed(string abilityId)
        {
            var record = Ensure().FindRecord(abilityId);
            if (record == null)
            {
                return;
            }

            record.TimesUsed++;
            record.LastUsedAtUnscaledTime = Time.unscaledTime;
        }

        public static void MarkRead(string abilityId)
        {
            var record = Ensure().FindRecord(abilityId);
            if (record != null)
            {
                record.IsNew = false;
            }
        }

        public bool CanUnlock(AbilityDefinition ability, out string reason)
        {
            reason = "";

            if (ability == null)
            {
                reason = "No ability selected.";
                return false;
            }

            if (HasUnlocked(ability.AbilityId))
            {
                reason = "Already unlocked.";
                return false;
            }

            if (PlayerLevel < ability.UnlockRequirement.RequiredPlayerLevel)
            {
                reason = $"Requires level {ability.UnlockRequirement.RequiredPlayerLevel}.";
                return false;
            }

            if (availableSkillPoints < Mathf.Max(0, ability.UnlockRequirement.SkillPointCost))
            {
                reason = $"Requires {ability.UnlockRequirement.SkillPointCost} skill point(s).";
                return false;
            }

            if (!ability.UnlockRequirement.IsMet(out reason))
            {
                return false;
            }

            return true;
        }

        public PlayerAbilityRecord UnlockInternal(AbilityDefinition ability, AbilityUnlockSource source, bool spendSkillPoints)
        {
            if (ability == null)
            {
                return null;
            }

            if (HasUnlocked(ability.AbilityId))
            {
                return FindRecord(ability.AbilityId);
            }

            if (!CanUnlock(ability, out string reason))
            {
                NotificationFeed.Post(reason, NotificationType.Warning);
                return null;
            }

            if (spendSkillPoints)
            {
                availableSkillPoints = Mathf.Max(0, availableSkillPoints - Mathf.Max(0, ability.UnlockRequirement.SkillPointCost));
            }

            if (!string.IsNullOrWhiteSpace(ability.UnlockRequirement.RequiredItemId)
                && ability.UnlockRequirement.ConsumeRequiredItem)
            {
                PlayerInventoryTracker.RemoveItemId(
                    ability.UnlockRequirement.RequiredItemId,
                    Mathf.Max(1, ability.UnlockRequirement.RequiredItemQuantity));
            }

            var record = new PlayerAbilityRecord
            {
                AbilityId = ability.AbilityId,
                UnlockSource = source == AbilityUnlockSource.Unknown ? ability.DefaultUnlockSource : source,
                IsUnlocked = true,
                IsNew = true
            };

            unlockedAbilities.Add(record);
            NotificationFeed.Post($"Ability unlocked: {ability.DisplayName}", NotificationType.Journal);

            PlayerJournalTracker.AddOrUpdateEntry(
                "ability_" + PlayerJournalTracker.Safe(ability.AbilityId),
                JournalEntryType.Element,
                ability.DisplayName,
                ability.Description,
                relatedId: ability.AbilityId);

            if (!ability.Passive)
            {
                EquipInternal(ability.AbilityId, ability.DefaultSlot);
            }

            return record;
        }

        public PlayerAbilityRecord UnlockByIdInternal(string abilityId, AbilityUnlockSource source)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return null;
            }

            var existing = FindRecord(abilityId);
            if (existing != null)
            {
                return existing;
            }

            var record = new PlayerAbilityRecord
            {
                AbilityId = abilityId,
                UnlockSource = source,
                IsUnlocked = true,
                IsNew = true
            };

            unlockedAbilities.Add(record);
            NotificationFeed.Post($"Ability unlocked: {abilityId}", NotificationType.Journal);
            return record;
        }

        public bool EquipInternal(string abilityId, AbilitySlotType slot)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return false;
            }

            if (!HasUnlocked(abilityId))
            {
                NotificationFeed.Post($"Ability not unlocked: {abilityId}", NotificationType.Warning);
                return false;
            }

            EnsureDefaultSlots();
            var slotRecord = loadout.Find(s => s.SlotType == slot);
            if (slotRecord == null)
            {
                slotRecord = new AbilityLoadoutSlot { SlotType = slot };
                loadout.Add(slotRecord);
            }

            slotRecord.AbilityId = abilityId;
            NotificationFeed.Post($"Equipped {abilityId} to {slot}.", NotificationType.Journal);
            return true;
        }

        public PlayerAbilityRecord FindRecord(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return null;
            }

            return unlockedAbilities.Find(a => a.AbilityId == abilityId && a.IsUnlocked);
        }

        public void ImportRecord(PlayerAbilityRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.AbilityId))
            {
                return;
            }

            unlockedAbilities.RemoveAll(a => a.AbilityId == record.AbilityId);
            unlockedAbilities.Add(record);
        }

        public void ImportLoadoutSlot(AbilityLoadoutSlot slot)
        {
            if (slot == null)
            {
                return;
            }

            var existing = loadout.Find(s => s.SlotType == slot.SlotType);
            if (existing == null)
            {
                loadout.Add(slot);
            }
            else
            {
                existing.AbilityId = slot.AbilityId;
            }
        }

        public void ImportLevelAndPoints(int level, int points)
        {
            playerLevel = Mathf.Max(1, level);
            availableSkillPoints = Mathf.Max(0, points);
        }

        public void Clear()
        {
            unlockedAbilities.Clear();
            loadout.Clear();
            playerLevel = 1;
            availableSkillPoints = 0;
            EnsureDefaultSlots();
        }

        private void EnsureDefaultSlots()
        {
            EnsureSlot(AbilitySlotType.Primary);
            EnsureSlot(AbilitySlotType.Secondary);
            EnsureSlot(AbilitySlotType.Utility);
            EnsureSlot(AbilitySlotType.Traversal);
            EnsureSlot(AbilitySlotType.Boat);
            EnsureSlot(AbilitySlotType.Creature);
            EnsureSlot(AbilitySlotType.Passive);
            EnsureSlot(AbilitySlotType.Ultimate);
        }

        private void EnsureSlot(AbilitySlotType slot)
        {
            if (loadout.Find(s => s.SlotType == slot) == null)
            {
                loadout.Add(new AbilityLoadoutSlot { SlotType = slot });
            }
        }
    }
}
