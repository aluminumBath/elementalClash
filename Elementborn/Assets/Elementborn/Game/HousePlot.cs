using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A claimable home plot. Calling <see cref="TryClaim"/> (from an interaction or UI) buys it and sets
    /// it as the player's respawn home via <see cref="PlayerInventory"/>. Each player has one house, so
    /// claiming a new plot relocates home. Set <c>claimOnEnter</c> to grab it on walk-in (handy for testing).
    /// </summary>
    public sealed class HousePlot : MonoBehaviour
    {
        [SerializeField] private long price = 200;
        [SerializeField] private bool claimOnEnter = false;

        public bool Owned { get; private set; }

        public bool TryClaim()
        {
            var inv = PlayerInventory.Instance;
            if (inv == null || Owned) return false;

            int level = ProgressionController.Instance != null ? ProgressionController.Instance.Progression.Level : 1;
            if (!Homestead.CanClaim(level))
            {
                GameHud.Instance?.Toast("Reach level " + HomesteadCatalog.RequiredLevelToClaim + " to claim a home.");
                return false;
            }

            if (!inv.TryClaimHouse(transform.position, price, out _)) return false;
            Owned = true;
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!claimOnEnter || Owned) return;
            if (other.GetComponentInParent<PlayerInventory>() != null || other.CompareTag("Player"))
                TryClaim();
        }
    }
}
