using System.IO;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Lightweight save-slot bridge for map markers.
    ///
    /// This is intentionally independent from the existing SaveSystem so it can be
    /// wired in without refactoring. Later, you can call SaveSlot/LoadSlot from the
    /// main save-slot flow and/or merge the data into your existing save payload.
    /// </summary>
    public sealed class MapMarkerSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot = 0;
        [SerializeField] private bool saveOnlyPersistentMarkers = true;
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
            var tracker = PlayerMapMarkerTracker.Ensure();
            var saveFile = new MapMarkerSaveFile
            {
                SlotIndex = Mathf.Max(0, slot)
            };

            foreach (var marker in tracker.Markers)
            {
                if (marker == null)
                {
                    continue;
                }

                if (saveOnlyPersistentMarkers && !marker.IsPersistent)
                {
                    continue;
                }

                saveFile.Markers.Add(MapMarkerSaveRecord.FromRuntime(marker));
            }

            string json = JsonUtility.ToJson(saveFile, prettyPrint: true);
            File.WriteAllText(GetPath(slot), json);
            Debug.Log($"Saved {saveFile.Markers.Count} map marker(s) for slot {slot}.");
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                Debug.Log($"No map marker save file exists for slot {slot}: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            var saveFile = JsonUtility.FromJson<MapMarkerSaveFile>(json);
            if (saveFile == null)
            {
                Debug.LogWarning($"Failed to parse map marker save file: {path}");
                return;
            }

            PlayerMapMarkerTracker.ClearAllMarkers();

            foreach (var marker in saveFile.Markers)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    marker.MarkerId,
                    marker.MarkerType,
                    marker.ToPosition(),
                    marker.Label,
                    marker.IsPersistent,
                    creatureTraversalType: marker.CreatureTraversalType,
                    contextId: marker.ContextId,
                    notes: marker.Notes,
                    hideWhileOverlappingPlayer: marker.HideWhileOverlappingPlayer);

                if (!marker.IsVisible)
                {
                    PlayerMapMarkerTracker.HideMarker(marker.MarkerId);
                }
            }

            Debug.Log($"Loaded {saveFile.Markers.Count} map marker(s) for slot {slot}.");
        }

        public void DeleteSlot(int slot)
        {
            string path = GetPath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "map_markers");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_map_markers.json");
        }
    }
}
