using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Lightweight inventory save bridge.
    /// Saves item IDs/quantities. Runtime item definition references are restored when items are added by ID.
    /// Later, connect this to the project's main SaveSystem and item catalog.
    /// </summary>
    public sealed class InventorySaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot = 0;
        [SerializeField] private bool includeSceneStorage = true;
        [SerializeField] private bool autoLoadOnStart = false;
        [SerializeField] private bool autoSaveOnApplicationPause = true;
        [SerializeField] private bool autoSaveOnApplicationQuit = true;

        private void Start()
        {
            if (autoLoadOnStart)
            {
                LoadCurrentSlot();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && autoSaveOnApplicationPause)
            {
                SaveCurrentSlot();
            }
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnApplicationQuit)
            {
                SaveCurrentSlot();
            }
        }

        public void SetCurrentSlot(int slot)
        {
            currentSlot = Mathf.Max(0, slot);
        }

        public void SaveCurrentSlot()
        {
            SaveSlot(currentSlot);
        }

        public void LoadCurrentSlot()
        {
            LoadSlot(currentSlot);
        }

        public void SaveSlot(int slot)
        {
            var inventory = PlayerInventoryTracker.Ensure();
            var save = new InventorySaveFile
            {
                SlotIndex = Mathf.Max(0, slot),
                Currency = inventory.Currency
            };

            foreach (var stack in inventory.Stacks)
            {
                if (stack == null || stack.IsEmpty)
                {
                    continue;
                }

                save.Stacks.Add(new InventorySaveStack
                {
                    ItemId = stack.ResolvedItemId,
                    Quantity = stack.Quantity
                });
            }

            if (includeSceneStorage)
            {
                foreach (var storage in ElementbornFindUtility.FindAll<StorageContainerInventory>())
                {
                    var record = new StorageSaveRecord
                    {
                        StorageId = storage.StorageId
                    };

                    foreach (var stack in storage.Stacks)
                    {
                        if (stack == null || stack.IsEmpty)
                        {
                            continue;
                        }

                        record.Stacks.Add(new InventorySaveStack
                        {
                            ItemId = stack.ResolvedItemId,
                            Quantity = stack.Quantity
                        });
                    }

                    save.Storage.Add(record);
                }
            }

            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
            Debug.Log($"Saved inventory for slot {slot}: {save.Stacks.Count} stack(s), {save.Storage.Count} storage container(s).");
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                Debug.Log($"No inventory save exists for slot {slot}: {path}");
                return;
            }

            var save = JsonUtility.FromJson<InventorySaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                Debug.LogWarning($"Failed to read inventory save: {path}");
                return;
            }

            var inventory = PlayerInventoryTracker.Ensure();
            inventory.Clear();

            if (save.Currency > 0)
            {
                PlayerInventoryTracker.AddCurrency(save.Currency);
            }

            foreach (var stack in save.Stacks)
            {
                inventory.ImportSavedStack(stack.ItemId, stack.Quantity);
            }

            if (includeSceneStorage)
            {
                foreach (var storage in ElementbornFindUtility.FindAll<StorageContainerInventory>())
                {
                    storage.Clear();
                    var record = save.Storage.Find(s => s.StorageId == storage.StorageId);
                    if (record == null)
                    {
                        continue;
                    }

                    foreach (var stack in record.Stacks)
                    {
                        storage.ImportSavedStack(stack.ItemId, stack.Quantity);
                    }
                }
            }

            Debug.Log($"Loaded inventory for slot {slot}.");
        }

        public void DeleteSlot(int slot)
        {
            string path = GetPath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "inventory");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_inventory.json");
        }
    }
}
