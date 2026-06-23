using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The full-screen leyline map (default key M, also opened from a rift's Interact): the overworld backdrop
    /// with every rift plotted by world position — discovered ones tappable to fast-travel, undiscovered ones a
    /// faint dot — plus your own marker, any sharing friends, and the "let friends see me" opt-in. Reads
    /// <see cref="MapState"/>; the bootstrap scene adds one.
    /// </summary>
    public sealed class MapViewerController : MonoBehaviour
    {
        public static MapViewerController Instance { get; private set; }

        [SerializeField] private Key toggleKey = Key.M;

        private const float MapW = 1120f;
        private const float MapH = 630f; // ~16:9 to match the backdrop art (markers align to it)

        private static readonly Color Backdrop  = new Color(0.10f, 0.16f, 0.22f, 1f);
        private static readonly Color RiftKnown = new Color(0.42f, 0.80f, 1f, 1f);
        private static readonly Color RiftDim   = new Color(0.45f, 0.85f, 1f, 0.30f);
        private static readonly Color SelfDot   = new Color(0.95f, 0.85f, 0.30f, 1f);
        private static readonly Color FriendDot = new Color(0.55f, 0.95f, 0.55f, 1f);
        private static readonly Color CheckpointDot    = new Color(1f, 0.62f, 0.22f, 0.70f); // amber shrine
        private static readonly Color CheckpointActive = new Color(1f, 0.74f, 0.36f, 1f);    // the active respawn

        private Canvas _canvas;
        private RectTransform _map;
        private Transform _markers;
        private UiLabel _shareLabel;
        private bool _open;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Build();
            Hide();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
            else if (_open && kb[Key.Escape].wasPressedThisFrame) Hide();
        }

        /// <summary>Open the map (used by a rift's Interact). No-op if already open.</summary>
        public void Open() { if (!_open) Show(); }

        private void Toggle() { if (_open) Hide(); else Show(); }
        private void Show() { _open = true; if (_canvas != null) _canvas.gameObject.SetActive(true); Rebuild(); }
        private void Hide() { _open = false; if (_canvas != null) _canvas.gameObject.SetActive(false); }

        private void Build()
        {
            _canvas = UiTheme.Canvas("MapViewerCanvas", 57);
            _canvas.gameObject.AddComponent<VrCanvasAdapter>();

            var root = new GameObject("Root", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(_canvas.transform, false);
            var rr = (RectTransform)root.transform;
            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
            rr.sizeDelta = new Vector2(1280, 760); rr.anchoredPosition = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.10f, 0.97f);

            var title = UiTheme.Label(root.transform, "Leyline Map", 32, UiTheme.TextColor, TextAnchor.MiddleLeft);
            var tr = title.Rect;
            tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(-220, 52); tr.anchoredPosition = new Vector2(28, -14);

            var close = UiTheme.Button(root.transform, "Close (Esc)", Hide, 150, 42);
            var cr = UiTheme.Rect(close.gameObject);
            cr.anchorMin = cr.anchorMax = new Vector2(1f, 1f); cr.pivot = new Vector2(1f, 1f);
            cr.anchoredPosition = new Vector2(-18, -16);

            // Backdrop map area — markers are placed within it by normalized world position.
            var mapGo = new GameObject("Map", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            mapGo.transform.SetParent(root.transform, false);
            _map = (RectTransform)mapGo.transform;
            _map.anchorMin = _map.anchorMax = new Vector2(0.5f, 1f); _map.pivot = new Vector2(0.5f, 1f);
            _map.sizeDelta = new Vector2(MapW, MapH); _map.anchoredPosition = new Vector2(0, -76);
            var mapImg = mapGo.GetComponent<Image>();
            var bg = LoadBackdrop();
            if (bg != null) { mapImg.sprite = bg; mapImg.color = Color.white; mapImg.preserveAspect = true; }
            else mapImg.color = Backdrop;

            var markersGo = new GameObject("Markers", typeof(RectTransform));
            markersGo.transform.SetParent(_map, false);
            _markers = markersGo.transform;
            var mk = (RectTransform)_markers;
            mk.anchorMin = Vector2.zero; mk.anchorMax = Vector2.one; mk.offsetMin = Vector2.zero; mk.offsetMax = Vector2.zero;

            // Location-sharing opt-in (label + toggle button), bottom-left.
            _shareLabel = UiTheme.Label(root.transform, ShareText(), 20, UiTheme.TextColor, TextAnchor.MiddleLeft);
            var slr = _shareLabel.Rect;
            slr.anchorMin = slr.anchorMax = new Vector2(0f, 0f); slr.pivot = new Vector2(0f, 0f);
            slr.sizeDelta = new Vector2(420, 32); slr.anchoredPosition = new Vector2(28, 22);

            var shareBtn = UiTheme.Button(root.transform, "Toggle sharing", ToggleSharing, 200, 40);
            var sbr = UiTheme.Rect(shareBtn.gameObject);
            sbr.anchorMin = sbr.anchorMax = new Vector2(0f, 0f); sbr.pivot = new Vector2(0f, 0f);
            sbr.anchoredPosition = new Vector2(470, 18);
        }

        private void Rebuild()
        {
            if (_markers == null) return;
            for (int i = _markers.childCount - 1; i >= 0; i--) DestroyImmediate(_markers.GetChild(i).gameObject);

            var state = MapState.Instance;
            if (state == null || state.Network == null) return;

            foreach (var rift in state.Network.All)
            {
                Vector2 n = Minimap.WorldToNormalized(rift.World, WorldMap.BoundsMin, WorldMap.BoundsMax);
                if (state.Network.IsDiscovered(rift.Id)) AddRiftButton(rift, n);
                else AddDot(n, RiftDim, 12f, null);
            }

            var cps = CheckpointState.Instance;
            if (cps != null)
                foreach (var m in cps.Markers())
                {
                    bool active = cps.IsActive(m.Id);
                    Vector2 cn = Minimap.WorldToNormalized(m.World, WorldMap.BoundsMin, WorldMap.BoundsMax);
                    AddDot(cn, active ? CheckpointActive : CheckpointDot, active ? 14f : 11f,
                        active ? m.Label + " (respawn)" : null);
                }

            var rig = RigTeleporter.Rig;
            if (rig != null)
                AddDot(Minimap.WorldToNormalized(rig.position, WorldMap.BoundsMin, WorldMap.BoundsMax), SelfDot, 16f, "You");

            foreach (var f in state.FriendMarkers()) // empty until a live position feed exists (consent-gated)
                AddDot(Minimap.WorldToNormalized(f.World, WorldMap.BoundsMin, WorldMap.BoundsMax), FriendDot, 14f, f.Label);

            if (_shareLabel != null) _shareLabel.text = ShareText();
        }

        private void AddDot(Vector2 n, Color c, float px, string label)
        {
            var go = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_markers, false);
            var img = go.GetComponent<Image>(); img.color = c; img.raycastTarget = false;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(px, px);
            rt.anchoredPosition = new Vector2(n.x * MapW, n.y * MapH);

            if (!string.IsNullOrEmpty(label))
            {
                var l = UiTheme.Label(go.transform, label, 16, c, TextAnchor.MiddleLeft);
                var lr = l.Rect; lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0.5f); lr.pivot = new Vector2(0f, 0.5f);
                lr.anchoredPosition = new Vector2(12, 0); lr.sizeDelta = new Vector2(220, 24);
            }
        }

        private void AddRiftButton(LeylineRift rift, Vector2 n)
        {
            string id = rift.Id;
            var btn = UiTheme.Button(_markers, rift.Name, () => { MapState.Instance?.WarpToRift(id); Hide(); }, 150, 30);
            btn.image.color = RiftKnown;
            var rt = UiTheme.Rect(btn.gameObject);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(n.x * MapW, n.y * MapH);
        }

        private void ToggleSharing()
        {
            if (MapState.Instance == null) return;
            MapState.Instance.ShareMyLocation = !MapState.Instance.ShareMyLocation;
            if (_shareLabel != null) _shareLabel.text = ShareText();
        }

        private static string ShareText() =>
            "Friends can see my location: " + (MapState.Instance != null && MapState.Instance.ShareMyLocation ? "On" : "Off");

        private static Sprite LoadBackdrop()
        {
            var sp = Resources.Load<Sprite>("ElementbornUI/worldmap");
            if (sp != null) return sp;
            var tex = Resources.Load<Texture2D>("ElementbornUI/worldmap");
            if (tex != null) return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            return null;
        }
    }
}
