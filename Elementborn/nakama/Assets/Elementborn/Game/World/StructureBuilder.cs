using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Instantiates a <see cref="BuildingPlan"/> into GameObjects: one mesh per part, coloured with
    /// <see cref="ToonPalette"/>. Meshes are simple flat-shaded primitives (Wind-Waker chunky look),
    /// generated once and shared. Box parts get a BoxCollider so building bodies are solid.
    /// </summary>
    public sealed class StructureBuilder : MonoBehaviour
    {
        public GameObject Build(BuildingPlan plan, Transform parent, Vector3 worldPosition)
        {
            var root = new GameObject(plan.Name ?? "Structure");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPosition;
            foreach (var part in plan.Parts) BuildPart(part, root.transform);
            return root;
        }

        private static void BuildPart(PlacedPart part, Transform parent)
        {
            var go = new GameObject($"{part.Shape}_{part.Role}", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = part.LocalPosition;
            go.transform.localEulerAngles = part.EulerRotation;
            go.transform.localScale = part.Size;

            go.GetComponent<MeshFilter>().sharedMesh = MeshFor(part.Shape);
            go.GetComponent<MeshRenderer>().sharedMaterial =
                part.Tint.HasValue ? ToonPalette.Tinted(part.Tint.Value) : ToonPalette.For(part.Role);

            if (part.Shape == PartShape.Box)
            {
                var col = go.AddComponent<BoxCollider>();
                col.center = new Vector3(0f, 0.5f, 0f);
                col.size = Vector3.one;
            }
        }

        // ---- shared flat-shaded meshes (base-centre origin, unit height/footprint) ----------

        private static readonly Dictionary<PartShape, Mesh> _meshes = new Dictionary<PartShape, Mesh>();

        private static Mesh MeshFor(PartShape shape)
        {
            if (_meshes.TryGetValue(shape, out var m) && m != null) return m;
            m = shape switch
            {
                PartShape.Box      => BuildBox(),
                PartShape.Cylinder => BuildCylinder(16),
                PartShape.Pyramid  => BuildPyramid(),
                PartShape.Cone     => BuildCone(16),
                _                  => BuildBox()
            };
            _meshes[shape] = m;
            return m;
        }

        private static Mesh BuildBox()
        {
            var c = new[]
            {
                new Vector3(-.5f, 0, -.5f), new Vector3(.5f, 0, -.5f), new Vector3(.5f, 0, .5f), new Vector3(-.5f, 0, .5f),
                new Vector3(-.5f, 1, -.5f), new Vector3(.5f, 1, -.5f), new Vector3(.5f, 1, .5f), new Vector3(-.5f, 1, .5f)
            };
            var t = new List<Vector3>();
            void Quad(int a, int b, int cc, int d) { t.Add(c[a]); t.Add(c[b]); t.Add(c[cc]); t.Add(c[a]); t.Add(c[cc]); t.Add(c[d]); }
            Quad(0, 1, 2, 3); Quad(4, 5, 6, 7);
            Quad(0, 1, 5, 4); Quad(1, 2, 6, 5); Quad(2, 3, 7, 6); Quad(3, 0, 4, 7);
            return FlatMesh(t);
        }

        private static Mesh BuildPyramid()
        {
            var b0 = new Vector3(-.5f, 0, -.5f); var b1 = new Vector3(.5f, 0, -.5f);
            var b2 = new Vector3(.5f, 0, .5f);  var b3 = new Vector3(-.5f, 0, .5f);
            var a = new Vector3(0, 1, 0);
            var t = new List<Vector3> { b0, b1, a, b1, b2, a, b2, b3, a, b3, b0, a, b0, b1, b2, b0, b2, b3 };
            return FlatMesh(t);
        }

        private static Mesh BuildCone(int n)
        {
            var t = new List<Vector3>();
            var apex = new Vector3(0, 1, 0); var ctr = Vector3.zero;
            for (int i = 0; i < n; i++)
            {
                float a0 = i / (float)n * Mathf.PI * 2f, a1 = (i + 1) / (float)n * Mathf.PI * 2f;
                var p0 = new Vector3(Mathf.Cos(a0) * .5f, 0, Mathf.Sin(a0) * .5f);
                var p1 = new Vector3(Mathf.Cos(a1) * .5f, 0, Mathf.Sin(a1) * .5f);
                t.Add(p0); t.Add(p1); t.Add(apex);
                t.Add(ctr); t.Add(p0); t.Add(p1);
            }
            return FlatMesh(t);
        }

        private static Mesh BuildCylinder(int n)
        {
            var t = new List<Vector3>();
            var bc = Vector3.zero; var tc = new Vector3(0, 1, 0);
            for (int i = 0; i < n; i++)
            {
                float a0 = i / (float)n * Mathf.PI * 2f, a1 = (i + 1) / (float)n * Mathf.PI * 2f;
                var b0 = new Vector3(Mathf.Cos(a0) * .5f, 0, Mathf.Sin(a0) * .5f);
                var b1 = new Vector3(Mathf.Cos(a1) * .5f, 0, Mathf.Sin(a1) * .5f);
                var u0 = new Vector3(b0.x, 1, b0.z); var u1 = new Vector3(b1.x, 1, b1.z);
                t.Add(b0); t.Add(b1); t.Add(u1); t.Add(b0); t.Add(u1); t.Add(u0);
                t.Add(bc); t.Add(b0); t.Add(b1);
                t.Add(tc); t.Add(u1); t.Add(u0);
            }
            return FlatMesh(t);
        }

        // Build a flat-shaded mesh from a triangle soup, orienting each face outward from the centroid.
        private static Mesh FlatMesh(List<Vector3> tris)
        {
            Vector3 c = Vector3.zero;
            for (int i = 0; i < tris.Count; i++) c += tris[i];
            if (tris.Count > 0) c /= tris.Count;

            var verts = new List<Vector3>(tris.Count);
            var idx = new List<int>(tris.Count);
            for (int i = 0; i < tris.Count; i += 3)
            {
                Vector3 a = tris[i], b = tris[i + 1], d = tris[i + 2];
                if (Vector3.Dot(Vector3.Cross(b - a, d - a), (a + b + d) / 3f - c) < 0f)
                {
                    var tmp = b; b = d; d = tmp;
                }
                int bi = verts.Count; verts.Add(a); verts.Add(b); verts.Add(d);
                idx.Add(bi); idx.Add(bi + 1); idx.Add(bi + 2);
            }
            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(idx, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
