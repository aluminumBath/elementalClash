using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    /// <summary>Editor entry point for the read-only inventory migration audit (step 2). Scans every
    /// InventoryItemDefinition asset in the project, compares it against the legacy ItemCatalog, and logs the gaps.
    /// Nothing is created or modified.</summary>
    public static class InventoryMigrationAuditMenu
    {
        [MenuItem("Elementborn/Inventory/Audit Migration Gaps")]
        public static void Audit()
        {
            var ids = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:InventoryItemDefinition"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<InventoryItemDefinition>(path);
                if (def != null && !string.IsNullOrWhiteSpace(def.ItemId)) ids.Add(def.ItemId);
            }

            var result = InventoryMigrationAudit.Compute(ids);
            Debug.Log(InventoryMigrationAudit.Summarize(result));
        }
    }
}
