using NUnit.Framework;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class MeshCombinerTests
    {
        private static Mesh Quad()
        {
            var m = new Mesh();
            m.vertices = new[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) };
            m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateNormals();
            return m;
        }

        private static void Child(Transform parent, Material mat, Vector3 pos)
        {
            var go = new GameObject("part", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.GetComponent<MeshFilter>().sharedMesh = Quad();
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        [Test]
        public void CombinesOneMeshPerMaterial()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) { Assert.Ignore("Unlit/Color shader unavailable in this environment."); return; }
            var matA = new Material(shader);
            var matB = new Material(shader);

            var root = new GameObject("StructureRoot");
            Child(root.transform, matA, Vector3.zero);
            Child(root.transform, matA, new Vector3(2, 0, 0));
            Child(root.transform, matB, new Vector3(0, 0, 2));

            int created = MeshCombiner.CombineHierarchy(root, addMeshCollider: false, destroyOriginals: false);
            Assert.AreEqual(2, created, "expected one combined mesh per distinct material");

            int combinedChildren = 0;
            foreach (Transform t in root.transform)
                if (t.name.StartsWith("Combined_"))
                {
                    combinedChildren++;
                    Assert.Greater(t.GetComponent<MeshFilter>().sharedMesh.vertexCount, 0);
                }
            Assert.AreEqual(2, combinedChildren);

            Object.DestroyImmediate(root);
        }
    }
}
