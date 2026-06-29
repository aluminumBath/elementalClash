#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornUiPrefabFactory
    {
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/UI";
        private const string ThemeDir = "Assets/Elementborn/Generated/UI";
        private const string CommonSpriteDir = "Assets/Elementborn/Art/UI/Common";

        [MenuItem("Elementborn/UI Prefabs/Create All Polished UI Prefabs")]
        public static void CreateAll()
        {
            ConfigureCommonSprites();
            CreateThemeAsset();
            CreatePlayableHudPrefab();
            CreateQuestLogPrefab();
            CreateBossHudPrefab();
            CreateSpellHudPrefab();
            CreateDebugHudPrefab();
            Debug.Log("Created all polished Elementborn UI prefabs.");
        }

        [MenuItem("Elementborn/UI Prefabs/Create Playable HUD Canvas Prefab")]
        public static void CreatePlayableHudPrefab()
        {
            EnsureDirs();
            ElementbornUiTheme theme = LoadOrCreateTheme();

            GameObject canvasGo = CreateCanvasRoot("Elementborn_PlayableHudCanvas");
            canvasGo.AddComponent<ElementbornHudReadmeComponent>();
            canvasGo.AddComponent<HudPanelAutoBinder>();
            var applier = canvasGo.AddComponent<ElementbornUiThemeApplier>();
            SetPrivate(applier, "theme", theme);

            GameObject questTracker = CreatePanel(canvasGo.transform, "Quest Tracker Panel", new Vector2(32, -32), new Vector2(420, 120), Anchor.TopLeft, theme.QuestPanelSprite);
            var questHud = questTracker.AddComponent<QuestTrackerHudView>();
            Text questTitle = CreateText(questTracker.transform, "Quest Title", "Tracked Quest", 24, new Vector2(18, -15), new Vector2(380, 32), Anchor.TopLeft);
            Text questObjective = CreateText(questTracker.transform, "Quest Objective", "Objective", 18, new Vector2(18, -50), new Vector2(330, 30), Anchor.TopLeft);
            Text questDistance = CreateText(questTracker.transform, "Quest Distance", "--m", 16, new Vector2(350, -50), new Vector2(60, 30), Anchor.TopLeft);
            SetPrivate(questHud, "root", questTracker);
            SetPrivate(questHud, "titleText", questTitle);
            SetPrivate(questHud, "objectiveText", questObjective);
            SetPrivate(questHud, "distanceText", questDistance);

            GameObject spellBar = CreatePanel(canvasGo.transform, "Spell HUD Panel", new Vector2(0, 28), new Vector2(520, 96), Anchor.BottomCenter, theme.SpellBarSprite);
            var loadoutView = spellBar.AddComponent<SpellLoadoutHudView>();
            SpellCooldownHudSlot[] slots = new SpellCooldownHudSlot[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject slotGo = CreatePanel(spellBar.transform, $"Spell Slot {i+1}", new Vector2(-195 + i * 130, 0), new Vector2(86, 72), Anchor.Center, theme.PanelSprite);
                slots[i] = slotGo.AddComponent<SpellCooldownHudSlot>();
                Image icon = CreateImage(slotGo.transform, "Icon", null, new Vector2(0, 0), new Vector2(48, 48), Anchor.Center);
                Image fill = CreateImage(slotGo.transform, "Cooldown Fill", theme.BossFrameSprite, new Vector2(0, 0), new Vector2(52, 52), Anchor.Center);
                Text cdText = CreateText(slotGo.transform, "Cooldown Text", "", 18, new Vector2(0, 0), new Vector2(80, 32), Anchor.Center);
                SetPrivate(slots[i], "icon", icon);
                SetPrivate(slots[i], "cooldownFill", fill);
                SetPrivate(slots[i], "cooldownText", cdText);
            }
            SetPrivate(loadoutView, "hudSlots", slots);

            GameObject castPanel = CreatePanel(canvasGo.transform, "Cast Bar Panel", new Vector2(0, 140), new Vector2(420, 54), Anchor.BottomCenter, theme.PanelSprite);
            var castBar = castPanel.AddComponent<SpellCastBarView>();
            Slider castSlider = CreateSlider(castPanel.transform, "Cast Slider", new Vector2(0, -8), new Vector2(360, 18), Anchor.Center);
            Text castLabel = CreateText(castPanel.transform, "Cast Label", "", 16, new Vector2(0, 13), new Vector2(360, 24), Anchor.Center);
            SetPrivate(castBar, "root", castPanel);
            SetPrivate(castBar, "slider", castSlider);
            SetPrivate(castBar, "label", castLabel);
            castPanel.SetActive(false);

            GameObject popupPanel = CreatePanel(canvasGo.transform, "Quest Popup Panel", new Vector2(0, -70), new Vector2(470, 110), Anchor.TopCenter, theme.PanelSprite);
            var popup = popupPanel.AddComponent<QuestObjectivePopupView>();
            Text popupTitle = CreateText(popupPanel.transform, "Popup Title", "Quest Updated", 24, new Vector2(0, -18), new Vector2(420, 32), Anchor.TopCenter);
            Text popupBody = CreateText(popupPanel.transform, "Popup Body", "", 17, new Vector2(0, -55), new Vector2(420, 36), Anchor.TopCenter);
            SetPrivate(popup, "root", popupPanel);
            SetPrivate(popup, "titleText", popupTitle);
            SetPrivate(popup, "bodyText", popupBody);
            popupPanel.SetActive(false);

            GameObject questLog = BuildQuestLogPanel(canvasGo.transform, theme);
            questLog.SetActive(false);

            var toggler = canvasGo.AddComponent<UiPanelToggleController>();
            SetPrivate(toggler, "questLogPanel", questLog);

            SavePrefab(canvasGo, $"{PrefabDir}/Elementborn_PlayableHudCanvas.prefab");
        }

        [MenuItem("Elementborn/UI Prefabs/Create Quest Log Prefab")]
        public static void CreateQuestLogPrefab()
        {
            EnsureDirs();
            ElementbornUiTheme theme = LoadOrCreateTheme();
            GameObject root = CreateCanvasRoot("Elementborn_QuestLogCanvas");
            BuildQuestLogPanel(root.transform, theme);
            SavePrefab(root, $"{PrefabDir}/Elementborn_QuestLogCanvas.prefab");
        }

        [MenuItem("Elementborn/UI Prefabs/Create Boss HUD Prefab")]
        public static void CreateBossHudPrefab()
        {
            EnsureDirs();
            ElementbornUiTheme theme = LoadOrCreateTheme();
            GameObject canvas = CreateCanvasRoot("Elementborn_BossHudCanvas");

            GameObject bossPanel = CreatePanel(canvas.transform, "Boss Health Panel", new Vector2(0, -24), new Vector2(680, 110), Anchor.TopCenter, theme.BossFrameSprite);
            var bossView = bossPanel.AddComponent<BossHealthBarView>();
            Text name = CreateText(bossPanel.transform, "Boss Name", "Boss", 26, new Vector2(0, -18), new Vector2(580, 34), Anchor.TopCenter);
            Slider slider = CreateSlider(bossPanel.transform, "Boss Health Slider", new Vector2(0, -62), new Vector2(560, 22), Anchor.TopCenter);
            Text phase = CreateText(bossPanel.transform, "Boss Phase", "Phase", 16, new Vector2(0, -84), new Vector2(260, 24), Anchor.TopCenter);
            Image icon = CreateImage(bossPanel.transform, "Boss Icon", null, new Vector2(-300, -54), new Vector2(56, 56), Anchor.TopCenter);
            SetPrivate(bossView, "root", bossPanel);
            SetPrivate(bossView, "healthSlider", slider);
            SetPrivate(bossView, "nameText", name);
            SetPrivate(bossView, "phaseText", phase);
            SetPrivate(bossView, "icon", icon);

            GameObject banner = CreatePanel(canvas.transform, "Boss Phase Banner", new Vector2(0, -150), new Vector2(520, 92), Anchor.TopCenter, theme.PanelSprite);
            var bannerView = banner.AddComponent<BossPhaseBannerView>();
            Text bannerText = CreateText(banner.transform, "Phase Banner Text", "Phase Change", 25, new Vector2(0, -26), new Vector2(440, 38), Anchor.TopCenter);
            Image bannerIcon = CreateImage(banner.transform, "Phase Icon", null, new Vector2(-220, -46), new Vector2(48, 48), Anchor.TopCenter);
            SetPrivate(bannerView, "root", banner);
            SetPrivate(bannerView, "phaseText", bannerText);
            SetPrivate(bannerView, "phaseIcon", bannerIcon);
            banner.SetActive(false);

            SavePrefab(canvas, $"{PrefabDir}/Elementborn_BossHudCanvas.prefab");
        }

        [MenuItem("Elementborn/UI Prefabs/Create Spell HUD Prefab")]
        public static void CreateSpellHudPrefab()
        {
            EnsureDirs();
            ElementbornUiTheme theme = LoadOrCreateTheme();
            GameObject canvas = CreateCanvasRoot("Elementborn_SpellHudCanvas");

            GameObject panel = CreatePanel(canvas.transform, "Spell Resource Panel", new Vector2(24, 24), new Vector2(330, 72), Anchor.BottomLeft, theme.SpellBarSprite);
            var resource = panel.AddComponent<SpellResourceBarView>();
            Slider slider = CreateSlider(panel.transform, "Resource Slider", new Vector2(0, -12), new Vector2(270, 18), Anchor.Center);
            Text label = CreateText(panel.transform, "Resource Label", "Focus", 17, new Vector2(0, 14), new Vector2(270, 26), Anchor.Center);
            SetPrivate(resource, "slider", slider);
            SetPrivate(resource, "label", label);

            SavePrefab(canvas, $"{PrefabDir}/Elementborn_SpellHudCanvas.prefab");
        }

        [MenuItem("Elementborn/UI Prefabs/Create Debug HUD Prefab")]
        public static void CreateDebugHudPrefab()
        {
            EnsureDirs();
            ElementbornUiTheme theme = LoadOrCreateTheme();
            GameObject canvas = CreateCanvasRoot("Elementborn_DebugHudCanvas");
            GameObject panel = CreatePanel(canvas.transform, "Debug Panel", new Vector2(-24, -24), new Vector2(420, 320), Anchor.TopRight, theme.PanelSprite);
            Text text = CreateText(panel.transform, "Debug Text", "Elementborn Debug", 16, new Vector2(16, -16), new Vector2(380, 280), Anchor.TopLeft);
            var generated = panel.AddComponent<GeneratedContentPresenceReport>();
            panel.AddComponent<RuntimeNullReferenceGuard>();
            SavePrefab(canvas, $"{PrefabDir}/Elementborn_DebugHudCanvas.prefab");
        }


// v59 compatibility wrapper used by CreateAll().
// Earlier versions called CreateThemeAsset(), while the factory implementation was renamed to LoadOrCreateTheme().
[MenuItem("Elementborn/UI Prefabs/Create Default UI Theme Asset")]
public static ElementbornUiTheme CreateThemeAsset()
{
    return LoadOrCreateTheme();
}

        public static ElementbornUiTheme LoadOrCreateTheme()
        {
            EnsureDirs();
            string path = $"{ThemeDir}/Elementborn_DefaultUiTheme.asset";
            ElementbornUiTheme theme = AssetDatabase.LoadAssetAtPath<ElementbornUiTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ElementbornUiTheme>();
                AssetDatabase.CreateAsset(theme, path);
            }

            var so = new SerializedObject(theme);
            so.FindProperty("panelSprite").objectReferenceValue = LoadSprite("ui_panel_blue");
            so.FindProperty("buttonSprite").objectReferenceValue = LoadSprite("ui_button_gold");
            so.FindProperty("questPanelSprite").objectReferenceValue = LoadSprite("ui_quest_panel");
            so.FindProperty("spellBarSprite").objectReferenceValue = LoadSprite("ui_spell_bar");
            so.FindProperty("bossFrameSprite").objectReferenceValue = LoadSprite("ui_boss_frame");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(theme);
            return theme;
        }

        public static void ConfigureCommonSprites()
        {
            EnsureDirs();
            string[] files =
            {
                "ui_panel_blue.png",
                "ui_panel_gold.png",
                "ui_panel_green.png",
                "ui_button_gold.png",
                "ui_quest_panel.png",
                "ui_spell_bar.png",
                "ui_boss_frame.png",
                "ui_common_preview.png"
            };

            foreach (string file in files)
            {
                string path = $"{CommonSpriteDir}/{file}";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{CommonSpriteDir}/{name}.png");
        }

        private static GameObject BuildQuestLogPanel(Transform parent, ElementbornUiTheme theme)
        {
            GameObject panel = CreatePanel(parent, "Quest Log Panel", new Vector2(0, 0), new Vector2(720, 520), Anchor.Center, theme.QuestPanelSprite);
            var log = panel.AddComponent<QuestLogPanelView>();
            Text title = CreateText(panel.transform, "Quest Log Title", "Quest Log", 28, new Vector2(0, -24), new Vector2(640, 38), Anchor.TopCenter);
            Text body = CreateText(panel.transform, "Quest Log Text", "No quests yet.", 17, new Vector2(28, -78), new Vector2(640, 390), Anchor.TopLeft);
            body.alignment = TextAnchor.UpperLeft;
            SetPrivate(log, "text", body);

            GameObject rewardPanel = CreatePanel(panel.transform, "Reward Preview Panel", new Vector2(0, 35), new Vector2(640, 72), Anchor.BottomCenter, theme.PanelSprite);
            Text rewardText = CreateText(rewardPanel.transform, "Reward Preview Text", "Rewards preview appears here.", 16, Vector2.zero, new Vector2(600, 48), Anchor.Center);
            var rewardPreview = rewardPanel.AddComponent<QuestRewardPreviewView>();
            SetPrivate(rewardPreview, "text", rewardText);
            return panel;
        }

        private static GameObject CreateCanvasRoot(string name)
        {
            GameObject go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            return go;
        }

        private enum Anchor
        {
            TopLeft,
            TopCenter,
            TopRight,
            Center,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        private static void ApplyAnchor(RectTransform rect, Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    break;
                case Anchor.TopCenter:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    break;
                case Anchor.TopRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    break;
                case Anchor.BottomLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                    rect.pivot = new Vector2(0f, 0f);
                    break;
                case Anchor.BottomCenter:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    break;
                case Anchor.BottomRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(1f, 0f);
                    break;
                default:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Anchor anchor, Sprite sprite)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            ApplyAnchor(rect, anchor);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string text, int size, Vector2 anchoredPosition, Vector2 rectSize, Anchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            ApplyAnchor(rect, anchor);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = rectSize;
            Text label = go.AddComponent<Text>();
            label.text = text;
            label.font = Elementborn.Game.ElementbornBuiltinFontUtility.GetDefaultFont();
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.96f, 0.86f, 1f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 anchoredPosition, Vector2 rectSize, Anchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            ApplyAnchor(rect, anchor);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = rectSize;
            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            return image;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition, Vector2 rectSize, Anchor anchor)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            ApplyAnchor(rootRect, anchor);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = rectSize;

            Slider slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(root.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.12f, 0.18f, 0.9f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(root.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(3f, 3f);
            fillAreaRect.offsetMax = new Vector2(-3f, -3f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(1f, 0.72f, 0.25f, 1f);

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            return slider;
        }

        private static void SavePrefab(GameObject go, string path)
        {
            EnsureDirs();
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Created UI prefab: {path}");
        }

        private static void EnsureDirs()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ThemeDir);
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
