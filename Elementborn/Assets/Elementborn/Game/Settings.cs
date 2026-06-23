using System;
using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Player-tweakable options, persisted as JSON next to the save files. Flat and JsonUtility-friendly.
    /// Consumers read <see cref="SettingsStore.Current"/>; the settings menu writes it and calls
    /// <see cref="SettingsStore.SaveAndApply"/>, which fires <see cref="SettingsStore.Changed"/>.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.6f;
        public float sfxVolume = 1f;
        public float mouseSensitivity = 1f;
        public bool invertY = false;
        public bool comfortVignette = true;
        public float fieldOfView = 70f;

        /// <summary>Clamp every field into a sane range and return this (chainable).</summary>
        public SettingsData Clamped()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            mouseSensitivity = Mathf.Clamp(mouseSensitivity, 0.1f, 5f);
            fieldOfView = Mathf.Clamp(fieldOfView, 50f, 100f);
            return this;
        }

        public SettingsData Copy() => (SettingsData)MemberwiseClone();
    }

    /// <summary>Loads/saves <see cref="SettingsData"/> and broadcasts changes so live systems can react.</summary>
    public static class SettingsStore
    {
        private const string FileName = "elementborn_settings.json";
        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private static SettingsData _current;

        /// <summary>Raised after settings change and are saved, so audio/rig/vignette can re-read.</summary>
        public static event Action Changed;

        public static SettingsData Current
        {
            get { if (_current == null) _current = Load(); return _current; }
        }

        public static SettingsData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var d = JsonUtility.FromJson<SettingsData>(File.ReadAllText(FilePath));
                    if (d != null) return d.Clamped();
                }
            }
            catch (Exception e) { Debug.LogWarning($"[Elementborn] Settings load failed: {e.Message}"); }
            return new SettingsData();
        }

        public static void Save()
        {
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(Current.Clamped(), true)); }
            catch (Exception e) { Debug.LogWarning($"[Elementborn] Settings save failed: {e.Message}"); }
        }

        /// <summary>Notify listeners of a change without persisting (use during live drags).</summary>
        public static void Apply() => Changed?.Invoke();

        /// <summary>Persist and notify (use when a control is released / committed).</summary>
        public static void SaveAndApply() { Save(); Changed?.Invoke(); }
    }
}
