
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StorySystemsDebugDashboardSetupMenu
    {
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/UI";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/Debug/Install Story Systems Debug Dashboard")]
        public static void InstallDashboard()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            GameObject canvas = GameObject.Find("Elementborn Story Debug Dashboard");
            if (canvas == null)
            {
                canvas = new GameObject("Elementborn Story Debug Dashboard");
                var c = canvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                canvas.AddComponent<GraphicRaycaster>();
            }

            foreach (Transform child in canvas.transform)
            {
                Object.DestroyImmediate(child.gameObject);
            }

            StorySystemsDebugDashboard dashboard = canvas.GetComponent<StorySystemsDebugDashboard>();
            if (dashboard == null) dashboard = canvas.AddComponent<StorySystemsDebugDashboard>();
            if (canvas.GetComponent<StorySystemsDebugDashboardInput>() == null) canvas.AddComponent<StorySystemsDebugDashboardInput>();
            StorySystemsDebugDashboardActionBar actionBar = canvas.GetComponent<StorySystemsDebugDashboardActionBar>();
            if (actionBar == null) actionBar = canvas.AddComponent<StorySystemsDebugDashboardActionBar>();

            GameObject panel = Panel(canvas.transform, "Dashboard Panel", new Vector2(0.02f, 0.04f), new Vector2(0.62f, 0.92f), new Color(0.04f, 0.06f, 0.1f, 0.88f));
            GameObject textArea = Panel(panel.transform, "Text Area", new Vector2(0.02f, 0.03f), new Vector2(0.74f, 0.94f), new Color(0.09f, 0.11f, 0.16f, 0.7f));
            Text text = textArea.AddComponent<Text>();
            text.font = Elementborn.Game.ElementbornBuiltinFontUtility.GetDefaultFont();
            text.fontSize = 15;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            RectTransform textRt = text.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0.02f, 0.02f);
            textRt.anchorMax = new Vector2(0.98f, 0.98f);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            GameObject buttonColumn = Panel(panel.transform, "Action Column", new Vector2(0.79f, 0.03f), new Vector2(0.98f, 0.94f), new Color(0.1f, 0.08f, 0.12f, 0.82f));
            List<Button> buttons = new List<Button>
            {
                CreateButton(buttonColumn.transform, "StartLoopButton", "Start Loop", 0),
                CreateButton(buttonColumn.transform, "SpawnWaveButton", "Spawn Wave", 1),
                CreateButton(buttonColumn.transform, "FireHookButton", "Fire Hook", 2),
                CreateButton(buttonColumn.transform, "FireVolcanoButton", "Fire Volcano", 3),
                CreateButton(buttonColumn.transform, "RefreshButton", "Refresh", 4),
                CreateButton(buttonColumn.transform, "SaveButton", "Save Slot 0", 5),
                CreateButton(buttonColumn.transform, "LoadButton", "Load Slot 0", 6),
                CreateButton(buttonColumn.transform, "EvaluatePoliticalButton", "Evaluate Political", 7),
                CreateButton(buttonColumn.transform, "SyncCapitalsButton", "Sync Capitals", 8),
                CreateButton(buttonColumn.transform, "SocialEventButton", "Trigger Social Event", 9),
                CreateButton(buttonColumn.transform, "AdmitOrphanageButton", "Admit Demo Creature", 10),
                CreateButton(buttonColumn.transform, "WolfPackButton", "Respawn Wolf Pack", 11)
            };

            SetPrivate(dashboard, "outputText", text);
            SetPrivate(dashboard, "panelRoot", panel);
            SetPrivate(actionBar, "dashboard", dashboard);
            SetPrivate(actionBar, "startLoopButton", buttons[0]);
            SetPrivate(actionBar, "spawnWaveButton", buttons[1]);
            SetPrivate(actionBar, "fireHookButton", buttons[2]);
            SetPrivate(actionBar, "fireVolcanoButton", buttons[3]);
            SetPrivate(actionBar, "refreshButton", buttons[4]);
            SetPrivate(actionBar, "saveButton", buttons[5]);
            SetPrivate(actionBar, "loadButton", buttons[6]);
            SetPrivate(actionBar, "evaluatePoliticalButton", buttons[7]);
            SetPrivate(actionBar, "syncCapitalsButton", buttons[8]);
            SetPrivate(actionBar, "socialEventButton", buttons[9]);
            SetPrivate(actionBar, "admitOrphanageButton", buttons[10]);
            SetPrivate(actionBar, "wolfPackButton", buttons[11]);

            PrefabUtility.SaveAsPrefabAsset(canvas, $"{PrefabDir}/Elementborn_StoryDebugDashboard.prefab");

            File.WriteAllText($"{ReportDir}/StoryDebugDashboardReport.md",
@"# Story Debug Dashboard Report

Generated by v45.

## Menu

```text
Elementborn → Debug → Install Story Systems Debug Dashboard
```

## Keys

```text
F10 refresh/toggle dashboard behavior
F11 save narrative slot 0
F12 load narrative slot 0
```

## Buttons

```text
Start Loop
Spawn Wave
Fire Hook
Fire Volcano
Refresh
Save Slot 0
Load Slot 0
Evaluate Political
Sync Capitals
Trigger Social Event
Admit Demo Creature
Respawn Wolf Pack
```

## Dashboard sections

```text
capital world state
political events
quest chains
social NPCs
social groups
creature orphanage recovery
story encounters
narrative save paths
```
");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            dashboard.Refresh();
            Debug.Log("Installed Story Systems Debug Dashboard.");
        }

        private static GameObject Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        private static Button CreateButton(Transform parent, string name, string label, int row)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.26f, 0.2f, 0.28f, 0.95f);
            Button button = go.AddComponent<Button>();
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.08f, 1f);
            rect.anchorMax = new Vector2(0.92f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 54f);
            rect.anchoredPosition = new Vector2(0f, -12f - row * 64f);

            GameObject textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            Text text = textGo.AddComponent<Text>();
            text.font = Elementborn.Game.ElementbornBuiltinFontUtility.GetDefaultFont();
            text.fontSize = 15;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                EditorUtility.SetDirty(target as Object);
            }
        }
    }
}
#endif
