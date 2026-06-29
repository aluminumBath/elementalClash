#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class AdminWristUiSetupMenu
    {
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/Admin";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/Admin UI/Generate Left Wrist Admin UI Prefab")]
        public static void GeneratePrefab()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            GameObject root = BuildCanvasRoot();
            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/Elementborn_LeftWristAdminPanel.prefab");
            Object.DestroyImmediate(root);

            WriteReport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated left wrist admin UI prefab.");
        }

        [MenuItem("Elementborn/Admin UI/Install Left Wrist Admin UI In Open Scene")]
        public static void InstallInOpenScene()
        {
            GeneratePrefab();

            GameObject existing = GameObject.Find("Elementborn Left Wrist Admin Panel");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Elementborn_LeftWristAdminPanel.prefab");
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate Elementborn_LeftWristAdminPanel.prefab");
                return;
            }

            instance.name = "Elementborn Left Wrist Admin Panel";
            instance.transform.position = new Vector3(-0.42f, 1.25f, 0.85f);
            instance.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            Undo.RegisterCreatedObjectUndo(instance, "Install left wrist admin UI");

            GameObject systems = GameObject.Find("Elementborn Admin Systems");
            if (systems == null)
            {
                systems = new GameObject("Elementborn Admin Systems");
                Undo.RegisterCreatedObjectUndo(systems, "Create admin systems");
            }

            if (systems.GetComponent<AdminRuntimeCommandRouter>() == null) systems.AddComponent<AdminRuntimeCommandRouter>();
            if (systems.GetComponent<AdminActionCatalog>() == null) systems.AddComponent<AdminActionCatalog>();
            if (systems.GetComponent<AdminActionExecutor>() == null) systems.AddComponent<AdminActionExecutor>();

            EditorUtility.SetDirty(instance);
            EditorUtility.SetDirty(systems);
            Debug.Log("Installed left wrist admin UI in open scene.");
        }

        private static GameObject BuildCanvasRoot()
        {
            GameObject root = new GameObject("Elementborn_LeftWristAdminPanel");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 12f;
            root.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = root.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(520f, 720f);
            root.transform.localScale = Vector3.one * 0.0016f;

            LeftWristAdminPanelController controller = root.AddComponent<LeftWristAdminPanelController>();
            AdminWristPanelView view = root.AddComponent<AdminWristPanelView>();

            GameObject panel = Panel(root.transform, "Admin Wrist Panel", new Vector2(0f, 0f), new Vector2(520f, 720f), new Color(0.035f, 0.045f, 0.065f, 0.94f));
            SetPrivate(controller, "panelRoot", panel);
            SetPrivate(controller, "visibleOnStart", false);

            Text title = Label(panel.transform, "Title", "LEFT WRIST ADMIN / CHEAT PANEL", 22, new Vector2(18f, -18f), new Vector2(484f, 34f), TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.82f, 0.42f, 1f);

            Text categoryLabel = Label(panel.transform, "Category Label", "Category", 14, new Vector2(20f, -68f), new Vector2(120f, 24f), TextAnchor.MiddleLeft);
            Dropdown category = Dropdown(panel.transform, "Category Dropdown", new Vector2(150f, -64f), new Vector2(340f, 32f));

            Text actionLabel = Label(panel.transform, "Action Label", "Action", 14, new Vector2(20f, -108f), new Vector2(120f, 24f), TextAnchor.MiddleLeft);
            Dropdown action = Dropdown(panel.transform, "Action Dropdown", new Vector2(150f, -104f), new Vector2(340f, 32f));

            Text description = Label(panel.transform, "Description", "Choose a category and action.", 12, new Vector2(20f, -146f), new Vector2(470f, 54f), TextAnchor.UpperLeft);

            List<AdminWristFieldRow> rows = new List<AdminWristFieldRow>();
            for (int i = 0; i < 5; i++)
            {
                rows.Add(CreateFieldRow(panel.transform, i, new Vector2(20f, -214f - i * 58f)));
            }

            Button apply = Button(panel.transform, "Apply Button", "APPLY CHANGE", new Vector2(20f, -510f), new Vector2(225f, 38f), new Color(0.22f, 0.34f, 0.22f, 0.96f));
            Button runRaw = Button(panel.transform, "Run Raw Command Button", "RUN RAW", new Vector2(265f, -510f), new Vector2(225f, 38f), new Color(0.24f, 0.24f, 0.34f, 0.96f));

            Text cheatTitle = Label(panel.transform, "Cheat Title", "Cheat Code Section", 16, new Vector2(20f, -558f), new Vector2(220f, 24f), TextAnchor.MiddleLeft);
            cheatTitle.color = new Color(1f, 0.82f, 0.42f, 1f);

            Dropdown cheatDropdown = Dropdown(panel.transform, "Cheat Dropdown", new Vector2(20f, -590f), new Vector2(300f, 32f));
            Button applyCheat = Button(panel.transform, "Apply Cheat Button", "APPLY CHEAT", new Vector2(332f, -590f), new Vector2(158f, 32f), new Color(0.36f, 0.2f, 0.36f, 0.96f));

            InputField rawCommand = Input(panel.transform, "Raw Command Input", "raw command, e.g. eventdir.summary", new Vector2(20f, -632f), new Vector2(470f, 32f));

            Text status = Label(panel.transform, "Status", "F8 toggles panel. Use dropdowns or raw command input.", 12, new Vector2(20f, -676f), new Vector2(470f, 36f), TextAnchor.UpperLeft);

            SetPrivate(view, "categoryDropdown", category);
            SetPrivate(view, "actionDropdown", action);
            SetPrivate(view, "descriptionText", description);
            SetPrivate(view, "statusText", status);
            SetPrivate(view, "applyButton", apply);
            SetPrivate(view, "fieldRows", rows);
            SetPrivate(view, "cheatDropdown", cheatDropdown);
            SetPrivate(view, "applyCheatButton", applyCheat);
            SetPrivate(view, "rawCommandInput", rawCommand);
            SetPrivate(view, "runRawCommandButton", runRaw);

            return root;
        }

        private static AdminWristFieldRow CreateFieldRow(Transform parent, int index, Vector2 position)
        {
            GameObject root = new GameObject("Field Row " + index);
            root.transform.SetParent(parent, false);
            RectTransform rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(470f, 48f);

            Text label = Label(root.transform, "Label", "Field", 12, new Vector2(0f, 0f), new Vector2(140f, 24f), TextAnchor.MiddleLeft);
            InputField input = Input(root.transform, "Input", "", new Vector2(150f, 0f), new Vector2(320f, 30f));
            Dropdown dropdown = Dropdown(root.transform, "Dropdown", new Vector2(150f, 0f), new Vector2(320f, 30f));
            Toggle toggle = Toggle(root.transform, "Toggle", new Vector2(150f, -2f), new Vector2(120f, 28f));

            return new AdminWristFieldRow
            {
                Root = root,
                Label = label,
                Input = input,
                Dropdown = dropdown,
                Toggle = toggle
            };
        }

        private static GameObject Panel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return go;
        }

        private static Text Label(Transform parent, string name, string text, int size, Vector2 position, Vector2 dimensions, TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text label = go.AddComponent<Text>();
            label.font = ElementbornBuiltinFontUtility.GetDefaultFont();
            label.fontSize = size;
            label.color = Color.white;
            label.text = text;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            return label;
        }

        private static InputField Input(Transform parent, string name, string placeholder, Vector2 position, Vector2 size)
        {
            GameObject root = Panel(parent, name, position, size, new Color(0.12f, 0.14f, 0.18f, 0.95f));
            InputField input = root.AddComponent<InputField>();

            Text text = Label(root.transform, "Text", "", 13, new Vector2(8f, -4f), new Vector2(size.x - 16f, size.y - 8f), TextAnchor.MiddleLeft);
            Text holder = Label(root.transform, "Placeholder", placeholder, 13, new Vector2(8f, -4f), new Vector2(size.x - 16f, size.y - 8f), TextAnchor.MiddleLeft);
            holder.color = new Color(0.65f, 0.68f, 0.75f, 0.75f);

            input.textComponent = text;
            input.placeholder = holder;
            return input;
        }

        private static Dropdown Dropdown(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject root = Panel(parent, name, position, size, new Color(0.1f, 0.12f, 0.18f, 0.95f));
            Dropdown dropdown = root.AddComponent<Dropdown>();

            Text label = Label(root.transform, "Label", "Option", 13, new Vector2(8f, -4f), new Vector2(size.x - 36f, size.y - 8f), TextAnchor.MiddleLeft);

            GameObject template = Panel(root.transform, "Template", new Vector2(0f, -size.y - 2f), new Vector2(size.x, 165f), new Color(0.07f, 0.08f, 0.12f, 0.98f));
            template.SetActive(false);

            GameObject viewport = Panel(template.transform, "Viewport", new Vector2(0f, 0f), new Vector2(size.x, 165f), new Color(0.07f, 0.08f, 0.12f, 0.25f));
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 30f);
            contentRect.anchoredPosition = Vector2.zero;

            GameObject item = Panel(content.transform, "Item", new Vector2(0f, 0f), new Vector2(size.x, 30f), new Color(0.13f, 0.14f, 0.2f, 0.98f));
            Toggle toggle = item.AddComponent<Toggle>();
            Text itemText = Label(item.transform, "Item Label", "Option", 13, new Vector2(8f, -3f), new Vector2(size.x - 16f, 24f), TextAnchor.MiddleLeft);
            toggle.targetGraphic = item.GetComponent<Image>();

            ScrollRect scroll = template.AddComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;

            dropdown.captionText = label;
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.itemText = itemText;
            return dropdown;
        }

        private static Toggle Toggle(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            RectTransform rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Toggle toggle = root.AddComponent<Toggle>();
            GameObject check = Panel(root.transform, "Check", new Vector2(0f, 0f), new Vector2(24f, 24f), new Color(0.25f, 0.42f, 0.25f, 1f));
            toggle.graphic = check.GetComponent<Image>();
            Label(root.transform, "Toggle Label", "Enabled", 12, new Vector2(34f, 0f), new Vector2(100f, 24f), TextAnchor.MiddleLeft);
            return toggle;
        }

        private static Button Button(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color)
        {
            GameObject root = Panel(parent, name, position, size, color);
            Button button = root.AddComponent<Button>();
            Label(root.transform, "Label", label, 13, new Vector2(0f, 0f), size, TextAnchor.MiddleCenter);
            return button;
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                if (target is Object unityObject)
                {
                    EditorUtility.SetDirty(unityObject);
                }
            }
        }

        private static void WriteReport()
        {
            Directory.CreateDirectory(ReportDir);
            File.WriteAllText($"{ReportDir}/AdminWristUiReport.md",
@"# Left Wrist Admin UI Report

Generated by v47.

## Menu

```text
Elementborn → Admin UI → Generate Left Wrist Admin UI Prefab
Elementborn → Admin UI → Install Left Wrist Admin UI In Open Scene
```

## Interaction

```text
F8 toggles the panel in non-VR playtests.
Assign LeftWristAdminPanelController.leftWristAnchor to the player's left wrist/hand transform for VR.
If no wrist transform is assigned, the panel follows the main camera with a left-side offset.
```

## Sections

```text
Category dropdown
Action dropdown
Dynamic form fields
Apply Change button
Cheat Code dropdown
Apply Cheat button
Raw admin command input
Run Raw button
```

## Covered admin categories

```text
GameplayLoop
SaveLoad
CapitalWorldState
PoliticalEvents
QuestChains
FireCapital
SocialGroups
CreatureOrphanage
WolfPack
StoryEncounters
CheatCodes
RawCommand
```
");
        }
    }
}
#endif
