using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The title screen — the game's front door, shown by <see cref="GameFlowController"/> before any
    /// stage runs. Its own world-space canvas (so it works in VR and on flat/desktop from one path), a gradient
    /// backdrop, a centred wordmark over a four-element accent bar, and a prioritised action list: when a saved
    /// journey exists <b>Continue</b> is the primary action and shows a one-line summary of that save; otherwise
    /// <b>New Game</b> leads. Settings/Save Slots/How to Play/Credits sit below, and Quit (with a confirm) is
    /// visually muted. Everything is built through <see cref="UiTheme"/>, so any installed <c>ElementbornUI</c>
    /// sprites, the project TMP font, and the UI click sounds are used automatically; with none installed it
    /// falls back to flat colours. The owning <see cref="GameFlowController"/> tears this down (destroying its
    /// child canvas with it) the moment New Game or Continue is chosen.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private System.Action _onNewGame, _onContinue, _onSettings, _onQuit;
        private bool _hasSave;
        private Canvas _canvas;
        private CanvasGroup _group;
        private GameObject _confirm; // the quit-confirm overlay while it's up (null otherwise)

        // Palette (code-drawn until art ships; ElementColor supplies the four accent hues).
        private static readonly Color BgTop      = new Color(0.063f, 0.075f, 0.122f);
        private static readonly Color BgBottom   = new Color(0.024f, 0.027f, 0.047f);
        private static readonly Color CardColor  = new Color(0.08f, 0.09f, 0.13f, 0.72f);
        private static readonly Color Primary    = new Color(0.18f, 0.50f, 0.90f);
        private static readonly Color Secondary  = new Color(0.11f, 0.15f, 0.25f);
        private static readonly Color QuitColor  = new Color(0.16f, 0.10f, 0.11f);
        private static readonly Color Subtitle   = new Color(0.62f, 0.70f, 0.78f);
        private static readonly Color SubLabel   = new Color(0.81f, 0.89f, 0.98f);
        private static readonly Color FooterText = new Color(0.36f, 0.42f, 0.49f);
        private static readonly Color QuitText   = new Color(0.83f, 0.62f, 0.64f);

        /// <summary>Build and show the menu. <paramref name="hasSave"/> gates Continue and picks the primary action.</summary>
        public void Show(bool hasSave, System.Action onNewGame, System.Action onContinue,
                         System.Action onSettings, System.Action onQuit)
        {
            _hasSave = hasSave;
            _onNewGame = onNewGame;
            _onContinue = onContinue;
            _onSettings = onSettings;
            _onQuit = onQuit;
            Build();
        }

        private void Update()
        {
            // The menu owns the cursor while it's up (the settings overlay relocks it on close).
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_group != null && _group.alpha < 1f)
                _group.alpha = Mathf.MoveTowards(_group.alpha, 1f, Time.unscaledDeltaTime / 0.30f);

            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                if (_confirm != null) { Destroy(_confirm); _confirm = null; }
                else ShowQuitConfirm();
            }
        }

        private void OnEnable() { Localization.LocaleChanged += OnLocaleChanged; }
        private void OnDisable() { Localization.LocaleChanged -= OnLocaleChanged; }

        private void OnLocaleChanged()
        {
            if (_canvas == null) return; // not built yet
            if (_confirm != null) { Destroy(_confirm); _confirm = null; }
            Destroy(_canvas.gameObject);
            Build(); // re-render the whole menu in the new language
        }

        private void Build()
        {
            Localization.Ensure(); // guarantee the string table exists (even in the rescue scene) so T() localizes
            _canvas = UiTheme.Canvas("MainMenuCanvas", 220);
            _canvas.gameObject.AddComponent<VrCanvasAdapter>(); // world-space in VR; no-op on flat/desktop
            var mmToken = _canvas.gameObject.AddComponent<UiGateToken>(); // modal gate
            mmToken.exclusive = false; // the title screen hosts its own sub-overlays (How to Play/Credits/Quit) — don't auto-close it
            _group = _canvas.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f; // fade in via Update
            _canvas.transform.SetParent(transform, false);      // destroying this GameObject cleans up the canvas

            var full = MakeRect("Root", _canvas.transform);
            full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one;
            full.offsetMin = Vector2.zero; full.offsetMax = Vector2.zero;

            BuildBackdrop(full);

            var col = MakeRect("Center", full);
            col.anchorMin = col.anchorMax = new Vector2(0.5f, 0.5f);
            col.pivot = new Vector2(0.5f, 0.5f);
            col.sizeDelta = new Vector2(600, 820);
            col.anchoredPosition = Vector2.zero;

            // Wordmark.
            var title = UiTheme.Label(col, "ELEMENTBORN", 60, new Color(0.92f, 0.95f, 0.98f), TextAnchor.MiddleCenter);
            var tr = title.Rect;
            tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(0, 78); tr.anchoredPosition = new Vector2(0, -6);
            if (title.Graphic is TMP_Text tmp) { tmp.characterSpacing = 12f; tmp.fontStyle = FontStyles.Bold; }

            var sub = UiTheme.Label(col, Localization.T("menu.subtitle"), 18, Subtitle, TextAnchor.MiddleCenter);
            var sr = sub.Rect;
            sr.anchorMin = new Vector2(0f, 1f); sr.anchorMax = new Vector2(1f, 1f); sr.pivot = new Vector2(0.5f, 1f);
            sr.sizeDelta = new Vector2(0, 26); sr.anchoredPosition = new Vector2(0, -92);

            BuildAccentBar(col, -128);

            // Card holding the action stack.
            var card = MakeRect("Card", col);
            card.anchorMin = new Vector2(0.5f, 1f); card.anchorMax = new Vector2(0.5f, 1f); card.pivot = new Vector2(0.5f, 1f);
            card.sizeDelta = new Vector2(560, 600); card.anchoredPosition = new Vector2(0, -156);
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = CardColor;
            var cardSprite = UiTheme.Load("panel");
            if (cardSprite != null) { cardImg.sprite = cardSprite; cardImg.type = Image.Type.Sliced; }

            var stack = MakeRect("Stack", card);
            stack.anchorMin = Vector2.zero; stack.anchorMax = Vector2.one;
            stack.offsetMin = new Vector2(30, 26); stack.offsetMax = new Vector2(-30, -26);
            var v = stack.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 10; v.childControlWidth = true; v.childForceExpandWidth = true;
            v.childControlHeight = false; v.childForceExpandHeight = false; v.childAlignment = TextAnchor.UpperCenter;

            Button primaryBtn;
            if (_hasSave)
            {
                primaryBtn = MakeButton(stack, Localization.T("menu.continue"), Primary, Color.white, () => _onContinue?.Invoke());
                AddSubLabel(stack, ContinueSummary());
                MakeButton(stack, Localization.T("menu.newGame"), Secondary, Color.white, () => _onNewGame?.Invoke());
            }
            else
            {
                primaryBtn = MakeButton(stack, Localization.T("menu.newGame"), Primary, Color.white, () => _onNewGame?.Invoke());
                var cont = MakeButton(stack, Localization.T("menu.continue"), Secondary, Color.white, () => _onContinue?.Invoke());
                cont.interactable = false;
                AddSubLabel(stack, Localization.T("menu.noSave"));
            }

            MakeButton(stack, Localization.T("menu.saveSlots"), Secondary, Color.white, () => SaveSlotController.EnsureInstance().Show());
            MakeButton(stack, Localization.T("menu.settings"), Secondary, Color.white, () => _onSettings?.Invoke());
            MakeButton(stack, Localization.T("menu.howToPlay"), Secondary, Color.white, ShowHowToPlay);
            MakeButton(stack, Localization.T("menu.credits"), Secondary, Color.white, ShowCredits);
            MakeButton(stack, Localization.T("menu.quit"), QuitColor, QuitText, ShowQuitConfirm, 52);

            // Footer: build + author credit.
            MakeFooter(full, "v" + GameVersion(), TextAnchor.LowerRight, new Vector2(1f, 0f), new Vector2(-18, 12));
            MakeFooter(full, Localization.T("menu.designBy"), TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(18, 12));

            // Keyboard / controller navigation starts on the primary action.
            if (EventSystem.current != null && primaryBtn != null)
                EventSystem.current.SetSelectedGameObject(primaryBtn.gameObject);

            AudioController.Instance?.Confirm();
        }

        private void BuildBackdrop(RectTransform parent)
        {
            var go = new GameObject("Backdrop", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var raw = go.AddComponent<RawImage>();
            var bg = UiTheme.Load("menu_bg"); // use a shipped backdrop sprite if one is installed
            if (bg != null) { raw.texture = bg.texture; }
            else { raw.texture = VerticalGradient(BgTop, BgBottom); raw.color = Color.white; }
        }

        private void BuildAccentBar(Transform parent, float y)
        {
            var bar = MakeRect("AccentBar", parent);
            bar.anchorMin = new Vector2(0.5f, 1f); bar.anchorMax = new Vector2(0.5f, 1f); bar.pivot = new Vector2(0.5f, 1f);
            bar.sizeDelta = new Vector2(300, 6); bar.anchoredPosition = new Vector2(0, y);
            Element[] order = { Element.Fire, Element.Water, Element.Earth, Element.Air };
            for (int i = 0; i < order.Length; i++)
            {
                var seg = MakeRect("Seg", bar);
                float x0 = i / (float)order.Length, x1 = (i + 1) / (float)order.Length;
                seg.anchorMin = new Vector2(x0, 0f); seg.anchorMax = new Vector2(x1, 1f);
                seg.offsetMin = Vector2.zero; seg.offsetMax = Vector2.zero;
                seg.gameObject.AddComponent<Image>().color = ElementColor.For(order[i]);
            }
        }

        private Button MakeButton(Transform parent, string label, Color baseColor, Color textColor,
                                  System.Action onClick, int height = 60)
        {
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = Color.white; // the ColorBlock supplies the visible tint, so hover/press can brighten/darken
            var sp = UiTheme.Load("btn_normal");
            if (sp != null) { img.sprite = sp; img.type = Image.Type.Sliced; }

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = baseColor;
            cb.highlightedColor = Shift(baseColor, 0.12f);
            cb.pressedColor = Shift(baseColor, -0.12f);
            cb.selectedColor = Shift(baseColor, 0.08f);
            cb.disabledColor = new Color(0.18f, 0.18f, 0.22f, 1f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;

            var le = go.AddComponent<LayoutElement>(); le.minHeight = height; le.preferredHeight = height;
            // The parent VerticalLayoutGroup leaves child height uncontrolled, so set it explicitly (width is expanded).
            ((RectTransform)go.transform).sizeDelta = new Vector2(0, height);

            var lbl = UiTheme.Label(go.transform, label, Mathf.RoundToInt(height * 0.34f), textColor, TextAnchor.MiddleCenter);
            lbl.Stretch();

            btn.onClick.AddListener(() => { AudioController.Instance?.Click(); onClick?.Invoke(); });
            return btn;
        }

        private void AddSubLabel(Transform parent, string text)
        {
            var lbl = UiTheme.Label(parent, text, 15, SubLabel, TextAnchor.MiddleCenter);
            var le = lbl.Graphic.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 20; le.preferredHeight = 20;
        }

        private UiLabel MakeFooter(Transform parent, string text, TextAnchor anchor, Vector2 corner, Vector2 offset)
        {
            var lbl = UiTheme.Label(parent, text, 14, FooterText, anchor);
            var r = lbl.Rect;
            r.anchorMin = r.anchorMax = corner; r.pivot = corner;
            r.sizeDelta = new Vector2(300, 22); r.anchoredPosition = offset;
            return lbl;
        }

        private string ContinueSummary()
        {
            var d = SaveSystem.Load();
            if (d == null) return "";
            string who =
                d.isConfluence ? Localization.T("menu.confluence") :
                !string.IsNullOrEmpty(d.playerElement) ? d.playerElement + (d.loadoutSubArts != null && d.loadoutSubArts.Count > 0 ? " (sub-art)" : "") :
                (!string.IsNullOrEmpty(d.loadoutWeapon) && d.loadoutWeapon != "None") ? d.loadoutWeapon + " user" :
                Localization.T("menu.channeler");
            string when = d.savedUnixSeconds > 0
                ? DateTimeOffset.FromUnixTimeSeconds(d.savedUnixSeconds).LocalDateTime.ToString("yyyy-MM-dd HH:mm")
                : "saved";
            return who + " · Lv " + Mathf.Max(1, d.level) + " · " + when;
        }

        private static string GameVersion()
        {
            string v = Application.version;
            return string.IsNullOrEmpty(v) ? "0.2.0" : v;
        }

        private void ShowQuitConfirm()
        {
            if (_confirm != null) return;
            Canvas cv = null;
            var p = OverlayUi.Panel("QuitConfirmCanvas", Localization.T("menu.quitConfirmTitle"), 240, new Vector2(460, 250),
                () => { if (cv != null) { Destroy(cv.gameObject); if (_confirm == cv.gameObject) _confirm = null; } });
            cv = p.canvas;
            _confirm = cv.gameObject;
            OverlayUi.Body(p.content, Localization.T("menu.quitConfirmBody"), 18);
            OverlayUi.Body(p.content, " ", 6);
            MakeButton(p.content, Localization.T("menu.quitDesktop"), QuitColor, QuitText, () => _onQuit?.Invoke(), 54);
            MakeButton(p.content, Localization.T("menu.back"), Secondary, Color.white, () => { if (cv != null) { Destroy(cv.gameObject); _confirm = null; } }, 54);
            AudioController.Instance?.Confirm();
        }

        private void ShowHowToPlay() => ShowOverlay("HowToPlayCanvas", Localization.T("menu.howToPlay"), c =>
        {
            OverlayUi.Body(c, Localization.T("howto.intro"), 18);
            OverlayUi.Header(c, Localization.T("howto.controls"));
            OverlayUi.Body(c, Localization.T("howto.move"), 16);
            OverlayUi.Body(c, Localization.T("howto.attack"), 16);
            OverlayUi.Body(c, Localization.T("howto.panels1"), 16);
            OverlayUi.Body(c, Localization.T("howto.panels2"), 16);
            OverlayUi.Body(c, Localization.T("howto.close"), 16);
        });

        private void ShowCredits() => ShowOverlay("CreditsCanvas", Localization.T("menu.credits"), c =>
        {
            OverlayUi.Header(c, "ELEMENTBORN");
            OverlayUi.Body(c, Localization.T("credits.tagline"), 18);
            OverlayUi.Body(c, Localization.T("credits.by"), 16);
            OverlayUi.Body(c, Localization.T("credits.tech"), 16);
            OverlayUi.Body(c, Localization.T("credits.thanks"), 16);
        });

        // A stacked overlay with its own Close button; closing destroys just this overlay's canvas.
        private void ShowOverlay(string canvasName, string title, System.Action<Transform> fill)
        {
            Canvas cv = null;
            var p = OverlayUi.Panel(canvasName, title, 230, new Vector2(640, 720), () => { if (cv != null) Destroy(cv.gameObject); });
            cv = p.canvas;
            fill(p.content);
            AudioController.Instance?.Confirm();
        }

        // ---- small builders ----
        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Color Shift(Color c, float amt) =>
            new Color(Mathf.Clamp01(c.r + amt), Mathf.Clamp01(c.g + amt), Mathf.Clamp01(c.b + amt), c.a);

        private static Texture2D VerticalGradient(Color top, Color bottom)
        {
            const int h = 64;
            var tex = new Texture2D(1, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < h; y++)
                tex.SetPixel(0, y, Color.Lerp(bottom, top, y / (float)(h - 1)));
            tex.Apply();
            return tex;
        }
    }
}
