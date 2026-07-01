#if UNITY_EDITOR
using UnityEditor;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeReachabilityAndChestVisualFixer
    {
        [MenuItem("Elementborn/Visuals/Repair Prototype Reachability")]
        public static void RepairPrototypeReachabilityMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.RepairMovementBlockers(true);
        }

        [MenuItem("Elementborn/Assets/Build Specific Treasure Chest Prefab")]
        public static void BuildSpecificTreasureChestPrefabMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.DiagnoseExactImportsMenu();
        }

        [MenuItem("Elementborn/Assets/Apply Specific Treasure Chest Visuals To All Chests")]
        public static void ApplySpecificTreasureChestVisualsMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.ForceVisibleChests(true);
        }

        [MenuItem("Elementborn/Visuals/Repair Reachability And Chest Visuals")]
        public static void RepairReachabilityAndChestVisualsMenu()
        {
            ElementbornPrototypeV102SafeVisualRecovery.ForceVisibleChests(false);
            ElementbornPrototypeV102SafeVisualRecovery.RepairMovementBlockers(true);
        }

        public static void RepairReachability(bool save)
        {
            ElementbornPrototypeV102SafeVisualRecovery.RepairMovementBlockers(save);
        }

        public static void ApplyTreasureChestVisualsToAllChests(bool save)
        {
            ElementbornPrototypeV102SafeVisualRecovery.ForceVisibleChests(save);
        }
    }
}
#endif
