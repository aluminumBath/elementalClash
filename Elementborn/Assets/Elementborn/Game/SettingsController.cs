using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// A code-built settings overlay (volumes, mouse sensitivity, field of view, invert-Y, comfort vignette).
    /// Toggle with Escape. Changes write <see cref="SettingsStore"/> and call SaveAndApply, so
    /// <see cref="AudioController"/>, <see cref="FirstPersonRig"/>/<see cref="ThirdPersonRig"/> and
    /// <see cref="ComfortVignette"/> pick them up immediately. While open it shows the cursor and disables the
    /// player's movement/combat. Drop it on a persistent object, or let another system spawn it.
    /// </summary>
    public sealed class SettingsController : MonoBehaviour
    {
        public static SettingsController Instance { get; private set; }

        private Canvas _canvas;
        private GameObject _root;
        private bool _open;
        private readonly List<(Behaviour beh, bool wasEnabled)> _suspended = new List<(Behaviour, bool)>();

        public static SettingsController EnsureInstance()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("SettingsController");
            return go.AddComponent<SettingsController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            if (InputBindings.Menu.WasPressedThisFrame()) Toggle();
        }

        public void Toggle() { if (_open) Close(); else Open(); }

        public void Open()
        {
            if (_open) return;
            _open = true;
            if (_root == null) Build();
            _root.SetActive(true);

            SuspendPlayer();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            AudioController.Instance?.Confirm();
        }

        public void Close()
        {
            if (!_open) return;
            _open = false;
            if (_root != null) _root.SetActive(false);

            RestorePlayer();
            AudioController.Instance?.Back();
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("SettingsCanvas", sortOrder: 200);
            _canvas.transform.SetParent(transform, false);

            _root = new GameObject("SettingsRoot", typeof(RectTransform));
            _root.transform.SetParent(_canvas.transform, false);
            var rootRt = (RectTransform)_root.transform;
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;

            var dim = UiTheme.Panel(_root.transform, new Color(0f, 0f, 0f, 0.5f), "overlay_dim");
            var dr = dim.rectTransform; dr.anchorMin = Vector2.zero; dr.anchorMax = Vector2.one;
            dr.offsetMin = Vector2.zero; dr.offsetMax = Vector2.zero;

            var panel = UiTheme.Panel(_root.transform);
            var pr = panel.rectTransform;
            pr.sizeDelta = new Vector2(640, 600);
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.anchoredPosition = Vector2.zero;

            var title = UiTheme.Label(panel.transform, "Settings", 34, Color.white, TextAnchor.UpperCenter);
            var tr = title.Rect; tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f); tr.sizeDelta = new Vector2(-40, 48); tr.anchoredPosition = new Vector2(0, -18);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(panel.transform, false);
            var crt = (RectTransform)content.transform;
            crt.anchorMin = new Vector2(0.06f, 0.22f); crt.anchorMax = new Vector2(0.94f, 0.86f);
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
            var vlg = content.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vlg.spacing = 10; vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            var s = SettingsStore.Current;
            UiTheme.Slider(content.transform, "Master volume", s.masterVolume, 0f, 1f,
                v => { s.masterVolume = v; SettingsStore.SaveAndApply(); });
            UiTheme.Slider(content.transform, "Music volume", s.musicVolume, 0f, 1f,
                v => { s.musicVolume = v; SettingsStore.SaveAndApply(); });
            UiTheme.Slider(content.transform, "SFX volume", s.sfxVolume, 0f, 1f,
                v => { s.sfxVolume = v; SettingsStore.SaveAndApply(); });
            UiTheme.Slider(content.transform, "Mouse sensitivity", s.mouseSensitivity, 0.1f, 5f,
                v => { s.mouseSensitivity = v; SettingsStore.SaveAndApply(); });
            UiTheme.Slider(content.transform, "Field of view", s.fieldOfView, 50f, 100f,
                v => { s.fieldOfView = v; SettingsStore.SaveAndApply(); });
            UiTheme.Toggle(content.transform, "Invert look (Y)", s.invertY,
                v => { s.invertY = v; SettingsStore.SaveAndApply(); });
            UiTheme.Toggle(content.transform, "Comfort vignette (VR)", s.comfortVignette,
                v => { s.comfortVignette = v; SettingsStore.SaveAndApply(); });

            var controls = UiTheme.Button(panel.transform, "Controls…",
                () => { Close(); RebindController.EnsureInstance().Show(); }, 240, 48);
            var conr = (RectTransform)controls.transform;
            conr.anchorMin = conr.anchorMax = new Vector2(0.5f, 0f);
            conr.anchoredPosition = new Vector2(0, 100);

            var close = UiTheme.Button(panel.transform, "Close", Close, 200, 52);
            var br = (RectTransform)close.transform;
            br.anchorMin = br.anchorMax = new Vector2(0.5f, 0f);
            br.anchoredPosition = new Vector2(0, 40);
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
            // Closing returns to gameplay, so relock the cursor for mouse-look.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static IEnumerable<Behaviour> CollectPlayerBehaviours()
        {
            var list = new List<Behaviour>();
            var combat = FindObjectOfType<PlayerCombatController>(); if (combat != null) list.Add(combat);
            var fp = FindObjectOfType<FirstPersonRig>(); if (fp != null) list.Add(fp);
            var tp = FindObjectOfType<ThirdPersonRig>(); if (tp != null) list.Add(tp);
            return list;
        }
    }
}
