using System;
using System.IO;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Loads JSON mod files at startup and feeds their content into the registries. Two folders are scanned:
    /// <c>StreamingAssets/Mods</c> (mods shipped with the game) and <c>&lt;persistentDataPath&gt;/Mods</c> (mods a
    /// player drops in). Each <c>*.json</c> file is parsed independently and a bad file is logged and skipped, so
    /// one broken mod can't take down the rest. Called once from <see cref="GameFlowController"/> at boot.
    /// </summary>
    public static class ModLoader
    {
        public static int FactionsLoaded { get; private set; }
        public static int EnemiesLoaded { get; private set; }

        public static string[] ModFolders => new[]
        {
            Path.Combine(Application.streamingAssetsPath, "Mods"),
            Path.Combine(Application.persistentDataPath, "Mods"),
        };

        public static void LoadAll()
        {
            FactionsLoaded = 0;
            EnemiesLoaded = 0;
            foreach (var dir in ModFolders) LoadFrom(dir);
            if (FactionsLoaded > 0 || EnemiesLoaded > 0)
                Debug.Log($"[Mods] loaded {FactionsLoaded} faction(s), {EnemiesLoaded} enemy type(s) from mods.");
        }

        private static void LoadFrom(string dir)
        {
            try
            {
                if (!Directory.Exists(dir)) return;
                foreach (var file in Directory.GetFiles(dir, "*.json"))
                {
                    try
                    {
                        var parsed = ModContent.Parse(File.ReadAllText(file));
                        foreach (var faction in parsed.Factions) { FactionRegistry.Register(faction); FactionsLoaded++; }
                        foreach (var enemy in parsed.Enemies) { EnemyRegistry.Register(enemy); EnemiesLoaded++; }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[Mods] skipped '{Path.GetFileName(file)}': {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Mods] couldn't read '{dir}': {e.Message}");
            }
        }
    }
}
