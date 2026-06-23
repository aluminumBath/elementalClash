using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// A code-built controls menu (toggle with F10, or open it from Settings) that lists every rebindable
    /// action with its keyboard/mouse binding and its gamepad binding. Tap a binding to listen for the next
    /// control and reassign it (Esc cancels); changes persist via <see cref="InputBindings"/>. A Reset button
    /// restores defaults. Movement/look sticks and the VR scheme are fixed and not shown here.
    /// </summary>
    public sealed class RebindController : MonoBehaviour
    {
        public static RebindController Instance { get; private set; }

        [SerializeField] private Key toggleKey = Key.F10;

        private Canvas _canvas;
        private GameObject _root;
        private Transform _content;
        private UiLabel _status;
        private bool _open;
        private bool _listening;
        private readonly List<(Behaviour beh, bool wasEnabled)> _suspended = new List<(Behaviour, bool)>();

        public static RebindController EnsureInstance()
        {
            if (Instance != null) return Instance;
            return new GameObject("RebindController").AddComponent<RebindController>();
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
            if (!_listening && k != null && k[toggleKey].wasPressedThisFrame) Toggle();
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
            if (_listening) return; // don't close mid-rebind
            if (_root != null) _root.SetActive(false);
            _open = false;
            RestorePlayer();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

        private void Build()
        {
            _canvas = UiTheme.Canvas("RebindCanvas", sortOrder: 210);
            _canvas.transform.SetParent(transform, false);

            _root = new GameObject("RebindRoot", typeof(RectTransform));
            _root.transform.SetParent(_canvas.transform, false);
            var rootRt = (RectTransform)_root.transform;
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;

            var dim = UiTheme.Panel(_root.transform, new Color(0f, 0f, 0f, 0.55f), "overlay_dim");
            var dr = dim.rectTransform; dr.anchorMin = Vector2.zero; dr.anchorMax = Vector2.one;
            dr.offsetMin = Vector2.zero; dr.offsetMax = Vector2.zero;

            var panel = UiTheme.Panel(_root.transform);
            var pr = panel.rectTransform;
            pr.sizeDelta = new Vector2(820, 700);
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.anchoredPosition = Vector2.zero;

            var title = UiTheme.Label(panel.transform, "Controls", 34, Color.white, TextAnchor.UpperCenter);
            var tr = title.Rect; tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f); tr.sizeDelta = new Vector2(-40, 44); tr.anchoredPosition = new Vector2(0, -14);

            _status = UiTheme.Label(panel.transform, "Left = keyboard/mouse   ·   Right = gamepad",
                18, new Color(0.8f, 0.84f, 0.9f), TextAnchor.UpperCenter);
            var srt = _status.Rect; srt.anchorMin = new Vector2(0f, 1f); srt.anchorMax = new Vector2(1f, 1f);
            srt.pivot = new Vector2(0.5f, 1f); srt.sizeDelta = new Vector2(-40, 26); srt.anchoredPosition = new Vector2(0, -58);

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(panel.transform, false);
            _content = contentGo.transform;
            var crt = (RectTransform)contentGo.transform;
            crt.anchorMin = new Vector2(0.04f, 0.13f); crt.anchorMax = new Vector2(0.96f, 0.86f);
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

            var reset = UiTheme.Button(panel.transform, "Reset to defaults", () => { InputBindings.ResetAll(); Populate(); }, 200, 48);
            var rr = (RectTransform)reset.transform;
            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0f); rr.anchoredPosition = new Vector2(-220, 40);

            var legend = UiTheme.Button(panel.transform, "Legend",
                () => { Hide(); ControlsLegendController.EnsureInstance().Show(); }, 180, 48);
            var lr = (RectTransform)legend.transform;
            lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0f); lr.anchoredPosition = new Vector2(0, 40);

            var close = UiTheme.Button(panel.transform, "Close", Hide, 200, 48);
            var cr = (RectTransform)close.transform;
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0f); cr.anchoredPosition = new Vector2(220, 40);
        }

        private void Populate()
        {
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);

            const float rowH = 44f, gap = 6f;
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
                row.AddComponent<UnityEngine.UI.Image>().color = (i % 2 == 0) ? UiTheme.TrackColor : new Color(0.16f, 0.18f, 0.23f, 1f);

                var label = UiTheme.Label(row.transform, entry.Label, 20, UiTheme.TextColor);
                Band(label.Rect, 0.02f, 0.40f);

                int kbm = 0, pad = 1;
                var kbmBtn = UiTheme.Button(row.transform, ControlGlyphs.Token(entry.Action, kbm),
                    () => BeginRebind(entry.Action, kbm), 200, 36);
                Band((RectTransform)kbmBtn.transform, 0.42f, 0.69f);

                var padBtn = UiTheme.Button(row.transform, ControlGlyphs.Token(entry.Action, pad),
                    () => BeginRebind(entry.Action, pad), 200, 36);
                Band((RectTransform)padBtn.transform, 0.71f, 0.98f);
            }
        }

        private void BeginRebind(InputAction action, int bindingIndex)
        {
            if (_listening) return;
            _listening = true;
            string which = bindingIndex == 0 ? "key or mouse button" : "gamepad button";
            if (_status != null) _status.text = $"Press a {which}…   (Esc to cancel)";

            InputBindings.StartRebind(action, bindingIndex, () =>
            {
                _listening = false;
                if (_status != null) _status.text = "Left = keyboard/mouse   ·   Right = gamepad";
                Populate();
            });
        }

        private static void Band(RectTransform rt, float xMin, float xMax)
        {
            rt.anchorMin = new Vector2(xMin, 0.12f);
            rt.anchorMax = new Vector2(xMax, 0.88f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
