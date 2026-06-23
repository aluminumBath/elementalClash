using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Holds the player's progress feeding Willow's companions. <see cref="Feed"/> records one (called by a
    /// <see cref="SidekickFeedPoint"/> on each companion); once all are fed within the window, Willow's hidden
    /// ability hint unlocks. Put one on the player rig.
    /// </summary>
    public sealed class SidekickFeedingController : MonoBehaviour
    {
        public static SidekickFeedingController Instance { get; private set; }

        [SerializeField] private float windowDays = 2f;

        private readonly SidekickFeedingTracker _tracker = new SidekickFeedingTracker();
        private bool _hintUnlocked;

        public bool HintUnlocked => _hintUnlocked;
        public int FedCount => _tracker.FedCount;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Feed(WillowSidekick sidekick)
        {
            var inv = PlayerInventory.Instance;
            string foodId = ItemCatalog.FoodFor(sidekick);
            if (inv == null || !inv.Items.Remove(foodId))
            {
                var def = ItemCatalog.Get(foodId);
                GameHud.Instance?.Toast("You need " + (def != null ? def.Name : "the right food") +
                                        " to feed " + WillowSidekicks.For(sidekick).Name + ".");
                return;
            }
            _tracker.Feed(sidekick, Time.timeAsDouble);
            if (_tracker.AllFedWithin(windowDays * 86400.0)) _hintUnlocked = true;
            GameHud.Instance?.Toast("You fed " + WillowSidekicks.For(sidekick).Name + ".");
            Debug.Log($"[Willow] You fed {WillowSidekicks.For(sidekick).Name}. ({_tracker.FedCount}/{WillowSidekicks.All.Length})");
        }
    }
}
