using System;
using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Persistent session tracker for map markers belonging to the player or their discoveries.
    /// This is a code-first foundation for future per-save-slot marker persistence.
    /// </summary>
    public sealed class PlayerMapMarkerTracker : MonoBehaviour
    {
        public static PlayerMapMarkerTracker Instance { get; private set; }

        [SerializeField] private List<TrackedMapMarkerRecord> markers = new List<TrackedMapMarkerRecord>();

        public IReadOnlyList<TrackedMapMarkerRecord> Markers => markers;

        public event Action<TrackedMapMarkerRecord> MarkerChanged;
        public event Action<string> MarkerRemoved;

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

        private void Update()
        {
            CleanupExpiredMarkers();
        }

        public static PlayerMapMarkerTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(PlayerMapMarkerTracker));
            return go.AddComponent<PlayerMapMarkerTracker>();
        }

        public static IReadOnlyList<TrackedMapMarkerRecord> GetAllMarkers()
        {
            return Ensure().markers;
        }


        // v52 compatibility overload for older generated registries that passed notes as the 6th positional argument.
        public static TrackedMapMarkerRecord ReportOrUpdateMarker(
            string markerId,
            MapMarkerType markerType,
            Vector3 worldPosition,
            string label,
            bool isPersistent,
            string notes)
        {
            return ReportOrUpdateMarker(
                markerId,
                markerType,
                worldPosition,
                label,
                isPersistent,
                expiresInSeconds: -1f,
                creatureTraversalType: CreatureTraversalType.Unknown,
                contextId: "",
                notes: notes,
                hideWhileOverlappingPlayer: false);
        }

        public static TrackedMapMarkerRecord ReportOrUpdateMarker(
            string markerId,
            MapMarkerType markerType,
            Vector3 worldPosition,
            string label,
            bool isPersistent = true,
            float expiresInSeconds = -1f,
            CreatureTraversalType creatureTraversalType = CreatureTraversalType.Unknown,
            string contextId = "",
            string notes = "",
            bool hideWhileOverlappingPlayer = false)
        {
            var tracker = Ensure();

            if (string.IsNullOrWhiteSpace(markerId))
            {
                markerId = BuildMarkerId(markerType, worldPosition, label);
            }

            var record = tracker.GetOrCreate(markerId);
            record.MarkerId = markerId;
            record.MarkerType = markerType;
            record.WorldPosition = worldPosition;
            record.Label = string.IsNullOrWhiteSpace(label) ? GetDefaultLabel(markerType) : label;
            record.IsVisible = true;
            record.IsPersistent = isPersistent;
            record.IsDiscovered = true;
            record.ContextId = contextId ?? string.Empty;
            record.Notes = notes ?? string.Empty;
            record.CreatureTraversalType = creatureTraversalType;
            record.HideWhileOverlappingPlayer = hideWhileOverlappingPlayer;
            record.ExpiresAtUnscaledTime = expiresInSeconds > 0f
                ? Time.unscaledTime + expiresInSeconds
                : -1f;

            tracker.MarkerChanged?.Invoke(record);
            return record;
        }

        public static void HideMarker(string markerId)
        {
            var tracker = Ensure();
            var marker = tracker.Find(markerId);
            if (marker != null)
            {
                marker.IsVisible = false;
                tracker.MarkerChanged?.Invoke(marker);
            }
        }

        public static void ShowMarker(string markerId)
        {
            var tracker = Ensure();
            var marker = tracker.Find(markerId);
            if (marker != null)
            {
                marker.IsVisible = true;
                tracker.MarkerChanged?.Invoke(marker);
            }
        }

        public static void RemoveMarker(string markerId)
        {
            var tracker = Ensure();
            tracker.markers.RemoveAll(m => m.MarkerId == markerId);
            tracker.MarkerRemoved?.Invoke(markerId);
        }

        public static void ClearAllNonPersistentMarkers()
        {
            var tracker = Ensure();
            for (int i = tracker.markers.Count - 1; i >= 0; i--)
            {
                if (!tracker.markers[i].IsPersistent)
                {
                    string id = tracker.markers[i].MarkerId;
                    tracker.markers.RemoveAt(i);
                    tracker.MarkerRemoved?.Invoke(id);
                }
            }
        }

        public static void ClearAllMarkers()
        {
            var tracker = Ensure();
            foreach (var marker in tracker.markers)
            {
                tracker.MarkerRemoved?.Invoke(marker.MarkerId);
            }
            tracker.markers.Clear();
        }

        public static void ReportPlayer(Vector3 worldPosition)
        {
            ReportOrUpdateMarker("player", MapMarkerType.Player, worldPosition, "Player", isPersistent: false);
        }

        public static void ReportBoat(Vector3 worldPosition, bool currentlyRidden)
        {
            ReportOrUpdateMarker(
                markerId: "player_boat",
                markerType: MapMarkerType.Boat,
                worldPosition: worldPosition,
                label: currentlyRidden ? "Current Boat" : "Boat",
                isPersistent: true,
                hideWhileOverlappingPlayer: currentlyRidden);

            if (currentlyRidden)
            {
                HideMarker("player_boat");
            }
        }

        public static void ReportLastRiddenCreature(Vector3 worldPosition, string creatureName)
        {
            var traversal = CreatureTraversalCatalog.GetTraversalType(creatureName);
            string label = string.IsNullOrWhiteSpace(creatureName)
                ? CreatureTraversalCatalog.GetDefaultMarkerLabel(traversal)
                : creatureName;

            ReportOrUpdateMarker(
                markerId: "last_ridden_creature",
                markerType: MapMarkerType.LastRiddenCreature,
                worldPosition: worldPosition,
                label: label,
                isPersistent: true,
                creatureTraversalType: traversal);
        }

        public static void ReportActiveCompanion(Vector3 worldPosition, string companionName)
        {
            var traversal = CreatureTraversalCatalog.GetTraversalType(companionName);
            string label = string.IsNullOrWhiteSpace(companionName) ? "Companion" : companionName;

            ReportOrUpdateMarker(
                markerId: "active_companion_" + SafeId(label),
                markerType: MapMarkerType.ActiveCompanion,
                worldPosition: worldPosition,
                label: label,
                isPersistent: false,
                creatureTraversalType: traversal);
        }


        public static void ReportCurrentObjective(Vector3 worldPosition, string label = "Current Objective", string questId = "", string notes = "")
        {
            ReportOrUpdateMarker(
                markerId: string.IsNullOrWhiteSpace(questId) ? "current_objective" : "current_objective_" + SafeId(questId),
                markerType: MapMarkerType.CurrentObjective,
                worldPosition: worldPosition,
                label: label,
                isPersistent: true,
                contextId: questId,
                notes: notes);
        }

        public static void ReportCamp(Vector3 worldPosition, string label = "Camp")
        {
            ReportOrUpdateMarker("player_camp", MapMarkerType.Camp, worldPosition, label, isPersistent: true);
        }

        public static void ReportHomeBase(Vector3 worldPosition, string label = "Home Base")
        {
            ReportOrUpdateMarker("home_base", MapMarkerType.HomeBase, worldPosition, label, isPersistent: true);
        }

        public static void ReportStorageChest(Vector3 worldPosition, string label = "Storage Chest")
        {
            ReportOrUpdateMarker(PositionMarkerId("storage_chest", worldPosition), MapMarkerType.StorageChest, worldPosition, label, isPersistent: true);
        }

        public static void ReportCraftingStation(Vector3 worldPosition, string label = "Crafting Station")
        {
            ReportOrUpdateMarker(PositionMarkerId("crafting_station", worldPosition), MapMarkerType.CraftingStation, worldPosition, label, isPersistent: true);
        }

        public static void ReportStable(Vector3 worldPosition, string label = "Stable")
        {
            ReportOrUpdateMarker(PositionMarkerId("stable", worldPosition), MapMarkerType.Stable, worldPosition, label, isPersistent: true);
        }

        public static void ReportQuestItem(Vector3 worldPosition, string label = "Quest Item")
        {
            ReportOrUpdateMarker("quest_item_" + SafeId(label), MapMarkerType.QuestItem, worldPosition, label, isPersistent: true);
        }

        public static void ReportRareItem(Vector3 worldPosition, string label = "Rare Item")
        {
            ReportOrUpdateMarker(PositionMarkerId("rare_item", worldPosition), MapMarkerType.RareItem, worldPosition, label, isPersistent: true);
        }

        public static void ReportWeapon(Vector3 worldPosition, string label = "Weapon")
        {
            ReportOrUpdateMarker(PositionMarkerId("weapon", worldPosition), MapMarkerType.Weapon, worldPosition, label, isPersistent: true);
        }

        public static void ReportResourceNode(Vector3 worldPosition, string label = "Resource Node")
        {
            ReportOrUpdateMarker(PositionMarkerId("resource_node", worldPosition), MapMarkerType.ResourceNode, worldPosition, label, isPersistent: true);
        }

        public static void ReportTreasure(Vector3 worldPosition, string label = "Treasure")
        {
            ReportOrUpdateMarker(PositionMarkerId("treasure", worldPosition), MapMarkerType.Treasure, worldPosition, label, isPersistent: true);
        }

        public static void ReportVendorNpc(Vector3 worldPosition, string label = "Vendor")
        {
            ReportOrUpdateMarker("vendor_" + SafeId(label), MapMarkerType.VendorNpc, worldPosition, label, isPersistent: true);
        }

        public static void ReportGuideNpc(Vector3 worldPosition, string label = "Guide")
        {
            ReportOrUpdateMarker("guide_" + SafeId(label), MapMarkerType.GuideNpc, worldPosition, label, isPersistent: true);
        }

        public static void ReportTrainerNpc(Vector3 worldPosition, string label = "Trainer")
        {
            ReportOrUpdateMarker("trainer_" + SafeId(label), MapMarkerType.TrainerNpc, worldPosition, label, isPersistent: true);
        }

        public static void ReportHealerNpc(Vector3 worldPosition, string label = "Healer")
        {
            ReportOrUpdateMarker("healer_" + SafeId(label), MapMarkerType.HealerNpc, worldPosition, label, isPersistent: true);
        }

        public static void ReportQuestGiverNpc(Vector3 worldPosition, string label = "Quest Giver")
        {
            ReportOrUpdateMarker("quest_giver_" + SafeId(label), MapMarkerType.QuestGiverNpc, worldPosition, label, isPersistent: true);
        }

        public static void ReportRareEnemySighting(Vector3 worldPosition, string label = "Rare Enemy Sighting", float expiresInSeconds = 600f)
        {
            ReportOrUpdateMarker(PositionMarkerId("rare_enemy", worldPosition), MapMarkerType.RareEnemySighting, worldPosition, label, isPersistent: false, expiresInSeconds: expiresInSeconds);
        }

        public static void ReportBossLair(Vector3 worldPosition, string label = "Boss Lair")
        {
            ReportOrUpdateMarker(PositionMarkerId("boss_lair", worldPosition), MapMarkerType.BossLair, worldPosition, label, isPersistent: true);
        }

        public static void ReportEnemyCamp(Vector3 worldPosition, string label = "Enemy Camp")
        {
            ReportOrUpdateMarker(PositionMarkerId("enemy_camp", worldPosition), MapMarkerType.EnemyCamp, worldPosition, label, isPersistent: true);
        }

        public static void ReportDangerZone(Vector3 worldPosition, string label = "Danger Zone", float expiresInSeconds = -1f)
        {
            ReportOrUpdateMarker(PositionMarkerId("danger_zone", worldPosition), MapMarkerType.DangerZone, worldPosition, label, isPersistent: expiresInSeconds <= 0f, expiresInSeconds: expiresInSeconds);
        }

        public static void ReportSeaMonsterSighting(Vector3 worldPosition, string label = "Sea Monster Sighting", float expiresInSeconds = 900f)
        {
            ReportOrUpdateMarker(PositionMarkerId("sea_monster", worldPosition), MapMarkerType.SeaMonsterSighting, worldPosition, label, isPersistent: false, expiresInSeconds: expiresInSeconds);
        }

        public static void ReportDock(Vector3 worldPosition, string label = "Dock")
        {
            ReportOrUpdateMarker(PositionMarkerId("dock", worldPosition), MapMarkerType.Dock, worldPosition, label, isPersistent: true);
        }

        public static void ReportFastTravel(Vector3 worldPosition, string label = "Fast Travel")
        {
            ReportOrUpdateMarker(PositionMarkerId("fast_travel", worldPosition), MapMarkerType.FastTravel, worldPosition, label, isPersistent: true);
        }

        public static void ReportShrine(Vector3 worldPosition, string label = "Shrine")
        {
            ReportOrUpdateMarker(PositionMarkerId("shrine", worldPosition), MapMarkerType.Shrine, worldPosition, label, isPersistent: true);
        }

        public static void ReportDungeon(Vector3 worldPosition, string label = "Dungeon")
        {
            ReportOrUpdateMarker(PositionMarkerId("dungeon", worldPosition), MapMarkerType.Dungeon, worldPosition, label, isPersistent: true);
        }

        public static void ReportCave(Vector3 worldPosition, string label = "Cave")
        {
            ReportOrUpdateMarker(PositionMarkerId("cave", worldPosition), MapMarkerType.Cave, worldPosition, label, isPersistent: true);
        }

        public static void ReportPuzzle(Vector3 worldPosition, string label = "Puzzle")
        {
            ReportOrUpdateMarker(PositionMarkerId("puzzle", worldPosition), MapMarkerType.Puzzle, worldPosition, label, isPersistent: true);
        }

        public static void ReportLockedDoor(Vector3 worldPosition, string label = "Locked Door")
        {
            ReportOrUpdateMarker(PositionMarkerId("locked_door", worldPosition), MapMarkerType.LockedDoor, worldPosition, label, isPersistent: true);
        }

        public static void ReportFishingSpot(Vector3 worldPosition, string label = "Fishing Spot")
        {
            ReportOrUpdateMarker(PositionMarkerId("fishing_spot", worldPosition), MapMarkerType.FishingSpot, worldPosition, label, isPersistent: true);
        }

        public static void ReportUnderwaterRuin(Vector3 worldPosition, string label = "Underwater Ruin")
        {
            ReportOrUpdateMarker(PositionMarkerId("underwater_ruin", worldPosition), MapMarkerType.UnderwaterRuin, worldPosition, label, isPersistent: true);
        }

        public static void ReportCoralReef(Vector3 worldPosition, string label = "Coral Reef")
        {
            ReportOrUpdateMarker(PositionMarkerId("coral_reef", worldPosition), MapMarkerType.CoralReef, worldPosition, label, isPersistent: true);
        }

        public static void ReportWindCurrent(Vector3 worldPosition, string label = "Wind Current")
        {
            ReportOrUpdateMarker(PositionMarkerId("wind_current", worldPosition), MapMarkerType.WindCurrent, worldPosition, label, isPersistent: true);
        }

        public static void ReportCustomPin(string markerId, Vector3 worldPosition, string label = "Pinned Location", string notes = "")
        {
            ReportOrUpdateMarker(string.IsNullOrWhiteSpace(markerId) ? PositionMarkerId("custom_pin", worldPosition) : markerId, MapMarkerType.CustomPin, worldPosition, label, isPersistent: true, contextId: markerId, notes: notes);
        }


        public static string BuildMarkerId(MapMarkerType markerType, Vector3 worldPosition, string label)
        {
            string labelPart = string.IsNullOrWhiteSpace(label) ? GetDefaultLabel(markerType) : label;
            return PositionMarkerId(markerType.ToString() + "_" + SafeId(labelPart), worldPosition);
        }

        public static string PositionMarkerId(string prefix, Vector3 worldPosition)
        {
            return $"{SafeId(prefix)}_{Mathf.RoundToInt(worldPosition.x)}_{Mathf.RoundToInt(worldPosition.z)}";
        }

        public static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "marker";
            }

            var chars = value.Trim().ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                bool ok = char.IsLetterOrDigit(c) || c == '_' || c == '-';
                chars[i] = ok ? c : '_';
            }

            return new string(chars).Trim('_');
        }

        public static string GetDefaultLabel(MapMarkerType markerType)
        {
            return markerType switch
            {
                MapMarkerType.Player => "Player",
                MapMarkerType.Boat => "Boat",
                MapMarkerType.LastRiddenCreature => "Last Ridden Creature",
                MapMarkerType.ActiveCompanion => "Companion",
                MapMarkerType.CurrentObjective => "Current Objective",
                MapMarkerType.Camp => "Camp",
                MapMarkerType.HomeBase => "Home Base",
                MapMarkerType.StorageChest => "Storage Chest",
                MapMarkerType.CraftingStation => "Crafting Station",
                MapMarkerType.Stable => "Stable",
                MapMarkerType.QuestItem => "Quest Item",
                MapMarkerType.RareItem => "Rare Item",
                MapMarkerType.Weapon => "Weapon",
                MapMarkerType.ResourceNode => "Resource Node",
                MapMarkerType.Treasure => "Treasure",
                MapMarkerType.VendorNpc => "Vendor",
                MapMarkerType.GuideNpc => "Guide",
                MapMarkerType.TrainerNpc => "Trainer",
                MapMarkerType.HealerNpc => "Healer",
                MapMarkerType.QuestGiverNpc => "Quest Giver",
                MapMarkerType.RareEnemySighting => "Rare Enemy Sighting",
                MapMarkerType.BossLair => "Boss Lair",
                MapMarkerType.EnemyCamp => "Enemy Camp",
                MapMarkerType.DangerZone => "Danger Zone",
                MapMarkerType.SeaMonsterSighting => "Sea Monster Sighting",
                MapMarkerType.Dock => "Dock",
                MapMarkerType.FastTravel => "Fast Travel",
                MapMarkerType.Shrine => "Shrine",
                MapMarkerType.Dungeon => "Dungeon",
                MapMarkerType.Cave => "Cave",
                MapMarkerType.Puzzle => "Puzzle",
                MapMarkerType.LockedDoor => "Locked Door",
                MapMarkerType.FishingSpot => "Fishing Spot",
                MapMarkerType.UnderwaterRuin => "Underwater Ruin",
                MapMarkerType.CoralReef => "Coral Reef",
                MapMarkerType.WindCurrent => "Wind Current",
                MapMarkerType.CustomPin => "Pinned Location",
                _ => "Marker"
            };
        }

        private TrackedMapMarkerRecord GetOrCreate(string markerId)
        {
            var existing = Find(markerId);
            if (existing != null)
            {
                return existing;
            }

            var created = new TrackedMapMarkerRecord { MarkerId = markerId };
            markers.Add(created);
            return created;
        }

        private TrackedMapMarkerRecord Find(string markerId)
        {
            return markers.Find(m => m.MarkerId == markerId);
        }

        private void CleanupExpiredMarkers()
        {
            for (int i = markers.Count - 1; i >= 0; i--)
            {
                if (markers[i].IsExpired(Time.unscaledTime))
                {
                    string id = markers[i].MarkerId;
                    markers.RemoveAt(i);
                    MarkerRemoved?.Invoke(id);
                }
            }
        }
    }
}
