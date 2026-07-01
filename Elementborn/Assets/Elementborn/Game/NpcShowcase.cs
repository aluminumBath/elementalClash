using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Shared spawn helpers for the NPC home spawner and the preview room: a colour-coded capsule
    /// placeholder carrying the right interact controller (talkable) and a floating name label. Replace the
    /// capsules with real models later, or bind models to the controllers.</summary>
    public static class NpcShowcase
    {
        public static GameObject SpawnRoyal(Royal id, Vector3 pos, Transform parent = null)
        {
            RoyalInfo info = RoyalCatalog.For(id);
            GameObject go = Figure("Royal_" + info.Name, pos, ElementColor.For(info.Element), parent);
            go.AddComponent<RoyalNpcController>().Configure(id);
            Label(go.transform, info.Name);
            return go;
        }

        public static GameObject SpawnGuide(GuideNpcId id, Vector3 pos, Transform parent = null)
        {
            GuideNpcInfo info = NpcCatalog.For(id);
            GameObject go = Figure("Guide_" + info.Name, pos, ElementColor.For(info.Element), parent);
            go.AddComponent<GuideNpcController>().Configure(id);
            Label(go.transform, info.Name);
            return go;
        }

        public static GameObject Figure(string name, Vector3 pos, Color color, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos + Vector3.up;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.material.color = color;
            return go;
        }

        public static void Label(Transform parent, string text)
        {
            var lgo = new GameObject("Label");
            lgo.transform.SetParent(parent, false);
            lgo.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var tm = lgo.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) tm.font = font;
            tm.text = text;
            tm.characterSize = 0.14f;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            var mr = lgo.GetComponent<MeshRenderer>();
            if (mr != null && font != null) mr.material = font.material;
        }
    }
}
