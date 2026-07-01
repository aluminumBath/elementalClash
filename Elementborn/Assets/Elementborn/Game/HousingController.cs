using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Builds additions onto the player's home and ticks the Garden each frame. Reads the home-owned flag + silver
    /// from <see cref="PlayerInventory"/> and the level from <see cref="ProgressionController"/>, validates through
    /// the pure <see cref="Homestead"/> (held on PlayerInventory, so it saves with the house), spends silver on
    /// success, and toasts the outcome. The <see cref="HomeMenuViewer"/> panel is the build/use UI; <see cref="TryBuild"/> is public.
    /// </summary>
    public sealed class HousingController : MonoBehaviour
    {
        public static HousingController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            var inv = PlayerInventory.Instance;
            if (inv != null && inv.Home.Has(HomeAddition.Garden)) inv.Garden.Accrue(Time.deltaTime);
        }

        /// <summary>Attempt to build one addition: spends silver and toasts on success, explains the block otherwise.</summary>
        public BuildOutcome TryBuild(HomeAddition a)
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) return BuildOutcome.NoHouse;

            int level = ProgressionController.Instance != null ? ProgressionController.Instance.Progression.Level : 1;
            int funds = inv.Wallet.CountOf(Currency.Silver);

            var outcome = inv.Home.TryBuild(a, inv.HasHouse, level, funds, out int cost);
            switch (outcome)
            {
                case BuildOutcome.Built:
                    inv.AddCurrency(Currency.Silver, -cost);
                    GameHud.Instance?.Toast("Built: " + HomesteadCatalog.DisplayName(a));
                    break;
                case BuildOutcome.NoHouse:
                    GameHud.Instance?.Toast("Claim a home first.");
                    break;
                case BuildOutcome.AlreadyBuilt:
                    GameHud.Instance?.Toast(HomesteadCatalog.DisplayName(a) + " is already built.");
                    break;
                case BuildOutcome.NeedLevel:
                    GameHud.Instance?.Toast(HomesteadCatalog.DisplayName(a) + " needs level "
                        + HomesteadCatalog.RequiredLevelFor(a) + ".");
                    break;
                case BuildOutcome.NeedFunds:
                    GameHud.Instance?.Toast("Need " + cost + " silver for " + HomesteadCatalog.DisplayName(a) + ".");
                    break;
            }
            return outcome;
        }
    }
}
