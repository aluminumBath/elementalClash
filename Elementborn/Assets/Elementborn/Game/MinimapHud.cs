using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// An always-on corner minimap (top-right): a player-centred window that plots discovered leyline rifts within
    /// range, north up. Reads <see cref="MapState"/>'s network and the player's position each frame. The bootstrap
    /// scene adds one.
    /// </summary>
    public sealed class MinimapHud : MonoBehaviour
    {
        [SerializeField] private float range = 150f; // world units from centre to the minimap edge
        [SerializeField] private float size = 210f;   // minimap square, px

        private RectTransform _plot;
        private readonly List<Image> _pool = new List<Image>();
        private readonly List<Image> _friendPool = new List<Image>();
        private readonly List<Image> _checkpointPool = new List<Image>();

        private static readonly Color Frame   = new Color(0.06f, 0.07f, 0.10f, 0.78f);
        private static readonly Color RiftDot  = new Color(0.45f, 0.85f, 1f, 1f);
        private static readonly Color SelfDot  = new Color(0.95f, 0.85f, 0.30f, 1f);
        private static readonly Color FriendDot = new Color(0.55f, 0.95f, 0.55f, 1f);
        private static readonly Color CheckpointDot = new Color(1f, 0.62f, 0.22f, 1f);

        private void Awake() => Build();

        private void Build()
        {
            var canvas = UiTheme.Canvas("MinimapCanvas", 40);
            canvas.transform.SetParent(transform, false);

            var frame = new GameObject("Minimap", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            frame.transform.SetParent(canvas.transform, false);
            var fr = (RectTransform)frame.transform;
            fr.anchorMin = fr.anchorMax = new Vector2(1f, 1f); fr.pivot = new Vector2(1f, 1f);
            fr.sizeDelta = new Vector2(size, size); fr.anchoredPosition = new Vector2(-16, -16);
            frame.GetComponent<Image>().color = Frame;

            var plotGo = new GameObject("Plot", typeof(RectTransform));
            plotGo.transform.SetParent(frame.transform, false);
            _plot = (RectTransform)plotGo.transform;
            _plot.anchorMin = Vector2.zero; _plot.anchorMax = Vector2.one;
            _plot.offsetMin = Vector2.zero; _plot.offsetMax = Vector2.zero;

            // the player sits at the centre, fixed
            var self = Dot("Self", SelfDot, 12f);
            self.rectTransform.SetParent(_plot, false);
            self.rectTransform.anchorMin = self.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            self.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            self.rectTransform.anchoredPosition = Vector2.zero;
        }

        private static Image Dot(string name, Color c, float px)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var img = go.GetComponent<Image>();
            img.color = c; img.raycastTarget = false;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(px, px);
            return img;
        }

        private void Update()
        {
            var rig = RigTeleporter.Rig;
            var state = MapState.Instance;
            if (_plot == null || rig == null || state == null || state.Network == null) return;

            Vector3 c = rig.position;
            float half = size * 0.5f;
            int used = 0;
            foreach (var r in state.Network.Discovered())
            {
                if (!Minimap.WithinRange(c, range, r.World)) continue;
                float u = (r.World.x - c.x) / range; // -1..1 east
                float v = (r.World.z - c.z) / range; // -1..1 north (up)
                var dot = FromPool(used++);
                dot.rectTransform.anchoredPosition = new Vector2(u * half, v * half);
                dot.gameObject.SetActive(true);
            }
            for (int i = used; i < _pool.Count; i++) _pool[i].gameObject.SetActive(false);

            int usedF = 0;
            foreach (var f in state.FriendMarkers())
            {
                if (!Minimap.WithinRange(c, range, f.World)) continue;
                float u = (f.World.x - c.x) / range;
                float v = (f.World.z - c.z) / range;
                var dot = FriendFromPool(usedF++);
                dot.rectTransform.anchoredPosition = new Vector2(u * half, v * half);
                dot.gameObject.SetActive(true);
            }
            for (int i = usedF; i < _friendPool.Count; i++) _friendPool[i].gameObject.SetActive(false);

            int usedC = 0;
            var cps = CheckpointState.Instance;
            if (cps != null)
                foreach (var m in cps.Markers())
                {
                    if (!Minimap.WithinRange(c, range, m.World)) continue;
                    float u = (m.World.x - c.x) / range;
                    float v = (m.World.z - c.z) / range;
                    var dot = CheckpointFromPool(usedC++);
                    dot.rectTransform.anchoredPosition = new Vector2(u * half, v * half);
                    dot.gameObject.SetActive(true);
                }
            for (int i = usedC; i < _checkpointPool.Count; i++) _checkpointPool[i].gameObject.SetActive(false);
        }

        private Image FromPool(int i)
        {
            while (_pool.Count <= i)
            {
                var d = Dot("Rift", RiftDot, 10f);
                d.rectTransform.SetParent(_plot, false);
                _pool.Add(d);
            }
            return _pool[i];
        }

        private Image FriendFromPool(int i)
        {
            while (_friendPool.Count <= i)
            {
                var d = Dot("Friend", FriendDot, 11f);
                d.rectTransform.SetParent(_plot, false);
                _friendPool.Add(d);
            }
            return _friendPool[i];
        }

        private Image CheckpointFromPool(int i)
        {
            while (_checkpointPool.Count <= i)
            {
                var d = Dot("Checkpoint", CheckpointDot, 9f);
                d.rectTransform.SetParent(_plot, false);
                _checkpointPool.Add(d);
            }
            return _checkpointPool[i];
        }
    }
}
