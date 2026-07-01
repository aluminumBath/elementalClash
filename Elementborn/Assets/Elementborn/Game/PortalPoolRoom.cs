using UnityEngine;
using UnityEngine.Rendering;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The "portal map" as a shimmering, shallow pool you stand inside. The overworld map is painted on the pool
    /// floor; a translucent toon-water surface shimmers over it; and every portal stands as a glowing node in the
    /// water at its map position — walk up and Interact to step through (no map UI, you're standing on the map). A
    /// surrounding inverted water globe plus a reflection probe give the polished "suspended inside a sphere of
    /// water, looking out" look. Every discovered portal is reachable from here.
    ///
    /// Drop this on an empty GameObject where the player will stand and press Play. Water transparency, the globe's
    /// inside-out cull, and VR scale are shader/inspector tuning (the ToonWater material drives the look); the layout,
    /// node placement, discovery glow, and travel are all wired here.
    /// </summary>
    public sealed class PortalPoolRoom : MonoBehaviour
    {
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private float radius = 12f;        // pool radius in metres
        [SerializeField] private float waterDepth = 0.35f;  // shallow
        private bool _built;

        private void Start() { if (buildOnStart) Build(); }

        public void Build()
        {
            if (_built) return;
            _built = true;

            var room = new GameObject("PortalPoolRoom").transform;
            room.SetParent(transform, false);
            const float floorY = 0f;

            // Key light.
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(room, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightGo.transform.rotation = Quaternion.Euler(55f, 30f, 0f);

            // Reflection probe (so the inside of the water globe and the surface reflect the room).
            var probeGo = new GameObject("ReflectionProbe");
            probeGo.transform.SetParent(room, false);
            probeGo.transform.position = new Vector3(0f, 2f, 0f);
            var probe = probeGo.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.boxProjection = true;
            probe.size = new Vector3(radius * 3f, radius * 2f, radius * 3f);
            probe.RenderProbe();

            // Pool floor — a flat plane with the overworld map painted on it.
            var floor = MakePlane("PoolFloor", room, new Vector3(0f, floorY, 0f), radius);
            var floorMat = floor.GetComponent<Renderer>().material;
            var mapTex = Resources.Load<Texture2D>("ElementbornUI/worldmap");
            if (mapTex != null)
            {
                if (floorMat.HasProperty("_BaseMap")) floorMat.SetTexture("_BaseMap", mapTex);
                floorMat.mainTexture = mapTex;
                floorMat.color = new Color(0.78f, 0.88f, 0.96f);
            }
            else floorMat.color = new Color(0.10f, 0.20f, 0.28f);

            // Shimmering water surface just above the floor (see the map through it).
            var water = MakePlane("PoolWater", room, new Vector3(0f, floorY + waterDepth, 0f), radius);
            ApplyWater(water.GetComponent<Renderer>(), insideOut: false);

            // Water globe — a big inverted sphere so you see shimmering water from the inside looking out.
            var globe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            globe.name = "WaterGlobe";
            globe.transform.SetParent(room, false);
            globe.transform.position = new Vector3(0f, radius * 0.35f, 0f);
            globe.transform.localScale = Vector3.one * (radius * 2.5f);
            var globeCol = globe.GetComponent<Collider>();
            if (globeCol != null) Destroy(globeCol); // don't trap the player inside the sphere collider
            ApplyWater(globe.GetComponent<Renderer>(), insideOut: true);

            // Portal nodes at their map positions, projected from world XZ onto the pool disc.
            foreach (var rift in WorldMapLayout.Rifts)
            {
                Vector2 nrm = Minimap.WorldToNormalized(rift.World, WorldMapLayout.BoundsMin, WorldMapLayout.BoundsMax);
                float x = (nrm.x - 0.5f) * 2f * (radius * 0.9f);
                float z = (nrm.y - 0.5f) * 2f * (radius * 0.9f);

                var node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                node.name = "Portal_" + rift.Id;
                node.transform.SetParent(room, false);
                node.transform.position = new Vector3(x, floorY + waterDepth + 0.5f, z);
                node.transform.localScale = new Vector3(0.9f, 0.55f, 0.9f);
                node.AddComponent<PortalNode>().Configure(rift);
                NpcShowcase.Label(node.transform, rift.Name);
            }
        }

        private static GameObject MakePlane(string name, Transform parent, Vector3 pos, float radius)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Plane); // Unity's plane is 10x10 units, facing +Y
            p.name = name;
            p.transform.SetParent(parent, false);
            p.transform.position = pos;
            p.transform.localScale = Vector3.one * (radius * 2f / 10f);
            return p;
        }

        private static void ApplyWater(Renderer r, bool insideOut)
        {
            if (r == null) return;
            var shader = Shader.Find("Elementborn/ToonWater")
                         ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader);
            var teal = new Color(0.16f, 0.70f, 0.72f, 0.55f);
            if (m.HasProperty("_ShallowColor")) m.SetColor("_ShallowColor", new Color(0.30f, 0.75f, 0.78f));
            if (m.HasProperty("_DeepColor")) m.SetColor("_DeepColor", new Color(0.05f, 0.30f, 0.42f));
            if (m.HasProperty("_FoamColor")) m.SetColor("_FoamColor", Color.white);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", teal);
            m.color = teal;
            m.renderQueue = (int)RenderQueue.Transparent; // let the map read through the surface
            if (insideOut && m.HasProperty("_Cull")) m.SetFloat("_Cull", (float)CullMode.Front); // see the globe from inside
            r.sharedMaterial = m;
        }
    }
}
