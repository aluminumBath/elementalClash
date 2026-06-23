using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Scatters a small school of decorative <see cref="FishWanderer"/>s around itself (e.g. inside a
    /// <see cref="WaterVolume"/>). Assign a fish prefab for real art; with none it makes flat placeholder cubes
    /// so the area still feels alive.
    /// </summary>
    public sealed class FishSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject fishPrefab;
        [SerializeField] private int count = 12;
        [SerializeField] private float radius = 8f;

        private void Start()
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = transform.position + Random.insideUnitSphere * radius;
                pos.y = transform.position.y + Random.Range(-radius * 0.4f, radius * 0.4f);

                GameObject go = fishPrefab != null
                    ? Instantiate(fishPrefab, pos, Random.rotationUniform, transform)
                    : Placeholder(pos);

                var fish = go.GetComponent<FishWanderer>();
                if (fish == null) fish = go.AddComponent<FishWanderer>();
                fish.Init(transform.position, radius);
            }
        }

        private GameObject Placeholder(Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Fish (placeholder)";
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.5f, 0.22f, 0.25f);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            return go;
        }
    }
}
