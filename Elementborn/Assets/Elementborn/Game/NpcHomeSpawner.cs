using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Drops the royal houses and Deb into the world at their home regions (near the relevant capitals/cities), each
    /// as a colour-coded capsule placeholder carrying its interact controller and a name label — so they exist and
    /// are talkable without hand-placing every one in a scene. Put this on a bootstrap object; it builds once on
    /// Start. Swap the capsules for real models later (or bind models to the controllers).
    /// </summary>
    public sealed class NpcHomeSpawner : MonoBehaviour
    {
        [SerializeField] private bool spawnOnStart = true;
        private bool _built;

        private void Start() { if (spawnOnStart) Build(); }

        public void Build()
        {
            if (_built) return;
            _built = true;

            var root = new GameObject("NpcHomes").transform;

            // The Crown — the Neutral Central City, near the Confluence.
            NpcShowcase.SpawnRoyal(Royal.KingRonald, new Vector3(6f, 0f, 4f), root);
            NpcShowcase.SpawnRoyal(Royal.QueenRenee, new Vector3(9f, 0f, 4f), root);

            // House Windwyrm — the Metal Capital's steamworks (steam + metal).
            NpcShowcase.SpawnRoyal(Royal.JaemysWindwyrm, new Vector3(140f, 0f, -118f), root);
            NpcShowcase.SpawnRoyal(Royal.SamaraWindwyrm, new Vector3(144f, 0f, -121f), root);
            NpcShowcase.SpawnRoyal(Royal.ConradWindwyrm, new Vector3(147f, 0f, -116f), root);

            // House Flowers — the terraced gardens near the Earth Capital.
            NpcShowcase.SpawnRoyal(Royal.KellyFlowers, new Vector3(-150f, 0f, 132f), root);
            NpcShowcase.SpawnRoyal(Royal.JaadebFlowers, new Vector3(-154f, 0f, 129f), root);
            NpcShowcase.SpawnRoyal(Royal.JB, new Vector3(-157f, 0f, 134f), root);

            // The Crab-Sign Creature Orphanage, Neritha Reefwood — Ella, Eloc, and Deb who guards it.
            NpcShowcase.SpawnRoyal(Royal.Ella, new Vector3(-206f, 0f, -106f), root);
            NpcShowcase.SpawnRoyal(Royal.Eloc, new Vector3(-209f, 0f, -109f), root);
            NpcShowcase.SpawnGuide(GuideNpcId.Deb, new Vector3(-210f, 0f, -112f), root);
        }
    }
}
