using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// Glues <see cref="SaveSystem"/> to <see cref="PlayerInventory"/>: loads on start, saves on quit, and
    /// offers manual save (F5) / load (F9) keys with a HUD toast. Put it on a persistent object alongside
    /// the inventory. Note: the character-creation flow still runs on load; this restores the wallet,
    /// lures, owned creatures, and house — the meaningful progression.
    /// </summary>
    public sealed class SaveController : MonoBehaviour
    {
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool saveOnQuit = true;

        private void Start()
        {
            if (loadOnStart && PlayerInventory.Instance != null)
            {
                var data = SaveSystem.Load();
                if (data != null) PlayerInventory.Instance.LoadFrom(data);
            }
        }

        private void OnApplicationQuit()
        {
            if (saveOnQuit) SaveNow();
        }

        private void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;

            if (k.f5Key.wasPressedThisFrame)
            {
                SaveNow();
                GameHud.Instance?.Toast("Saved");
            }
            else if (k.f9Key.wasPressedThisFrame)
            {
                var data = SaveSystem.Load();
                if (data != null)
                {
                    PlayerInventory.Instance?.LoadFrom(data);
                    GameHud.Instance?.Toast("Loaded");
                }
                else GameHud.Instance?.Toast("No save found");
            }
        }

        private void SaveNow()
        {
            if (PlayerInventory.Instance != null) SaveSystem.Save(PlayerInventory.Instance.ToSave());
        }
    }
}
