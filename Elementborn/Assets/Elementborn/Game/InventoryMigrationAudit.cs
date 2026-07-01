using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Read-only migration aid (step 2). Reports which legacy <see cref="ItemCatalog"/> item ids do NOT yet have a
    /// matching <see cref="InventoryItemDefinition"/>, so we know what to author (or bridge) before crafting and
    /// equipment move onto <see cref="PlayerInventoryTracker"/>. This changes nothing.
    ///
    /// Run the definitive project-wide scan from the Unity menu "Elementborn/Inventory/Audit Migration Gaps", or
    /// call <see cref="RunAndLog"/> at runtime to audit against whatever definitions are currently loaded.
    /// </summary>
    public static class InventoryMigrationAudit
    {
        public struct Result
        {
            public List<string> MissingIds;    // legacy ids with no InventoryItemDefinition
            public List<string> CoveredIds;    // legacy ids that already have one
            public List<string> OrphanDefIds;  // definition ids that are not a legacy catalog item
            public int LegacyCount;
            public int DefinitionCount;
        }

        /// <summary>Pure comparison of the legacy catalog against a supplied set of definition ids. No Unity lookups,
        /// so this is unit-testable.</summary>
        public static Result Compute(ICollection<string> definitionIds)
        {
            var defs = new HashSet<string>();
            if (definitionIds != null)
            {
                foreach (var id in definitionIds)
                {
                    if (!string.IsNullOrWhiteSpace(id)) defs.Add(id);
                }
            }

            var result = new Result
            {
                MissingIds = new List<string>(),
                CoveredIds = new List<string>(),
                OrphanDefIds = new List<string>(),
                DefinitionCount = defs.Count
            };

            var legacy = ItemCatalog.All();
            result.LegacyCount = legacy.Count;

            var legacyIds = new HashSet<string>();
            foreach (var item in legacy)
            {
                legacyIds.Add(item.Id);
                if (defs.Contains(item.Id)) result.CoveredIds.Add(item.Id);
                else result.MissingIds.Add(item.Id);
            }

            foreach (var id in defs)
            {
                if (!legacyIds.Contains(id)) result.OrphanDefIds.Add(id);
            }

            result.MissingIds.Sort();
            result.CoveredIds.Sort();
            result.OrphanDefIds.Sort();
            return result;
        }

        /// <summary>Ids of every InventoryItemDefinition currently loaded in memory. Runtime fallback for when the
        /// editor's project-wide asset scan is unavailable.</summary>
        public static List<string> CollectLoadedDefinitionIds()
        {
            var ids = new List<string>();
            foreach (var def in Resources.FindObjectsOfTypeAll<InventoryItemDefinition>())
            {
                if (def != null && !string.IsNullOrWhiteSpace(def.ItemId)) ids.Add(def.ItemId);
            }
            return ids;
        }

        /// <summary>Formats a human-readable, multi-line summary of an audit result.</summary>
        public static string Summarize(Result r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Inventory migration audit] legacy items: " + r.LegacyCount
                          + "   definitions seen: " + r.DefinitionCount);
            sb.AppendLine("Missing a definition (" + r.MissingIds.Count + "):");
            if (r.MissingIds.Count == 0)
            {
                sb.AppendLine("  (none - every legacy item has a definition)");
            }
            else
            {
                foreach (var id in r.MissingIds)
                {
                    var name = ItemCatalog.Get(id)?.Name ?? id;
                    sb.AppendLine("  - " + id + "  (\"" + name + "\")");
                }
            }
            if (r.CoveredIds.Count > 0)
            {
                sb.AppendLine("Already covered (" + r.CoveredIds.Count + "): " + string.Join(", ", r.CoveredIds));
            }
            if (r.OrphanDefIds.Count > 0)
            {
                sb.AppendLine("Definitions with no legacy item (" + r.OrphanDefIds.Count + "): "
                              + string.Join(", ", r.OrphanDefIds));
            }
            return sb.ToString();
        }

        /// <summary>Runtime convenience: audit against the loaded definitions and log the summary.</summary>
        public static string RunAndLog()
        {
            var text = Summarize(Compute(CollectLoadedDefinitionIds()));
            Debug.Log(text);
            return text;
        }
    }
}
