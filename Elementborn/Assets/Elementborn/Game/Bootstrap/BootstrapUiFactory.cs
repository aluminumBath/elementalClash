using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public static class BootstrapUiFactory
    {
        public static Canvas BuildDefaultCanvas(string canvasName = "Elementborn Test UI Canvas")
        {
            Canvas existing = ElementbornFindUtility.FindFirst<Canvas>();
            if (existing != null && existing.name == canvasName)
            {
                return existing;
            }

            GameObject canvasObject = new GameObject(canvasName);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            if (ElementbornFindUtility.FindFirst<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            BuildQuestHud(canvas.transform);
            BuildQuestPopup(canvas.transform);
            BuildSpellHud(canvas.transform);
            BuildBossHud(canvas.transform);
            BuildNotificationFeed(canvas.transform);
            return canvas;
        }

        private static void BuildQuestHud(Transform parent)
        {
            GameObject panel = Panel(parent, "Quest HUD", new Vector2(20f, -20f), new Vector2(420f, 110f));
            Text title = Text(panel.transform, "QuestTitle", "Quest", 18, new Vector2(10f, -10f), new Vector2(390f, 28f));
            Text obj = Text(panel.transform, "Objective", "Objective", 14, new Vector2(10f, -42f), new Vector2(390f, 24f));
            Text dist = Text(panel.transform, "Distance", "", 14, new Vector2(10f, -70f), new Vector2(160f, 24f));
            var view = panel.AddComponent<QuestTrackerHudView>();
            Set(view, "root", panel);
            Set(view, "titleText", title);
            Set(view, "objectiveText", obj);
            Set(view, "distanceText", dist);
        }

        private static void BuildQuestPopup(Transform parent)
        {
            GameObject panel = Panel(parent, "Quest Popup", new Vector2(420f, -20f), new Vector2(360f, 96f));
            Text title = Text(panel.transform, "PopupTitle", "Quest Updated", 18, new Vector2(10f, -8f), new Vector2(330f, 28f));
            Text body = Text(panel.transform, "PopupBody", "", 14, new Vector2(10f, -42f), new Vector2(330f, 40f));
            var view = panel.AddComponent<QuestObjectivePopupView>();
            Set(view, "root", panel);
            Set(view, "titleText", title);
            Set(view, "bodyText", body);
            panel.SetActive(false);
        }

        private static void BuildSpellHud(Transform parent)
        {
            GameObject panel = Panel(parent, "Spell HUD", new Vector2(20f, 130f), new Vector2(480f, 90f), anchorBottom: true);
            Text resource = Text(panel.transform, "SpellResource", "Focus", 14, new Vector2(10f, -8f), new Vector2(220f, 24f));
            var resourceView = panel.AddComponent<SpellResourceBarView>();
            Set(resourceView, "label", resource);
        }

        private static void BuildBossHud(Transform parent)
        {
            GameObject panel = Panel(parent, "Boss HUD", new Vector2(240f, -20f), new Vector2(560f, 84f), centerTop: true);
            Text name = Text(panel.transform, "BossName", "Boss", 20, new Vector2(10f, -8f), new Vector2(520f, 26f));
            Text phase = Text(panel.transform, "BossPhase", "Phase", 14, new Vector2(10f, -36f), new Vector2(200f, 22f));
            Slider slider = Slider(panel.transform, "BossHealth", new Vector2(10f, -60f), new Vector2(520f, 16f));
            var view = panel.AddComponent<BossHealthBarView>();
            Set(view, "root", panel);
            Set(view, "healthSlider", slider);
            Set(view, "nameText", name);
            Set(view, "phaseText", phase);
            panel.SetActive(false);
        }

        private static void BuildNotificationFeed(Transform parent)
        {
            GameObject panel = Panel(parent, "Notification Feed", new Vector2(-360f, -20f), new Vector2(340f, 160f), anchorRight: true);
            Text text = Text(panel.transform, "Notifications", "Notifications", 13, new Vector2(10f, -10f), new Vector2(310f, 130f));
            var view = panel.AddComponent<NotificationFeedView>();
            Set(view, "text", text);
        }

        private static GameObject Panel(Transform parent, string name, Vector2 anchoredPos, Vector2 size, bool anchorBottom = false, bool centerTop = false, bool anchorRight = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.05f, 0.08f, 0.12f, 0.72f);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            if (anchorBottom)
            {
                rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(0f, 0f); rt.pivot = new Vector2(0f, 0f);
            }
            else if (centerTop)
            {
                rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
            }
            else if (anchorRight)
            {
                rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(1f, 1f);
            }
            else
            {
                rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f);
            }
            rt.anchoredPosition = anchoredPos;
            return go;
        }

        private static Text Text(Transform parent, string name, string value, int size, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.text = value;
            text.font = Elementborn.Game.ElementbornBuiltinFontUtility.GetDefaultFont();
            text.fontSize = size;
            text.color = Color.white;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return text;
        }

        private static Slider Slider(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Slider slider = go.AddComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return slider;
        }

        private static void Set(object target, string fieldName, object value)
        {
            if (target == null) return;
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
