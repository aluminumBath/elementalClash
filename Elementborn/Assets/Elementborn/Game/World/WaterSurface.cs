using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// A code-built water plane using the Elementborn/ToonWater shader, sized to the world and placed
    /// at sea level. Build standalone (buildOnStart) or let <see cref="TerrainBuilder"/> drive it so
    /// the surface matches the terrain's sea level.
    /// </summary>
    public sealed class WaterSurface : MonoBehaviour
    {
        [SerializeField] private float size = 1200f;
        [SerializeField] private int subdivisions = 60;
        [SerializeField] private float seaWorldY = 14.4f;
        [SerializeField] private Color shallow = new Color(0.30f, 0.70f, 0.74f);
        [SerializeField] private Color deep = new Color(0.06f, 0.28f, 0.45f);
        [SerializeField] private Color foam = Color.white;
        [SerializeField] private bool buildOnStart = true;

        private GameObject _surface;

        private void Start()
        {
            if (buildOnStart) Build(size, seaWorldY);
        }

        public GameObject Build(float planeSize, float worldY)
        {
            if (_surface != null) Destroy(_surface);
            _surface = new GameObject("ToonWater", typeof(MeshFilter), typeof(MeshRenderer));
            _surface.transform.SetParent(transform, false);
            _surface.transform.position = new Vector3(planeSize * 0.5f, worldY, planeSize * 0.5f);
            _surface.GetComponent<MeshFilter>().sharedMesh = BuildPlane(planeSize, Mathf.Max(1, subdivisions));

            var shader = Shader.Find("Elementborn/ToonWater")
                         ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            if (mat.HasProperty("_ShallowColor")) mat.SetColor("_ShallowColor", shallow);
            if (mat.HasProperty("_DeepColor")) mat.SetColor("_DeepColor", deep);
            if (mat.HasProperty("_FoamColor")) mat.SetColor("_FoamColor", foam);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", shallow);
            _surface.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return _surface;
        }

        private static Mesh BuildPlane(float size, int sub)
        {
            int n = sub + 1;
            var verts = new Vector3[n * n];
            var normals = new Vector3[n * n];
            var uvs = new Vector2[n * n];
            float half = size * 0.5f, step = size / sub;
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                {
                    int idx = z * n + x;
                    verts[idx] = new Vector3(-half + x * step, 0f, -half + z * step);
                    normals[idx] = Vector3.up;
                    uvs[idx] = new Vector2(x / (float)sub, z / (float)sub);
                }

            var tris = new int[sub * sub * 6];
            int t = 0;
            for (int z = 0; z < sub; z++)
                for (int x = 0; x < sub; x++)
                {
                    int bl = z * n + x, br = bl + 1, tl = bl + n, tr = tl + 1;
                    tris[t++] = bl; tris[t++] = tl; tris[t++] = tr;
                    tris[t++] = bl; tris[t++] = tr; tris[t++] = br;
                }

            var mesh = new Mesh { name = "ToonWaterPlane" };
            mesh.indexFormat = n * n > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
