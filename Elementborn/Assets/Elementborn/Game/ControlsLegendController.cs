using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// A read-only "Controls" page (toggle with F1, or open from the rebind menu) listing every action with
    /// its current glyph for the active device and gamepad brand: the imported sprite when available, else a
    /// text token. It rebuilds when you switch between keyboard/mouse and a gamepad (or swap pad brands), so
    /// the legend always matches what the prompts show. Suspends the player while open. Edit bindings via the
    /// linked rebind menu.
    /// </summary>
    public sealed class ControlsLegendController : MonoBehaviour
    {
        public static ControlsLegendController Instance { get; private set; }

        [SerializeField] private Key toggleKey = Key.F1;

        private Canvas _canvas;
        private GameObject _root;
        private Transform _content;
        private bool _open;
        private bool _lastPad;
        private GamepadBrand _lastBrand;
        private readonly List<(Behaviour beh, bool wasEnabled)> _suspended = new List<(Behaviour, bool)>();

        public static ControlsLegendController EnsureInstance()
        {
            if (Instance != null) return Instance;
            return new GameObject("ControlsLegendController").AddComponent<ControlsLegendController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            var k = Keyboard.current;
            if (k != null && k[toggleKey].wasPressedThisFrame) Toggle();

            if (_open && (ControlGlyphs.UsingGamepad != _lastPad || ControlGlyphs.Brand != _lastBrand))
                Populate();
        }

        public void Toggle() { if (_open) Hide(); else Show(); }

        public void Show()
        {
            if (_root == null) Build();
            _root.SetActive(true);
            _open = true;
            Populate();
            SuspendPlayer();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
            _open = false;
            RestorePlayer();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("ControlsLegendCanvas", sortOrder: 205);
            _canvas.transform.SetParent(transform, false);

            _root = new GameObject("LegendRoot", typeof(RectTransform));
            _root.transform.SetParent(_canvas.transform, false);
            var rootRt = (RectTransform)_root.transform;
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;

            var dim = UiTheme.Panel(_root.transform, new Color(0f, 0f, 0f, 0.55f), "overlay_dim");
            var dr = dim.rectTransform; dr.anchorMin = Vector2.zero; dr.anchorMax = Vector2.one;
            dr.offsetMin = Vector2.zero; dr.offsetMax = Vector2.zero;

            var panel = UiTheme.Panel(_root.transform);
            var pr = panel.rectTransform;
            pr.sizeDelta = new Vector2(760, 640);
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.anchoredPosition = Vector2.zero;

            var title = UiTheme.Label(panel.transform, "Controls", 34, Color.white, TextAnchor.UpperCenter);
            var tr = title.Rect; tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f); tr.sizeDelta = new Vector2(-40, 44); tr.anchoredPosition = new Vector2(0, -16);

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(panel.transform, false);
            _content = contentGo.transform;
            var crt = (RectTransform)contentGo.transform;
            crt.anchorMin = new Vector2(0.05f, 0.13f); crt.anchorMax = new Vector2(0.95f, 0.85f);
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

            var edit = UiTheme.Button(panel.transform, "Edit bindings…",
                () => { Hide(); RebindController.EnsureInstance().Show(); }, 220, 48);
            var er = (RectTransform)edit.transform;
            er.anchorMin = er.anchorMax = new Vector2(0.5f, 0f); er.anchoredPosition = new Vector2(-130, 40);

            var close = UiTheme.Button(panel.transform, "Close", Hide, 200, 48);
            var cr = (RectTransform)close.transform;
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0f); cr.anchoredPosition = new Vector2(130, 40);
        }

        private void Populate()
        {
            _lastPad = ControlGlyphs.UsingGamepad;
            _lastBrand = ControlGlyphs.Brand;
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);

            const float rowH = 40f, gap = 5f;
            var list = InputBindings.Rebindable;
            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];

                var row = new GameObject(entry.Action.name + "Row", typeof(RectTransform));
                row.transform.SetParent(_content, false);
                var rrt = (RectTransform)row.transform;
                rrt.anchorMin = new Vector2(0f, 1f); rrt.anchorMax = new Vector2(1f, 1f); rrt.pivot = new Vector2(0.5f, 1f);
                rrt.sizeDelta = new Vector2(0f, rowH);
                rrt.anchoredPosition = new Vector2(0f, -i * (rowH + gap));
                row.AddComponent<Image>().color = (i % 2 == 0) ? UiTheme.TrackColor : new Color(0.16f, 0.18f, 0.23f, 1f);

                var label = UiTheme.Label(row.transform, entry.Label, 20, UiTheme.TextColor, TextAnchor.MiddleLeft);
                var lr = label.Rect;
                lr.anchorMin = new Vector2(0.05f, 0f); lr.anchorMax = new Vector2(0.62f, 1f);
                lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;

                var sprite = ControlGlyphs.Sprite(entry.Action);
                if (sprite != null)
                {
                    var glyphGo = new GameObject("Glyph", typeof(RectTransform), typeof(Image));
                    glyphGo.transform.SetParent(row.transform, false);
                    var img = glyphGo.GetComponent<Image>();
                    img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false;
                    var gr = (RectTransform)glyphGo.transform;
                    gr.anchorMin = gr.anchorMax = new Vector2(0.9f, 0.5f); gr.pivot = new Vector2(0.5f, 0.5f);
                    gr.sizeDelta = new Vector2(30, 30);
                }
                else
                {
                    var token = UiTheme.Label(row.transform, ControlGlyphs.Token(entry.Action), 20, Color.white, TextAnchor.MiddleRight);
                    var tr = token.Rect;
                    tr.anchorMin = new Vector2(0.64f, 0f); tr.anchorMax = new Vector2(0.96f, 1f);
                    tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
                }
            }
        }

        private void SuspendPlayer()
        {
            _suspended.Clear();
            foreach (var b in CollectPlayerBehaviours())
            {
                _suspended.Add((b, b.enabled));
                b.enabled = false;
            }
        }

        private void RestorePlayer()
        {
            foreach (var (beh, wasEnabled) in _suspended)
                if (beh != null) beh.enabled = wasEnabled;
            _suspended.Clear();
        }

        private static List<Behaviour> CollectPlayerBehaviours()
        {
            var list = new List<Behaviour>();
            var combat = FindObjectOfType<PlayerCombatController>(); if (combat != null) list.Add(combat);
            var fp = FindObjectOfType<FirstPersonRig>(); if (fp != null) list.Add(fp);
            var tp = FindObjectOfType<ThirdPersonRig>(); if (tp != null) list.Add(tp);
            return list;
        }
    }
}
