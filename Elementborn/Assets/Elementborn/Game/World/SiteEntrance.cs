using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>An in-world entrance to an instanced destination (a cave, aerie, sunken gate, temple, or spring).
    /// Stepping near it discovers it (a one-time toast); standing in range offers an Interact to enter. The
    /// instanced-interior loader is wired into <see cref="Enter"/> in the next pass — for now entering surfaces the
    /// site's lore. Built from <see cref="ToonPalette"/>-tinted primitives so it needs no assets. Placed by
    /// <see cref="WorldSpawnPlacer"/> on a region whose biome fits the site.</summary>
    public sealed class SiteEntrance : MonoBehaviour, IInteractable
    {
        [SerializeField] private float discoverRange = 9f;
        [SerializeField] private float interactRange = 4.5f;

        private SiteInfo _info;
        private string _id;
        private bool _discovered;

        public void Configure(SiteKind kind, string id)
        {
            _info = SiteCatalog.For(kind);
            _id = id;
            BuildMarker();
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            float d = Vector3.Distance(playerPosition, transform.position);

            if (!_discovered && d <= discoverRange)
            {
                _discovered = true;
                GameHud.Instance?.Toast("Discovered \u2014 " + _info.DisplayName);
                AudioController.Instance?.Confirm();
            }

            if (d > interactRange) return false;
            interaction = new Interaction(d, 0, "Enter " + _info.DisplayName, Enter);
            return true;
        }

        // Open the instanced interior this entrance leads to; the player returns here on leaving.
        private void Enter()
        {
            if (SiteInteriorController.Instance != null)
                SiteInteriorController.Instance.Enter(_info, transform.position + Vector3.up * 1f);
            else
                GameHud.Instance?.Toast(_info.Lore);
        }

        private void BuildMarker()
        {
            Color tint = Accent(_info.Kind);

            // A simple portal: two pillars + a lintel, with a glowing orb in the gateway.
            Pillar(new Vector3(-1.1f, 1.4f, 0f), tint);
            Pillar(new Vector3(1.1f, 1.4f, 0f), tint);
            var lintel = Block(PrimitiveType.Cube, new Vector3(2.8f, 0.5f, 0.7f), new Vector3(0f, 2.9f, 0f), tint);
            lintel.name = "Lintel";

            var orb = Block(PrimitiveType.Sphere, Vector3.one * 0.9f, new Vector3(0f, 1.6f, 0f), tint);
            orb.name = "Gateheart";
            var mr = orb.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(tint); // already tinted; kept explicit for the glow
        }

        private void Pillar(Vector3 localPos, Color tint)
        {
            var p = Block(PrimitiveType.Cube, new Vector3(0.6f, 2.8f, 0.7f), localPos, tint);
            p.name = "Pillar";
        }

        private GameObject Block(PrimitiveType prim, Vector3 scale, Vector3 localPos, Color tint)
        {
            var go = GameObject.CreatePrimitive(prim);
            go.transform.SetParent(transform, false);
            go.transform.localScale = scale;
            go.transform.localPosition = localPos;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col); // discovery/interaction is distance-based; don't block the player
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(tint);
            return go;
        }

        private static Color Accent(SiteKind kind) => kind switch
        {
            SiteKind.CaveMouth => new Color(0.40f, 0.34f, 0.30f),      // dark stone
            SiteKind.Aerie => new Color(0.78f, 0.90f, 1.00f),         // pale sky
            SiteKind.SunkenEntrance => new Color(0.20f, 0.42f, 0.72f), // deep blue
            SiteKind.TempleDoor => new Color(0.82f, 0.74f, 0.45f),    // weathered gold
            SiteKind.Spring => new Color(0.45f, 0.72f, 0.50f),        // mossy green
            _ => new Color(0.7f, 0.7f, 0.7f),
        };
    }
}
