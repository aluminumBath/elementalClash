using System.Collections;
using UnityEngine;
using Elementborn.Core;
using Elementborn.Game.Feel;

namespace Elementborn.Game
{
    /// <summary>Concord — the neutral convergence hub — built at the world's capital region: the towering
    /// <b>Convergence Tower</b> landmark, the diplomat <see cref="StoryLore.DiplomatName"/> at its base, and the
    /// inciting <b>tower blast</b>. While the campaign is at <see cref="StoryChapter.Arrival"/>, approaching the
    /// tower triggers the blast — it advances the story to <see cref="StoryChapter.TheTowerBlast"/>, kills the
    /// diplomat, and shakes the world. The tall spire also seeds the vertical world to come (reaching it later
    /// will want climbing/gliding/flight). Built from <see cref="ToonPalette"/>-tinted primitives so it needs no
    /// assets — models can replace the parts later. Spawned by <see cref="WorldSpawnPlacer"/> at the capital.</summary>
    public sealed class ConcordSite : MonoBehaviour
    {
        [SerializeField] private float towerHeight = 48f;
        [SerializeField] private float triggerRadius = 14f;

        private Transform _diplomat;
        private Transform _crown;
        private FirstPersonRig _rig;
        private bool _fired;

        private void Start()
        {
            BuildTower();
            // A save loaded past Arrival is already post-blast: don't place the diplomat or replay the explosion.
            bool aftermath = StoryController.Instance != null
                             && StoryController.Instance.Chapter != StoryChapter.Arrival;
            if (!aftermath) BuildDiplomat();
            _fired = aftermath;
        }

        private void Update()
        {
            if (_fired) return;
            if (StoryController.Instance == null || StoryController.Instance.Chapter != StoryChapter.Arrival) return;
            if (_rig == null) _rig = FindObjectOfType<FirstPersonRig>();
            if (_rig == null) return;
            if ((_rig.transform.position - transform.position).sqrMagnitude <= triggerRadius * triggerRadius)
                Detonate();
        }

        /// <summary>Fire the inciting incident: the blast that kills the diplomat and tips the realms toward war.</summary>
        public void Detonate()
        {
            if (_fired) return;
            _fired = true;

            FindObjectOfType<CameraShaker>()?.Shake(0.5f, 0.7f);
            if (_diplomat != null) Destroy(_diplomat.gameObject);
            if (_crown != null) Destroy(_crown.gameObject);
            StartCoroutine(BlastFlash());

            StoryController.Instance?.SetChapter(StoryChapter.TheTowerBlast);
            GameHud.Instance?.Toast(StoryLore.ConvergenceTower + " erupts — " + StoryLore.DiplomatName + " is gone.");
        }

        // ----- construction -----

        private void BuildTower()
        {
            var baseCol = Part(PrimitiveType.Cylinder, "ConvergenceTower",
                new Vector3(7f, towerHeight * 0.5f, 7f), new Vector3(0f, towerHeight * 0.5f, 0f),
                new Color(0.86f, 0.88f, 0.93f));
            baseCol.name = "ConvergenceTower";

            Part(PrimitiveType.Cylinder, "Spire",
                new Vector3(3.6f, towerHeight * 0.30f, 3.6f), new Vector3(0f, towerHeight * 1.10f, 0f),
                new Color(0.92f, 0.93f, 0.97f));

            _crown = MakeOrb(new Color(1f, 0.97f, 0.80f), 4f, new Vector3(0f, towerHeight * 1.50f, 0f)).transform;

            // The four realms converge: an element-tinted orb at each compass point near the crown.
            Element[] elems = { Element.Fire, Element.Water, Element.Earth, Element.Air };
            for (int i = 0; i < elems.Length; i++)
            {
                float a = i * Mathf.PI * 0.5f;
                var pos = new Vector3(Mathf.Cos(a) * 5.5f, towerHeight * 0.95f, Mathf.Sin(a) * 5.5f);
                MakeOrb(ElementColor.For(elems[i]), 2.2f, pos);
            }
        }

        private void BuildDiplomat()
        {
            var d = Part(PrimitiveType.Capsule, "Ambassador Calderon",
                new Vector3(0.9f, 1.0f, 0.9f), new Vector3(6f, 1f, 0f),
                new Color(0.95f, 0.92f, 0.82f));
            d.name = "Ambassador Calderon";
            // A small gold mote marks her as the story figure among any nearby civilians.
            var mote = MakeOrb(new Color(1f, 0.9f, 0.55f), 0.5f, Vector3.zero);
            mote.transform.SetParent(d.transform, false);
            mote.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            _diplomat = d.transform;
        }

        private GameObject MakeOrb(Color c, float size, Vector3 localPos)
        {
            return Part(PrimitiveType.Sphere, "Orb", Vector3.one * size, localPos, c);
        }

        private GameObject Part(PrimitiveType prim, string name, Vector3 scale, Vector3 localPos, Color color)
        {
            var go = GameObject.CreatePrimitive(prim);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localScale = scale;
            go.transform.localPosition = localPos;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col); // landmarks: visual only for this slice, no physics blocking
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(color);
            return go;
        }

        private IEnumerator BlastFlash()
        {
            var f = Part(PrimitiveType.Sphere, "Blast", Vector3.one * 2f,
                new Vector3(0f, towerHeight, 0f), new Color(1f, 0.78f, 0.42f));
            float t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                f.transform.localScale = Vector3.one * Mathf.Lerp(2f, 34f, t / 0.45f);
                yield return null;
            }
            Destroy(f);
        }
    }
}
