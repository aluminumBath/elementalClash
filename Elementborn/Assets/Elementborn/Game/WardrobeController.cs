using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Applies the player's chosen cosmetic Channeler look (from the home <see cref="Wardrobe"/> on
    /// <see cref="PlayerInventory"/>, gated behind the built Wardrobe addition and the look's unlock level) by
    /// swapping the player's display mesh — appearance only, never their element, stats, or abilities. Mirrors
    /// <see cref="CreatureModelLibrary"/>'s attach-the-model-and-hide-the-old-renderers approach. The look persists
    /// with the save. The <see cref="WardrobeViewer"/> panel drives selection; <see cref="SelectLook"/> is public.
    /// </summary>
    public sealed class WardrobeController : MonoBehaviour
    {
        public static WardrobeController Instance { get; private set; }

        private const string AttachedName = "Wardrobe_Look";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Start() { ApplyCurrent(); }   // restore the saved look once the player exists

        private static int PlayerLevel() =>
            ProgressionController.Instance != null ? ProgressionController.Instance.Progression.Level : 1;

        /// <summary>Whether a look can be selected right now (Wardrobe built + unlocked at the current level).</summary>
        public bool IsAvailable(ChannelerLook look)
        {
            var inv = PlayerInventory.Instance;
            return inv != null && inv.Home.Has(HomeAddition.Wardrobe) && WardrobeCatalog.IsUnlocked(look, PlayerLevel());
        }

        /// <summary>Pick a look: gated behind a built Wardrobe and the unlock level; persists + applies on success,
        /// explains the block otherwise. Never changes the player's element — appearance only.</summary>
        public bool SelectLook(ChannelerLook look)
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) return false;
            if (!inv.Home.Has(HomeAddition.Wardrobe))
            {
                GameHud.Instance?.Toast("Build a Wardrobe at your home first.");
                return false;
            }
            int level = PlayerLevel();
            if (!WardrobeCatalog.IsUnlocked(look, level))
            {
                GameHud.Instance?.Toast(WardrobeCatalog.DisplayName(look) + " unlocks at level "
                    + WardrobeCatalog.RequiredLevelFor(look) + ".");
                return false;
            }
            if (!inv.Wardrobe.TrySelect(look, true, level)) return false;
            ApplyToPlayer(look);
            GameHud.Instance?.Toast("Now wearing: " + WardrobeCatalog.DisplayName(look));
            AudioController.Instance?.Confirm();
            return true;
        }

        private void ApplyCurrent()
        {
            var inv = PlayerInventory.Instance;
            if (inv != null && inv.Home.Has(HomeAddition.Wardrobe)) ApplyToPlayer(inv.Wardrobe.Current);
        }

        private void ApplyToPlayer(ChannelerLook look)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            GameObject prefab = null;
            foreach (var path in WardrobeCatalog.CandidatePaths(look))
            {
                prefab = Resources.Load<GameObject>(path);
                if (prefab != null) break;
            }
            if (prefab == null)
            {
                GameHud.Instance?.Toast(WardrobeCatalog.DisplayName(look) + " isn't installed yet.");
                return; // keep the current visual rather than blanking the player
            }

            // Remove a previously-worn look so skins don't stack.
            var existing = player.transform.Find(AttachedName);
            if (existing != null) Destroy(existing.gameObject);

            // Hide whatever currently renders the player (blocky base / prior visual), then add the chosen look.
            var prior = player.GetComponentsInChildren<Renderer>(true);

            var model = Instantiate(prefab, player.transform);
            model.name = AttachedName;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            foreach (var r in prior)
                if (r != null && !r.transform.IsChildOf(model.transform)) r.enabled = false;
        }
    }
}
