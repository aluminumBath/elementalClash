using UnityEngine;
using TMPro;

namespace Elementborn.Game
{
    /// <summary>A loading overlay shown during the heavy world-generation step so the player sees a clean
    /// "Generating world" screen instead of a frozen, half-built frame. A dark quad parented to the active camera
    /// covers the view in both flat and VR, with a message, animated dots, and a progress bar. The flow controller
    /// yields a frame after <see cref="Show"/> so this renders before the blocking build, then steps
    /// <see cref="SetProgress"/> between build phases. Built once and toggled with SetActive.</summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }

        private GameObject _root;
        private TextMeshPro _label;
        private Transform _fill;
        private string _message = "Loading";
        private float _progress;
        private float _dotTimer;
        private int _dots;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Show(string message)
        {
            _message = string.IsNullOrEmpty(message) ? "Loading" : message;
            _progress = 0f;
            Build();
            ApplyBar();
            if (_root != null) _root.SetActive(true);
        }

        public void SetProgress(float p) { _progress = Mathf.Clamp01(p); ApplyBar(); }

        public void Hide() { if (_root != null) _root.SetActive(false); }

        private void Build()
        {
            if (_root != null) return;
            _root = new GameObject("LoadingScreen");
            var cam = Camera.main;
            if (cam != null)
            {
                _root.transform.SetParent(cam.transform, false);
                _root.transform.localPosition = new Vector3(0f, 0f, 0.5f);
                _root.transform.localRotation = Quaternion.identity;
            }

            MakeQuad("Cover", new Vector3(0f, 0f, 0f), new Vector3(6f, 6f, 1f), new Color(0.03f, 0.03f, 0.05f));

            var font = Elementborn.Game.ElementbornTmpFontUtility.GetDefaultFontAsset();
            if (font != null)
            {
                var lg = new GameObject("Label");
                lg.transform.SetParent(_root.transform, false);
                lg.transform.localPosition = new Vector3(0f, 0.05f, -0.06f);
                _label = lg.AddComponent<TextMeshPro>();
                _label.font = font;
                _label.fontSize = 1.2f;
                _label.alignment = TextAlignmentOptions.Center;
                _label.color = Color.white;
                _label.rectTransform.sizeDelta = new Vector2(3.4f, 1f);
            }

            var track = MakeQuad("Track", new Vector3(0f, -0.18f, -0.05f), new Vector3(1.6f, 0.05f, 1f), new Color(0.2f, 0.2f, 0.26f));
            var fillGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var fcol = fillGo.GetComponent<Collider>(); if (fcol != null) Destroy(fcol);
            fillGo.transform.SetParent(track.transform, false);
            var fmr = fillGo.GetComponent<MeshRenderer>(); if (fmr != null) fmr.sharedMaterial = ToonPalette.Tinted(new Color(0.5f, 0.8f, 1f));
            _fill = fillGo.transform;
        }

        private GameObject MakeQuad(string name, Vector3 localPos, Vector3 localScale, Color tint)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = name;
            var col = q.GetComponent<Collider>(); if (col != null) Destroy(col);
            q.transform.SetParent(_root.transform, false);
            q.transform.localPosition = localPos;
            q.transform.localScale = localScale;
            var mr = q.GetComponent<MeshRenderer>(); if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(tint);
            return q;
        }

        private void ApplyBar()
        {
            if (_fill == null) return;
            float p = Mathf.Clamp01(_progress);
            _fill.localScale = new Vector3(p, 1f, 1f);
            _fill.localPosition = new Vector3(-0.5f + p * 0.5f, 0f, -0.01f); // left-anchored within the track
        }

        private void Update()
        {
            if (_root == null || !_root.activeSelf || _label == null) return;
            _dotTimer += Time.unscaledDeltaTime;
            if (_dotTimer >= 0.35f) { _dotTimer = 0f; _dots = (_dots + 1) % 4; }
            _label.text = _message + new string('.', _dots);
        }
    }
}
