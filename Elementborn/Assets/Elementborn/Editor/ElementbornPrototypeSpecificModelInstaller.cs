#if UNITY_EDITOR
using UnityEditor;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeSpecificModelInstaller
    {
        [MenuItem("Elementborn/Assets/V100 Build Exact Chest And Channeler Prefabs")]
        public static void BuildExactChestAndChannelerPrefabs()
        {
            ElementbornPrototypeV102SafeVisualRecovery.DiagnoseExactImportsMenu();
        }

        [MenuItem("Elementborn/Assets/V100 Apply Channeler Visual To Player")]
        public static void ApplyChannelerVisualToPlayerMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.TryChannelerThenRobed(true);
        }

        [MenuItem("Elementborn/Assets/V100 Apply Axolotl Visual To Player")]
        public static void ApplyAxolotlVisualToPlayerMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.TryAxolotlThenRobed(true);
        }

        [MenuItem("Elementborn/Assets/V100 Apply Exact Chest Visuals")]
        public static void ApplyExactChestVisualsMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.ForceVisibleChests(true);
        }

        [MenuItem("Elementborn/Assets/V100 Restore Visible Fallback Chests")]
        public static void RestoreVisibleFallbackChestsMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.ForceVisibleChests(true);
        }

        [MenuItem("Elementborn/Assets/V100 Restore Blocky Player")]
        public static void RestoreBlockyPlayerMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.ForceRobedPlayer(true);
        }

        [MenuItem("Elementborn/Assets/V100 Apply Channeler Player And Chest Visuals")]
        public static void ApplyChannelerPlayerAndChestVisualsMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.FullSafeVisualRecovery(true);
        }

        [MenuItem("Elementborn/Assets/V100 Apply Axolotl Player And Chest Visuals")]
        public static void ApplyAxolotlPlayerAndChestVisualsMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.ForceVisibleChests(false);
            ElementbornPrototypeV102SafeVisualRecovery.TryAxolotlThenRobed(false);
            ElementbornPrototypeV102SafeVisualRecovery.RepairMovementBlockers(true);
        }

        public static void RestoreVisibleFallbackChests(bool save)
        {
            ElementbornPrototypeV102SafeVisualRecovery.ForceVisibleChests(save);
        }
    }
}
#endif
