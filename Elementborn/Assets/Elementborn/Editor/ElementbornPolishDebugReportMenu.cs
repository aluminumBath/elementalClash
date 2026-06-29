#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPolishDebugReportMenu
    {
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/Diagnostics/Write v44 Polish Debug Report")]
        public static void WriteReport()
        {
            Directory.CreateDirectory(ReportDir);
            File.WriteAllText($"{ReportDir}/PolishDebugReport_v44.md",
@"# v44 Polish Debug Report

This report is generated from the v44 polish/debug pass.

## Fixed compatibility issues

```text
BaseInteractable.GetPrompt return type compatibility
JournalEntryType.Character alias
MapMarkerType.QuestObjective alias
MapMarkerType.Vendor alias
Elementborn.Core imports for generated map-marker files
NamedShipAssetGenerator duplicate local variable
```

## Recommended smoke test order

```text
1. Let Unity compile.
2. Run Elementborn → Diagnostics → Write v44 Polish Debug Report.
3. Run Elementborn → NPC Tools → Import All NPC Roster CSVs.
4. Run Elementborn → World State → Generate Capital World State Assets.
5. Run Elementborn → World State → Generate Political World Events.
6. Run Elementborn → Quest Chains → Generate Quest Chain Assets.
7. Run Elementborn → Story Encounters → Generate New Villain and Orphanage Encounters.
8. Run Elementborn → Social NPCs → Generate Social NPC Quests And Dialogue.
9. Run Elementborn → Social NPCs → Generate Social Group Assets.
10. Run Elementborn → Save → Install Narrative Runtime Save Bridges.
11. Run Elementborn → Debug → Install Story Systems Debug Dashboard.
```

## Notes

This report does not replace the Unity compiler. It documents the hardening pass and gives a clean test path.
");
            AssetDatabase.Refresh();
            Debug.Log("Wrote v44 polish/debug report.");
        }
    }
}
#endif
