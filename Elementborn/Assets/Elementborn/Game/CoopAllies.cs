using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Renders your friends as visible ally figures in the world — turning the tracked, consent-gated
    /// presence (which until now only drew dots on the map) into figures you can actually see moving around beside
    /// you. Reads MapState's fresh friend positions each frame, spawns a per-friend tinted capsule + floating
    /// nameplate, smoothly chases each reported position, billboards the name to the camera, and despawns anyone who
    /// drops out of presence. Works unchanged with the dev presence simulator offline and the Nakama feed online —
    /// the figures follow whatever positions the active source publishes.</summary>
    public sealed class CoopAllies : MonoBehaviour
    {
        private const float FollowLerp = 6f;   // how snappily a figure chases its reported position
        private const float NameHeight = 2.4f; // nameplate height above the figure's feet

        private sealed class Ally
        {
            public GameObject Go;
            public TextMeshPro Name;
            public Vector3 Target;
        }

        private readonly Dictionary<string, Ally> _allies = new Dictionary<string, Ally>();
        private readonly List<string> _stale = new List<string>();
        private Transform _cam;

        private void Update()
        {
            var map = MapState.Instance;
            if (map == null) return;
            if (_cam == null && Camera.main != null) _cam = Camera.main.transform;

            var positions = map.VisibleFriendWorldPositions;

            foreach (var kv in positions)
            {
                if (!_allies.TryGetValue(kv.Key, out var a)) { a = Build(kv.Key); _allies[kv.Key] = a; }
                a.Target = kv.Value;
            }

            _stale.Clear();
            float k = 1f - Mathf.Exp(-FollowLerp * Time.deltaTime);
            foreach (var kv in _allies)
            {
                if (!positions.ContainsKey(kv.Key)) { _stale.Add(kv.Key); continue; }
                var a = kv.Value;
                Vector3 feet = a.Target;
                feet.y = TerrainHeight.Sample(a.Target) + 0.05f;
                a.Go.transform.position = Vector3.Lerp(a.Go.transform.position, feet, k);
                if (_cam != null && a.Name != null) a.Name.transform.rotation = _cam.rotation; // billboard
            }

            for (int i = 0; i < _stale.Count; i++)
            {
                if (_allies[_stale[i]].Go != null) Destroy(_allies[_stale[i]].Go);
                _allies.Remove(_stale[i]);
            }
        }

        private Ally Build(string friendId)
        {
            string name = friendId;
            var hub = SocialHub.Instance;
            if (hub != null && hub.Directory.TryGet(friendId, out var user)) name = user.Name;

            Color tint = TintFor(friendId);
            var root = new GameObject("CoopAlly_" + friendId);

            // body: a capsule figure, no collider (it's a presence marker, not a blocking actor)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            var col = body.GetComponent<Collider>(); if (col != null) Destroy(col);
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            var mr = body.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(tint);

            // nameplate: TMP world text, asset-free like FloatingText (skip if no default font)
            TextMeshPro tmp = null;
            var font = TMP_Settings.defaultFontAsset;
            if (font != null)
            {
                var label = new GameObject("Name");
                label.transform.SetParent(root.transform, false);
                label.transform.localPosition = new Vector3(0f, NameHeight, 0f);
                tmp = label.AddComponent<TextMeshPro>();
                tmp.font = font;
                tmp.text = name;
                tmp.fontSize = 3f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = tint;
                tmp.rectTransform.sizeDelta = new Vector2(6f, 1.2f);
            }

            return new Ally { Go = root, Name = tmp, Target = root.transform.position };
        }

        // Stable, bright per-friend hue derived from the id so each ally is consistently recognisable.
        private static Color TintFor(string id)
        {
            int h = 0;
            foreach (char c in id) h = h * 31 + c;
            float hue = (Mathf.Abs(h) % 360) / 360f;
            return Color.HSVToRGB(hue, 0.55f, 1f);
        }

        private void OnDestroy()
        {
            foreach (var kv in _allies) if (kv.Value.Go != null) Destroy(kv.Value.Go);
            _allies.Clear();
        }
    }
}
