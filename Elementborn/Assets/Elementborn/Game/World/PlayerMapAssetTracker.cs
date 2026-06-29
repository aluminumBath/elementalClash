using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Persistent per-session tracker for map markers that belong to the player
    /// but are not necessarily at the player's current position:
    /// - owned/current boat
    /// - last-ridden creature/mount
    ///
    /// This is intentionally additive and can be called from boat, mount, save,
    /// and map UI systems without creating tight dependencies among them.
    /// </summary>
    public sealed class PlayerMapAssetTracker : MonoBehaviour
    {
        public static PlayerMapAssetTracker Instance { get; private set; }

        [Header("Boat")]
        [SerializeField] private bool hasBoat;
        [SerializeField] private Vector3 boatWorldPosition;
        [SerializeField] private bool boatCurrentlyRidden;

        [Header("Last Ridden Creature")]
        [SerializeField] private bool hasLastRiddenCreature;
        [SerializeField] private Vector3 lastRiddenCreatureWorldPosition;
        [SerializeField] private bool creatureCurrentlyRidden;
        [SerializeField] private string lastRiddenCreatureName = string.Empty;
        [SerializeField] private Element lastRiddenCreatureElement;
        [SerializeField] private CreatureTraversalType lastRiddenCreatureTraversalType = CreatureTraversalType.Unknown;

        public bool HasBoat => hasBoat;
        public Vector3 BoatWorldPosition => boatWorldPosition;
        public bool BoatCurrentlyRidden => boatCurrentlyRidden;

        public bool HasLastRiddenCreature => hasLastRiddenCreature;
        public Vector3 LastRiddenCreatureWorldPosition => lastRiddenCreatureWorldPosition;
        public bool CreatureCurrentlyRidden => creatureCurrentlyRidden;
        public string LastRiddenCreatureName => lastRiddenCreatureName;
        public Element LastRiddenCreatureElement => lastRiddenCreatureElement;
        public CreatureTraversalType LastRiddenCreatureTraversalType => lastRiddenCreatureTraversalType;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static PlayerMapAssetTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(PlayerMapAssetTracker));
            return go.AddComponent<PlayerMapAssetTracker>();
        }

        public static void ReportBoatPosition(Vector3 worldPosition, bool currentlyRidden)
        {
            var tracker = Ensure();
            tracker.hasBoat = true;
            tracker.boatWorldPosition = worldPosition;
            tracker.boatCurrentlyRidden = currentlyRidden;
        }

        public static void ClearBoat()
        {
            var tracker = Ensure();
            tracker.hasBoat = false;
            tracker.boatCurrentlyRidden = false;
        }

        public static void ReportCreaturePosition(
            Vector3 worldPosition,
            bool currentlyRidden,
            string creatureName,
            Element element)
        {
            var tracker = Ensure();
            tracker.hasLastRiddenCreature = true;
            tracker.lastRiddenCreatureWorldPosition = worldPosition;
            tracker.creatureCurrentlyRidden = currentlyRidden;
            tracker.lastRiddenCreatureName = creatureName ?? string.Empty;
            tracker.lastRiddenCreatureElement = element;
            tracker.lastRiddenCreatureTraversalType =
                CreatureTraversalCatalog.GetTraversalType(creatureName);
        }

        public static void ReportCreaturePosition(
            Vector3 worldPosition,
            bool currentlyRidden,
            string creatureName,
            Element element,
            CreatureTraversalType traversalType)
        {
            var tracker = Ensure();
            tracker.hasLastRiddenCreature = true;
            tracker.lastRiddenCreatureWorldPosition = worldPosition;
            tracker.creatureCurrentlyRidden = currentlyRidden;
            tracker.lastRiddenCreatureName = creatureName ?? string.Empty;
            tracker.lastRiddenCreatureElement = element;
            tracker.lastRiddenCreatureTraversalType = traversalType;
        }

        public static void ClearLastRiddenCreature()
        {
            var tracker = Ensure();
            tracker.hasLastRiddenCreature = false;
            tracker.creatureCurrentlyRidden = false;
            tracker.lastRiddenCreatureName = string.Empty;
            tracker.lastRiddenCreatureTraversalType = CreatureTraversalType.Unknown;
        }
    }
}
