using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class RecipeBookSaveBridge : MonoBehaviour
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
            var tracker = RecipeBookTracker.Ensure();
            var save = new RecipeBookSaveFile { SlotIndex = Mathf.Max(0, slot) };

            foreach (var recipeId in tracker.KnownRecipeIds)
            {
                save.KnownRecipeIds.Add(recipeId);
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

            var save = JsonUtility.FromJson<RecipeBookSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            RecipeBookTracker.Clear();
            foreach (var recipeId in save.KnownRecipeIds)
            {
                RecipeBookTracker.Ensure().ImportKnownRecipe(recipeId);
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "recipes");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_recipes.json");
        }
    }
}
