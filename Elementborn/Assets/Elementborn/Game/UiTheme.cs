using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace Elementborn.Game
{
    /// <summary>A label that is backed by TextMeshPro when available, or legacy uGUI Text as a fallback.</summary>
    public sealed class UiLabel
    {
        private readonly TMP_Text _tmp;
        private readonly Text _legacy;
        public UiLabel(TMP_Text t) { _tmp = t; }
        public UiLabel(Text t) { _legacy = t; }
        public string text { get => _tmp ? _tmp.text : _legacy.text; set { if (_tmp) _tmp.text = value; else _legacy.text = value; } }
        public Graphic Graphic => _tmp ? (Graphic)_tmp : _legacy;
        public RectTransform Rect => Graphic.rectTransform;
        public void SetColor(Color c) { if (_tmp) _tmp.color = c; else _legacy.color = c; }
        public void Stretch() { var r = Rect; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero; }
    }

    /// <summary>A single/multi-line text field backed by TextMeshPro when available, or legacy uGUI InputField.</summary>
    public sealed class UiInput
    {
        private readonly TMP_InputField _tmp;
        private readonly InputField _legacy;
        public UiInput(TMP_InputField t) { _tmp = t; }
        public UiInput(InputField t) { _legacy = t; }
        public string text { get => _tmp ? _tmp.text : _legacy.text; set { if (_tmp) _tmp.text = value; else _legacy.text = value; } }
        public RectTransform Rect => _tmp ? (RectTransform)_tmp.transform : (RectTransform)_legacy.transform;
    }

    /// <summary>
    /// One place for the look of the code-built UI: shared colours, optional 9-slice sprites (loaded from
    /// <c>Resources/ElementbornUI/</c> if present — see docs/GENERATED_ART.md — otherwise flat colours), a
    /// TextMeshPro label factory with a legacy-Text fallback, and factories for panels, buttons, sliders and
    /// toggles. New screens build through this; existing screens can migrate to it incrementally.
    /// </summary>
    public static class UiTheme
    {
        public static readonly Color PanelColor = new Color(0.08f, 0.09f, 0.12f, 0.92f);
        public static readonly Color ButtonColor = new Color(0.25f, 0.6f, 0.95f, 1f);
        public static readonly Color TextColor = new Color(0.92f, 0.94f, 0.97f, 1f);
        public static readonly Color TrackColor = new Color(0.20f, 0.22f, 0.28f, 1f);

        private static bool _spritesProbed;
        private static bool _spritesPresent;

        /// <summary>Loads a UI sprite from Resources/ElementbornUI/ (cached null if none are installed).</summary>
        public static Sprite Load(string name)
        {
            if (!_spritesProbed)
            {
                _spritesProbed = true;
                _spritesPresent = Resources.Load<Sprite>("ElementbornUI/panel") != null;
            }
            return _spritesPresent ? Resources.Load<Sprite>("ElementbornUI/" + name) : null;
        }

        public static Canvas Canvas(string name, int sortOrder = 0)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            EnsureEventSystem();
            return c;
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        public static RectTransform Rect(GameObject go) => (RectTransform)go.transform;

        public static UiLabel Label(Transform parent, string text, int size, Color? color = null,
            TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var font = TMP_Settings.defaultFontAsset;
            if (font != null)
            {
                var t = go.AddComponent<TextMeshProUGUI>();
                t.text = text; t.fontSize = size; t.color = color ?? TextColor;
                t.alignment = ToTmp(anchor); t.raycastTarget = false;
                return new UiLabel(t);
            }
            var legacy = go.AddComponent<Text>();
            legacy.text = text; legacy.fontSize = size; legacy.color = color ?? TextColor;
            legacy.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            legacy.alignment = anchor; legacy.raycastTarget = false;
            return new UiLabel(legacy);
        }

        public static Image Panel(Transform parent, Color? color = null, string sprite = "panel")
        {
            var go = new GameObject("Panel", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color ?? PanelColor;
            var sp = Load(sprite);
            if (sp != null) { img.sprite = sp; img.type = Image.Type.Sliced; }
            return img;
        }

        public static Button Button(Transform parent, string label, Action onClick, int width = 360, int height = 56)
        {
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = ButtonColor;
            var sp = Load("btn_normal");
            if (sp != null) { img.sprite = sp; img.type = Image.Type.Sliced; }

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            Rect(go).sizeDelta = new Vector2(width, height);

            var lbl = Label(go.transform, label, Mathf.RoundToInt(height * 0.4f), Color.white, TextAnchor.MiddleCenter);
            lbl.Stretch();

            btn.onClick.AddListener(() => { AudioController.Instance?.Click(); onClick?.Invoke(); });
            return btn;
        }

        /// <summary>A labelled fill-bar slider (label left, track right).</summary>
        public static Slider Slider(Transform parent, string label, float value, float min, float max,
            Action<float> onChange)
        {
            var row = new GameObject(label + "Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowLe = row.AddComponent<LayoutElement>(); rowLe.minHeight = 44; rowLe.preferredHeight = 44;

            var lbl = Label(row.transform, label, 22, TextColor);
            var lr = lbl.Rect; lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(0.45f, 1f);
            lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;

            var sgo = new GameObject("Slider", typeof(RectTransform));
            sgo.transform.SetParent(row.transform, false);
            var sr = Rect(sgo); sr.anchorMin = new Vector2(0.47f, 0.25f); sr.anchorMax = new Vector2(1f, 0.75f);
            sr.offsetMin = Vector2.zero; sr.offsetMax = Vector2.zero;
            var bg = sgo.AddComponent<Image>(); bg.color = TrackColor;

            var slider = sgo.AddComponent<Slider>();
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = min; slider.maxValue = max; slider.value = value;

            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(sgo.transform, false);
            var fr = Rect(fillGo); fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
            fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;
            var fill = fillGo.AddComponent<Image>(); fill.color = ButtonColor;

            slider.fillRect = fr;
            slider.targetGraphic = fill;
            slider.onValueChanged.AddListener(v => onChange?.Invoke(v));
            return slider;
        }

        public static Toggle Toggle(Transform parent, string label, bool value, Action<bool> onChange)
        {
            var row = new GameObject(label + "Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowLe = row.AddComponent<LayoutElement>(); rowLe.minHeight = 44; rowLe.preferredHeight = 44;

            var lbl = Label(row.transform, label, 22, TextColor);
            var lr = lbl.Rect; lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(0.8f, 1f);
            lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;

            var box = new GameObject("Toggle", typeof(RectTransform));
            box.transform.SetParent(row.transform, false);
            var br = Rect(box); br.anchorMin = new Vector2(0.84f, 0.15f); br.anchorMax = new Vector2(0.99f, 0.85f);
            br.offsetMin = Vector2.zero; br.offsetMax = Vector2.zero;
            var bg = box.AddComponent<Image>(); bg.color = TrackColor;

            var toggle = box.AddComponent<Toggle>();
            toggle.targetGraphic = bg;

            var check = new GameObject("Check", typeof(RectTransform));
            check.transform.SetParent(box.transform, false);
            var cr = Rect(check); cr.anchorMin = new Vector2(0.15f, 0.15f); cr.anchorMax = new Vector2(0.85f, 0.85f);
            cr.offsetMin = Vector2.zero; cr.offsetMax = Vector2.zero;
            var checkImg = check.AddComponent<Image>(); checkImg.color = ButtonColor;
            toggle.graphic = checkImg;

            toggle.isOn = value;
            toggle.onValueChanged.AddListener(v => { AudioController.Instance?.Click(); onChange?.Invoke(v); });
            return toggle;
        }

        /// <summary>A text input row (TMP when available, legacy InputField otherwise). Read/write via .text.
        /// Built inactive then activated so the input control binds its text components before it first enables.</summary>
        public static UiInput Input(Transform parent, string placeholder, int height = 44, bool multiline = false)
        {
            var go = new GameObject("Input", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var bg = go.AddComponent<Image>();
            bg.color = TrackColor;
            var sp = Load("input");
            if (sp != null) { bg.sprite = sp; bg.type = Image.Type.Sliced; }
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height; le.preferredHeight = height;

            int fontSize = Mathf.RoundToInt(height * 0.42f);
            var phColor = new Color(0.62f, 0.64f, 0.70f, 1f);

            if (TMP_Settings.defaultFontAsset != null)
            {
                var area = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
                area.transform.SetParent(go.transform, false);
                var ar = (RectTransform)area.transform;
                ar.anchorMin = Vector2.zero; ar.anchorMax = Vector2.one;
                ar.offsetMin = new Vector2(10, 6); ar.offsetMax = new Vector2(-10, -6);

                var ph = Label(area.transform, placeholder, fontSize, phColor); ph.Stretch();
                var txt = Label(area.transform, "", fontSize, TextColor); txt.Stretch();

                var input = go.AddComponent<TMP_InputField>();
                input.textViewport = ar;
                input.textComponent = (TMP_Text)txt.Graphic;
                input.placeholder = ph.Graphic;
                input.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
                go.SetActive(true);
                return new UiInput(input);
            }

            var lph = Label(go.transform, placeholder, fontSize, phColor); lph.Stretch();
            var ltxt = Label(go.transform, "", fontSize, TextColor); ltxt.Stretch();
            var legacy = go.AddComponent<InputField>();
            legacy.textComponent = (Text)ltxt.Graphic;
            legacy.placeholder = lph.Graphic;
            legacy.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            go.SetActive(true);
            return new UiInput(legacy);
        }

        private static TextAlignmentOptions ToTmp(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }
    }
}
