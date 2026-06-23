using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Builds the character-creation front-end entirely in code (like the VFX), so the roll is
    /// clickable the moment you press play. Flat mode works out of the box. In VR the canvas is
    /// World Space — add an XRI TrackedDeviceGraphicRaycaster + XRUIInputModule and a controller
    /// ray interactor (standard XRI UI setup) to make it clickable in-headset.
    ///
    /// Flow (locked single roll): Path -> Element/Weapon -> Reveal -> Begin. There is no
    /// re-roll: the reveal commits the character.
    /// </summary>
    [RequireComponent(typeof(CharacterCreationController))]
    public sealed class CharacterCreationUI : MonoBehaviour
    {
        public enum Mode { Auto, Flat, Vr }

        [SerializeField] private Mode mode = Mode.Auto;
        [SerializeField] private Vector3 vrCanvasPosition = new Vector3(0f, 1.4f, 2f);
        [SerializeField] private float vrCanvasScale = 0.0025f;

        /// <summary>Raised when the player taps Begin after the reveal.</summary>
        public UnityEngine.Events.UnityEvent OnComplete = new UnityEngine.Events.UnityEvent();

        private CharacterCreationController _controller;
        private Canvas _canvas;
        private GameObject _pathPanel, _elementPanel, _weaponPanel, _revealPanel;
        private UiLabel _revealText;

        private void Awake()
        {
            _controller = GetComponent<CharacterCreationController>();
            _controller.OnResult.AddListener(ShowReveal);

            bool vr = mode == Mode.Vr || (mode == Mode.Auto && IsXrActive());
            BuildCanvas(vr);
            EnsureEventSystem(vr);
            BuildPanels();
            ShowOnly(_pathPanel);

            if (vr)
                Debug.Log("[Elementborn] Creation UI is World Space. Add an XRI " +
                          "TrackedDeviceGraphicRaycaster + XRUIInputModule and a controller ray " +
                          "interactor to make it clickable in-headset.");
        }

        // --- construction ----------------------------------------------------------
        private void BuildCanvas(bool vr)
        {
            var canvasGo = new GameObject("CreationCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<GraphicRaycaster>();

            if (vr)
            {
                _canvas.renderMode = RenderMode.WorldSpace;
                var rt = _canvas.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(800f, 600f);
                rt.localScale = Vector3.one * vrCanvasScale;
                rt.position = vrCanvasPosition;
            }
            else
            {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }
        }

        private void EnsureEventSystem(bool vr)
        {
            if (EventSystem.current != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            // Flat input module; VR uses XRI's XRUIInputModule, added with the XR rig in-editor.
            if (!vr) es.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildPanels()
        {
            _pathPanel = CreatePanel("PathPanel", "Choose Your Path");
            CreateButton(_pathPanel, "Channel an Element", 60f, () => ShowOnly(_elementPanel));
            CreateButton(_pathPanel, "Wield a Weapon", -20f, () => ShowOnly(_weaponPanel));

            _elementPanel = CreatePanel("ElementPanel", "Choose Your Element");
            CreateButton(_elementPanel, "Fire", 140f, _controller.ChooseFire);
            CreateButton(_elementPanel, "Water", 60f, _controller.ChooseWater);
            CreateButton(_elementPanel, "Earth", -20f, _controller.ChooseEarth);
            CreateButton(_elementPanel, "Air", -100f, _controller.ChooseAir);
            CreateButton(_elementPanel, "Back", -200f, () => ShowOnly(_pathPanel));

            _weaponPanel = CreatePanel("WeaponPanel", "Choose Your Weapon");
            CreateButton(_weaponPanel, "Hammer", 180f, _controller.ChooseWeaponHammer);
            CreateButton(_weaponPanel, "Sword", 114f, _controller.ChooseWeaponSword);
            CreateButton(_weaponPanel, "Long Bow", 48f, _controller.ChooseWeaponLongBow);
            CreateButton(_weaponPanel, "Shield", -18f, _controller.ChooseWeaponShield);
            CreateButton(_weaponPanel, "Dagger", -84f, _controller.ChooseWeaponDagger);
            CreateButton(_weaponPanel, "Sai", -150f, _controller.ChooseWeaponSai);
            CreateButton(_weaponPanel, "Back", -230f, () => ShowOnly(_pathPanel));

            _revealPanel = CreatePanel("RevealPanel", string.Empty);
            _revealText = CreateText(_revealPanel, string.Empty, 40, new Vector2(0f, 60f), new Vector2(720f, 220f));
            CreateButton(_revealPanel, "Begin", -160f, Complete);
        }

        // --- flow -------------------------------------------------------------------
        private void ShowReveal(CharacterCreationResult result)
        {
            _revealText.text = Headline(result);
            ShowOnly(_revealPanel);
        }

        private static string Headline(CharacterCreationResult r)
        {
            switch (r.Tier)
            {
                case RevealTier.Confluence:
                    return "THE CONFLUENCE\nYou command all four elements!";
                case RevealTier.SubArt:
                    return r.Loadout.SubArts.Count > 0
                        ? $"{r.Loadout.SubArts[0].ToString().ToUpper()}!\nA rare gift awakens."
                        : "A RARE GIFT!";
                case RevealTier.BaseElement:
                    return $"{r.ChosenElement.ToString().ToUpper()} CHANNELER";
                case RevealTier.Weapon:
                    return $"WARRIOR\nYour weapon: {r.Loadout.Weapon}";
                default:
                    return "Ready";
            }
        }

        private void Complete()
        {
            _canvas.gameObject.SetActive(false);
            OnComplete.Invoke();
        }

        // --- ui helpers -------------------------------------------------------------
        private GameObject CreatePanel(string panelName, string title)
        {
            var img = UiTheme.Panel(_canvas.transform, UiTheme.PanelColor);
            var panel = img.gameObject;
            panel.name = panelName;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            if (!string.IsNullOrEmpty(title))
                CreateText(panel, title, 54, new Vector2(0f, 230f), new Vector2(800f, 120f));
            return panel;
        }

        private Button CreateButton(GameObject parent, string label, float y, Action onClick)
        {
            var btn = UiTheme.Button(parent.transform, label, onClick, 360, 64);
            var rt = (RectTransform)btn.transform;
            rt.anchoredPosition = new Vector2(0f, y);
            return btn;
        }

        private UiLabel CreateText(GameObject parent, string content, int size, Vector2 pos, Vector2 dim)
        {
            var lbl = UiTheme.Label(parent.transform, content, size, Color.white, TextAnchor.MiddleCenter);
            var rt = lbl.Rect;
            rt.sizeDelta = dim;
            rt.anchoredPosition = pos;
            return lbl;
        }

        private void ShowOnly(GameObject panel)
        {
            _pathPanel.SetActive(panel == _pathPanel);
            _elementPanel.SetActive(panel == _elementPanel);
            _weaponPanel.SetActive(panel == _weaponPanel);
            _revealPanel.SetActive(panel == _revealPanel);
        }

        private static bool IsXrActive()
        {
#if UNITY_2020_1_OR_NEWER
            var s = UnityEngine.XR.Management.XRGeneralSettings.Instance;
            return s != null && s.Manager != null && s.Manager.activeLoader != null;
#else
            return false;
#endif
        }
    }
}
