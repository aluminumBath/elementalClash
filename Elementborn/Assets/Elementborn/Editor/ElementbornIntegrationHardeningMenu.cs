#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornIntegrationHardeningMenu
    {
        [MenuItem("Elementborn/Integration/Ensure Diagnostics Object")]
        public static void EnsureDiagnosticsObject()
        {
            GameObject go = GameObject.Find("Elementborn Integration Diagnostics");
            if (go == null)
            {
                go = new GameObject("Elementborn Integration Diagnostics");
            }

            if (go.GetComponent<ElementbornIntegrationDiagnostics>() == null)
            {
                go.AddComponent<ElementbornIntegrationDiagnostics>();
            }

            Selection.activeGameObject = go;
            Debug.Log("Elementborn Integration Diagnostics object is ready.");
        }

        [MenuItem("Elementborn/Integration/Write Integration Checklist")]
        public static void WriteIntegrationChecklist()
        {
            const string dir = "Assets/Elementborn/Generated/Checklists";
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "IntegrationHardeningChecklist_v22.md");
            File.WriteAllText(path,
@"# Elementborn Integration Hardening Checklist v22

- [ ] Import the latest rolling patch ZIP into the Unity project.
- [ ] Wait for Unity to finish compiling.
- [ ] Run all starter content generators needed by the test scene.
- [ ] Run Elementborn → Playable Setup → Build Full Test Scene.
- [ ] Run Elementborn → Integration → Ensure Diagnostics Object.
- [ ] Press Play and check console diagnostics.
- [ ] Confirm the Player object has health, stamina, defense, spells, interactor, and quest UI systems.
- [ ] Confirm a boss arena trigger can call BossController.StartBoss().
- [ ] Confirm quest objectives can set waypoints.
- [ ] Confirm spell cooldowns, boss UI, quest HUD, and notifications show on the test Canvas.
");
            AssetDatabase.Refresh();
            Debug.Log($"Integration checklist written to {path}");
        }
    }
}
#endif
