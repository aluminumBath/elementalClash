using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Builds a single showcase "room" that previews all the new content at once: a floor and light, the four
    /// elemental portal pads (plus the neutral gate) laid out as a small <b>portal map</b> — each glowing in its
    /// PortalTheme colour (water teal, fire ember, earth moss, air updraft) — and every NPC lined up with a name
    /// label: the four guides (Willow, Kiana, Parfa, Deb) and the ten royals (Ronald, Renee, the Windwyrms, the
    /// Flowers, Ella and Eloc). Drop this on an empty GameObject in a blank scene and press Play; walk up to any
    /// figure to talk, or any pad to read it. A no-art, no-scene way to see everything together.
    /// </summary>
    public sealed class PreviewRoomBuilder : MonoBehaviour
    {
        [SerializeField] private bool buildOnStart = true;
        private bool _built;

        private void Start() { if (buildOnStart) Build(); }

        public void Build()
        {
            if (_built) return;
            _built = true;

            var room = new GameObject("PreviewRoom").transform;

            Floor(room, new Vector3(0f, -0.5f, 4f), new Vector3(46f, 1f, 30f));

            var lightGo = new GameObject("PreviewLight");
            lightGo.transform.SetParent(room, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Portal map — the four elemental pads in a row, plus the neutral Confluence gate.
            Element[] els = { Element.Fire, Element.Water, Element.Earth, Element.Air };
            for (int i = 0; i < els.Length; i++)
                PortalPad(room, els[i], new Vector3(-9f + i * 6f, 0.05f, 13f));
            PortalPad(room, null, new Vector3(15f, 0.05f, 13f)); // neutral gate

            // Guides + Deb, front row.
            float x = -18f;
            GuideNpcId[] guides = { GuideNpcId.Willow, GuideNpcId.Kiana, GuideNpcId.Parfa, GuideNpcId.Deb };
            foreach (var g in guides) { NpcShowcase.SpawnGuide(g, new Vector3(x, 0f, 0f), room); x += 2.6f; }

            // The royal houses, back row.
            x = -12f;
            foreach (var r in RoyalCatalog.All) { NpcShowcase.SpawnRoyal(r, new Vector3(x, 0f, 5f), room); x += 2.6f; }
        }

        private static void Floor(Transform parent, Vector3 center, Vector3 size)
        {
            var f = GameObject.CreatePrimitive(PrimitiveType.Cube);
            f.name = "Floor";
            f.transform.SetParent(parent, false);
            f.transform.position = center;
            f.transform.localScale = size;
            var r = f.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.16f, 0.17f, 0.20f);
        }

        private static void PortalPad(Transform parent, Element? element, Vector3 pos)
        {
            PortalStyle style = PortalTheme.For(element);
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = "Portal_" + (element.HasValue ? element.Value.ToString() : "Neutral");
            pad.transform.SetParent(parent, false);
            pad.transform.position = pos;
            pad.transform.localScale = new Vector3(2.4f, 0.1f, 2.4f);
            var r = pad.GetComponent<Renderer>();
            if (r != null)
            {
                var m = r.material;
                m.color = style.Glow;
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", style.Glow * 2f);
            }
            NpcShowcase.Label(pad.transform, (element.HasValue ? element.Value.ToString() : "Neutral") + " portal");
        }
    }
}
