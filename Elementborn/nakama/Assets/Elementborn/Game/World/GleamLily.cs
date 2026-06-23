using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A rare gold-and-pink sparkly lily. A plant user can <see cref="Harvest"/> it (via
    /// <see cref="PlantControlController"/>) to coax out a silver-skinned, blood-red <see cref="Heartfruit"/>
    /// that heals and cures — on a long cooldown, since it's uncommon. Spawns a placeholder fruit if no prefab
    /// is assigned.
    /// </summary>
    public sealed class GleamLily : MonoBehaviour
    {
        [SerializeField] private GameObject heartfruitPrefab;
        [SerializeField] private float harvestCooldown = PlantTuning.LilyHarvestCooldown;

        private float _cd;

        public bool CanHarvest => _cd <= 0f;

        private void Update()
        {
            if (_cd > 0f) _cd -= Time.deltaTime;
        }

        /// <summary>Plant-user command: produce one Heartfruit. Returns false if still recharging.</summary>
        public bool Harvest()
        {
            if (_cd > 0f) return false;
            _cd = harvestCooldown;

            Vector3 at = transform.position + Vector3.up * 0.5f;
            if (heartfruitPrefab != null) Instantiate(heartfruitPrefab, at, Quaternion.identity);
            else MakePlaceholderFruit(at);
            return true;
        }

        private void MakePlaceholderFruit(Vector3 at)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Heartfruit";
            go.transform.position = at;
            go.transform.localScale = Vector3.one * 0.4f;
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.78f, 0.78f, 0.82f); // silver skin
            go.AddComponent<Heartfruit>();
        }
    }
}
