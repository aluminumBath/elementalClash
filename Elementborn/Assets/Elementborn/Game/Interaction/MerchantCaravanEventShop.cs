using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Activates a vendor object when a merchant caravan event is active.
    /// </summary>
    public sealed class MerchantCaravanEventShop : MonoBehaviour
    {
        [SerializeField] private string requiredWorldEventId = "merchant_caravan_arrival";
        [SerializeField] private GameObject shopRoot;
        [SerializeField] private bool hideWhenInactive = true;

        private void Awake()
        {
            if (shopRoot == null)
            {
                shopRoot = gameObject;
            }
        }

        private void Update()
        {
            bool active = string.IsNullOrWhiteSpace(requiredWorldEventId) || WorldEventTracker.IsActive(requiredWorldEventId);
            if (hideWhenInactive && shopRoot != null && shopRoot.activeSelf != active)
            {
                shopRoot.SetActive(active);
            }
        }
    }
}
