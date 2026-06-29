using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Null-safe player attunement HUD.
    /// This replacement intentionally avoids assuming serialized UI references exist during
    /// PlayMode tests or generated scene setup.
    /// </summary>
    public sealed class PlayerAttunementHud : MonoBehaviour
    {
        [SerializeField] private Text attunementLabel;
        [SerializeField] private Image attunementSwatch;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool autoCreateUi = true;
        [SerializeField] private string fallbackText = "Attunement: Unbound";

        private string lastDisplayValue;

        private void Awake()
        {
            EnsureUi();
            RefreshIfChanged(ReadCurrentAttunementValue());
        }

        private void OnEnable()
        {
            EnsureUi();
            RefreshIfChanged(ReadCurrentAttunementValue());
        }

        private void Update()
        {
            RefreshIfChanged(ReadCurrentAttunementValue());
        }

        public void RefreshNow()
        {
            lastDisplayValue = null;
            RefreshIfChanged(ReadCurrentAttunementValue());
        }

        public void RefreshIfChanged(object mine)
        {
            string displayValue = NormalizeAttunement(mine);
            if (string.Equals(displayValue, lastDisplayValue, StringComparison.Ordinal))
            {
                return;
            }

            lastDisplayValue = displayValue;
            EnsureUi();

            if (attunementLabel != null)
            {
                attunementLabel.text = string.IsNullOrWhiteSpace(displayValue)
                    ? fallbackText
                    : "Attunement: " + displayValue;
            }

            if (attunementSwatch != null)
            {
                attunementSwatch.color = ColorFor(displayValue);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void EnsureUi()
        {
            if (!autoCreateUi)
            {
                return;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            }

            if (attunementLabel == null)
            {
                attunementLabel = GetComponentInChildren<Text>(true);
            }

            if (attunementSwatch == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                foreach (Image image in images)
                {
                    if (image != null && image.name.IndexOf("Swatch", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        attunementSwatch = image;
                        break;
                    }
                }
            }

            if (attunementLabel != null && canvasGroup != null)
            {
                return;
            }

            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Attunement HUD Canvas", typeof(RectTransform));
                canvasGo.transform.SetParent(transform, false);
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = canvas.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (attunementSwatch == null)
            {
                GameObject swatchGo = new GameObject("Attunement Swatch", typeof(RectTransform));
                swatchGo.transform.SetParent(canvas.transform, false);
                attunementSwatch = swatchGo.AddComponent<Image>();
                RectTransform swatchRect = swatchGo.GetComponent<RectTransform>();
                swatchRect.anchorMin = new Vector2(0f, 1f);
                swatchRect.anchorMax = new Vector2(0f, 1f);
                swatchRect.pivot = new Vector2(0f, 1f);
                swatchRect.sizeDelta = new Vector2(28f, 28f);
                swatchRect.anchoredPosition = new Vector2(18f, -18f);
            }

            if (attunementLabel == null)
            {
                GameObject labelGo = new GameObject("Attunement Label", typeof(RectTransform));
                labelGo.transform.SetParent(canvas.transform, false);
                attunementLabel = labelGo.AddComponent<Text>();
                ElementbornBuiltinFontUtility.ApplyDefaultFont(attunementLabel);
                attunementLabel.fontSize = 18;
                attunementLabel.alignment = TextAnchor.MiddleLeft;
                attunementLabel.color = Color.white;
                attunementLabel.text = fallbackText;

                RectTransform labelRect = labelGo.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0f, 1f);
                labelRect.anchorMax = new Vector2(0f, 1f);
                labelRect.pivot = new Vector2(0f, 1f);
                labelRect.sizeDelta = new Vector2(340f, 34f);
                labelRect.anchoredPosition = new Vector2(54f, -14f);
            }
        }

        private static object ReadCurrentAttunementValue()
        {
            try
            {
                MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour == null)
                    {
                        continue;
                    }

                    Type type = behaviour.GetType();
                    if (type == typeof(PlayerAttunementHud))
                    {
                        continue;
                    }

                    string typeName = type.Name;
                    if (typeName.IndexOf("Attunement", StringComparison.OrdinalIgnoreCase) < 0 &&
                        typeName.IndexOf("Channel", StringComparison.OrdinalIgnoreCase) < 0 &&
                        typeName.IndexOf("Element", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    object value = ReadMember(behaviour, "CurrentAttunement")
                        ?? ReadMember(behaviour, "Attunement")
                        ?? ReadMember(behaviour, "CurrentElement")
                        ?? ReadMember(behaviour, "Element")
                        ?? ReadMember(behaviour, "ActiveAttunement")
                        ?? ReadMember(behaviour, "ActiveElement");

                    if (value != null)
                    {
                        return value;
                    }
                }
            }
            catch
            {
                // PlayMode tests should never fail because the optional HUD could not discover a tracker.
            }

            return null;
        }

        private static object ReadMember(object target, string memberName)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = target.GetType();

            try
            {
                PropertyInfo property = type.GetProperty(memberName, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(target, null);
                }

                FieldInfo field = type.GetField(memberName, flags);
                if (field != null)
                {
                    return field.GetValue(target);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static string NormalizeAttunement(object value)
        {
            if (value == null)
            {
                return "Unbound";
            }

            string text = value.ToString();
            return string.IsNullOrWhiteSpace(text) ? "Unbound" : text;
        }

        private static Color ColorFor(string attunement)
        {
            string key = (attunement ?? string.Empty).Trim().ToLowerInvariant();
            switch (key)
            {
                case "fire":
                    return new Color(1f, 0.34f, 0.12f, 0.95f);
                case "water":
                    return new Color(0.15f, 0.45f, 1f, 0.95f);
                case "air":
                case "wind":
                    return new Color(0.72f, 0.95f, 1f, 0.95f);
                case "earth":
                    return new Color(0.45f, 0.32f, 0.18f, 0.95f);
                case "blood":
                    return new Color(0.62f, 0.03f, 0.08f, 0.95f);
                case "ice":
                    return new Color(0.72f, 0.92f, 1f, 0.95f);
                case "plant":
                case "plants":
                    return new Color(0.25f, 0.72f, 0.24f, 0.95f);
                case "steam":
                    return new Color(0.72f, 0.72f, 0.76f, 0.95f);
                case "metal":
                    return new Color(0.74f, 0.70f, 0.62f, 0.95f);
                default:
                    return new Color(0.42f, 0.36f, 0.72f, 0.85f);
            }
        }
    }
}
