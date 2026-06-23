using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The on-screen heads-up display: a currency readout (top-left, polled from the player's wallet), a
    /// contextual interaction prompt (bottom-centre), and a transient toast. The prompt can be a plain string
    /// or an action+verb pair; for the latter it shows the matching control glyph (image when the glyph set is
    /// imported, otherwise a device/brand-aware text token) beside the verb, and updates live as the player
    /// switches between keyboard/mouse and a gamepad. Singleton; builds its own canvas via <see cref="UiTheme"/>.
    /// </summary>
    public sealed class GameHud : MonoBehaviour
    {
        public static GameHud Instance { get; private set; }

        [SerializeField] private float toastSeconds = 2.5f;

        private UiLabel _currency;
        private UiLabel _toast;
        private float _toastTimer;

        // prompt (glyph + text row)
        private UiLabel _prompt;
        private Image _promptGlyph;
        private InputAction _promptAction;
        private string _promptVerb = "";
        private string _promptRaw = "";
        private Sprite _shownSprite;
        private string _shownText;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start() => Build();

        private void Update()
        {
            if (_currency != null && PlayerInventory.Instance != null)
                _currency.text = Format(PlayerInventory.Instance.Wallet);

            RenderPrompt();

            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                if (_toastTimer <= 0f && _toast != null) _toast.text = "";
            }
        }

        /// <summary>Show a plain prompt string (no glyph), or clear it with null/empty.</summary>
        public void SetPrompt(string text)
        {
            _promptAction = null;
            _promptRaw = text ?? "";
        }

        /// <summary>Show a glyph-prefixed prompt for an action, e.g. action=Interact, verb="Ride".</summary>
        public void SetPrompt(InputAction action, string verb)
        {
            _promptAction = action;
            _promptVerb = verb ?? "";
        }

        /// <summary>Flash a brief message in the centre of the screen.</summary>
        public void Toast(string text)
        {
            if (_toast == null) return;
            _toast.text = text ?? "";
            _toastTimer = toastSeconds;
        }

        private void RenderPrompt()
        {
            if (_prompt == null) return;

            if (_promptAction != null)
            {
                Sprite sp = ControlGlyphs.Sprite(_promptAction);
                if (sp != null) { SetGlyph(sp); SetText(_promptVerb); }
                else { SetGlyph(null); SetText(ControlGlyphs.Prompt(_promptAction, _promptVerb)); }
            }
            else
            {
                SetGlyph(null);
                SetText(_promptRaw);
            }
        }

        private void SetGlyph(Sprite sp)
        {
            if (sp == _shownSprite) return;
            _shownSprite = sp;
            if (sp != null) { _promptGlyph.sprite = sp; _promptGlyph.gameObject.SetActive(true); }
            else _promptGlyph.gameObject.SetActive(false);
        }

        private void SetText(string t)
        {
            if (t == _shownText) return;
            _shownText = t;
            _prompt.text = t;
        }

        private static string Format(Wallet w) =>
            $"Dia {w.CountOf(Currency.Diamond)}   Sap {w.CountOf(Currency.Sapphire)}   " +
            $"Eme {w.CountOf(Currency.Emerald)}   Rub {w.CountOf(Currency.Ruby)}   Sil {w.CountOf(Currency.Silver)}";

        private void Build()
        {
            var canvas = UiTheme.Canvas("GameHud", sortOrder: 10);
            canvas.transform.SetParent(transform, false);

            _currency = Place(UiTheme.Label(canvas.transform, "", 24, Color.white, TextAnchor.UpperLeft),
                new Vector2(0, 1), new Vector2(24, -20), new Vector2(640, 40));

            _toast = Place(UiTheme.Label(canvas.transform, "", 30, new Color(0.72f, 1f, 0.8f), TextAnchor.MiddleCenter),
                new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(820, 48));

            BuildPromptRow(canvas.transform);
        }

        private void BuildPromptRow(Transform parent)
        {
            var row = new GameObject("Prompt", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            row.transform.SetParent(parent, false);
            var rt = (RectTransform)row.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 80);

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 10;
            hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            var fitter = row.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var glyphGo = new GameObject("Glyph", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            glyphGo.transform.SetParent(row.transform, false);
            _promptGlyph = glyphGo.GetComponent<Image>();
            _promptGlyph.preserveAspect = true;
            _promptGlyph.raycastTarget = false;
            var le = glyphGo.GetComponent<LayoutElement>();
            le.preferredWidth = 34; le.preferredHeight = 34;
            glyphGo.SetActive(false);

            _prompt = UiTheme.Label(row.transform, "", 26, new Color(1f, 0.95f, 0.7f), TextAnchor.MiddleCenter);
        }

        private static UiLabel Place(UiLabel lbl, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = lbl.Rect;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return lbl;
        }
    }
}
