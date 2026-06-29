using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ShopSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot;
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
            var save = new ShopSaveFile { SlotIndex = Mathf.Max(0, slot) };

            foreach (var shop in ElementbornFindUtility.FindAll<ShopRuntimeInventory>())
            {
                if (shop == null || shop.ShopDefinition == null)
                {
                    continue;
                }

                var record = new ShopSaveRecord
                {
                    ShopId = shop.ShopDefinition.ShopId,
                    LastRestockedAtUnscaledTime = shop.LastRestockedAtUnscaledTime
                };

                foreach (var listing in shop.Listings)
                {
                    if (listing == null || listing.InfiniteStock)
                    {
                        continue;
                    }

                    record.Stock.Add(new ShopStockSaveRecord
                    {
                        ItemId = listing.ItemId,
                        Stock = listing.Stock
                    });
                }

                save.Shops.Add(record);
            }

            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                return;
            }

            var save = JsonUtility.FromJson<ShopSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            foreach (var shop in ElementbornFindUtility.FindAll<ShopRuntimeInventory>())
            {
                if (shop == null || shop.ShopDefinition == null)
                {
                    continue;
                }

                var record = save.Shops.Find(s => s.ShopId == shop.ShopDefinition.ShopId);
                if (record == null)
                {
                    continue;
                }

                foreach (var stock in record.Stock)
                {
                    shop.ImportStock(stock.ItemId, stock.Stock);
                }
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "shops");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_shops.json");
        }
    }
}
