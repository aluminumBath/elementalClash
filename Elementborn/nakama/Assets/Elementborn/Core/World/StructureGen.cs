using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    public sealed class StructureGenConfig
    {
        public float Scale = 1f; // multiplies all part sizes/positions (world units per block)
    }

    /// <summary>
    /// Assembles a <see cref="BuildingPlan"/> for a structure, driven primarily by the POIs the world
    /// generator produces. Each <see cref="PoiType"/> maps to a <see cref="StructureKind"/>, then a
    /// builder lays out chunky primitive parts (boxes, pillars, pyramidal roofs, cones) in local
    /// space. Pure + seeded, so it is deterministic and unit-tested. Wind-Waker-leaning proportions
    /// and flat colours; the Unity side renders the parts with the toon shader.
    /// </summary>
    public static class StructureGenerator
    {
        public static StructureKind KindFor(PoiType poi) => poi switch
        {
            PoiType.City        => StructureKind.Town,
            PoiType.Village     => StructureKind.Village,
            PoiType.Temple      => StructureKind.Temple,
            PoiType.Shrine      => StructureKind.Shrine,
            PoiType.Dungeon     => StructureKind.RuinEntrance,
            PoiType.Market      => StructureKind.MarketStalls,
            PoiType.Landmark    => StructureKind.StandingStones,
            PoiType.Dock        => StructureKind.Dock,
            PoiType.Camp        => StructureKind.Tents,
            PoiType.Arena       => StructureKind.Arena,
            PoiType.WeaponCache => StructureKind.Crate,
            _                   => StructureKind.Cottage
        };

        public static BuildingPlan Generate(PointOfInterest poi, IRandomSource rng, StructureGenConfig cfg)
            => Generate(KindFor(poi.Type), poi.Name, rng, cfg);

        public static BuildingPlan Generate(StructureKind kind, string name, IRandomSource rng, StructureGenConfig cfg)
        {
            cfg ??= new StructureGenConfig();
            var parts = new List<PlacedPart>();
            float radius = kind switch
            {
                StructureKind.Cottage        => Cottage(parts, rng, Vector3.zero),
                StructureKind.Village        => Cluster(parts, rng, 4, 5f),
                StructureKind.Town           => Town(parts, rng),
                StructureKind.Hall           => Hall(parts, rng, Vector3.zero),
                StructureKind.Tower          => Tower(parts, rng),
                StructureKind.Temple         => Temple(parts, rng),
                StructureKind.Shrine         => Shrine(parts, rng),
                StructureKind.MarketStalls   => Market(parts, rng),
                StructureKind.Tents          => Tents(parts, rng),
                StructureKind.Dock           => Dock(parts, rng),
                StructureKind.Arena          => Arena(parts, rng),
                StructureKind.Statue         => Statue(parts, rng, Vector3.zero),
                StructureKind.StandingStones => StandingStones(parts, rng),
                StructureKind.RuinEntrance   => Ruin(parts, rng),
                StructureKind.Crate          => Crate(parts, rng, Vector3.zero),
                _                            => Cottage(parts, rng, Vector3.zero)
            };
            if (cfg.Scale != 1f) Rescale(parts, cfg.Scale);
            return new BuildingPlan(name, kind, parts, radius * cfg.Scale);
        }

        // ---- per-kind builders (return rough footprint radius in blocks) -------------------

        private static float Cottage(List<PlacedPart> p, IRandomSource r, Vector3 o)
        {
            Color roof = Pick(RoofColors, r);
            float w = F(r, 3f, 4f), d = F(r, 3f, 4f), h = F(r, 2.2f, 2.8f);
            Box(p, PartRole.Wall, o, V(w, h, d));
            Add(p, PartShape.Pyramid, PartRole.Roof, o + Up(h), V(w * 1.18f, F(r, 1.4f, 1.9f), d * 1.18f), roof);
            Box(p, PartRole.Wood, o + V(0, 0, -d * 0.5f - 0.06f), V(0.9f, 1.6f, 0.15f));            // door
            Add(p, PartShape.Box, PartRole.Accent, o + V(w * 0.28f, h * 0.5f, -d * 0.5f - 0.06f),   // window
                V(0.55f, 0.55f, 0.12f), WindowColor);
            return Mathf.Max(w, d) * 0.6f;
        }

        private static float Cluster(List<PlacedPart> p, IRandomSource r, int count, float ring)
        {
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f + F(r, -0.3f, 0.3f);
                Cottage(p, r, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * F(r, ring * 0.6f, ring));
            }
            return ring + 3f;
        }

        private static float Town(List<PlacedPart> p, IRandomSource r)
        {
            Hall(p, r, Vector3.zero);
            int n = 6;
            for (int i = 0; i < n; i++)
            {
                float a = i / (float)n * Mathf.PI * 2f + F(r, -0.2f, 0.2f);
                Cottage(p, r, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * F(r, 7f, 10f));
            }
            return 13f;
        }

        private static float Hall(List<PlacedPart> p, IRandomSource r, Vector3 o)
        {
            Color roof = Pick(RoofColors, r);
            float w = F(r, 6f, 8f), d = F(r, 7f, 9f), h = F(r, 3.4f, 4.2f);
            Box(p, PartRole.Wall, o, V(w, h, d));
            Add(p, PartShape.Pyramid, PartRole.Roof, o + Up(h), V(w * 1.15f, F(r, 2.2f, 3f), d * 1.15f), roof);
            Add(p, PartShape.Cylinder, PartRole.Stone, o + V(-w * 0.3f, 0, -d * 0.5f - 0.4f), V(0.6f, h, 0.6f));
            Add(p, PartShape.Cylinder, PartRole.Stone, o + V(w * 0.3f, 0, -d * 0.5f - 0.4f), V(0.6f, h, 0.6f));
            Add(p, PartShape.Box, PartRole.Cloth, o + V(0, h * 0.6f, -d * 0.5f - 0.5f), V(1.2f, 1.6f, 0.08f), Pick(ClothColors, r));
            Box(p, PartRole.Wood, o + V(0, 0, -d * 0.5f - 0.06f), V(1.6f, 2.2f, 0.2f));
            return Mathf.Max(w, d) * 0.65f;
        }

        private static float Tower(List<PlacedPart> p, IRandomSource r)
        {
            Color roof = Pick(RoofColors, r);
            int seg = 2 + (int)(r.NextUnit() * 2);
            float w = F(r, 2.6f, 3.2f), y = 0f;
            for (int i = 0; i < seg; i++)
            {
                float sh = F(r, 2.6f, 3.2f), sw = w * (1f - i * 0.12f);
                Box(p, PartRole.Stone, Up(y), V(sw, sh, sw));
                y += sh;
            }
            Add(p, PartShape.Cone, PartRole.Roof, Up(y), V(w * 1.1f, F(r, 2.4f, 3.2f), w * 1.1f), roof);
            return w;
        }

        private static float Temple(List<PlacedPart> p, IRandomSource r)
        {
            Color roof = Pick(RoofColors, r);
            float baseW = F(r, 9f, 11f), y = 0f;
            for (int i = 0; i < 3; i++)
            {
                Box(p, PartRole.Stone, Up(y), V(baseW - i * 1.6f, 0.6f, baseW - i * 1.6f));
                y += 0.6f;
            }
            float bodyW = baseW - 5f, bh = F(r, 3.2f, 4f);
            Box(p, PartRole.Wall, Up(y), V(bodyW, bh, bodyW));
            int cols = 8;
            float pr = bodyW * 0.5f + 0.8f;
            for (int i = 0; i < cols; i++)
            {
                float a = i / (float)cols * Mathf.PI * 2f;
                Add(p, PartShape.Cylinder, PartRole.Stone, new Vector3(Mathf.Cos(a) * pr, y, Mathf.Sin(a) * pr), V(0.5f, bh, 0.5f));
            }
            Add(p, PartShape.Pyramid, PartRole.Roof, Up(y + bh), V(bodyW * 1.5f, F(r, 2.6f, 3.4f), bodyW * 1.5f), roof);
            return baseW * 0.6f;
        }

        private static float Shrine(List<PlacedPart> p, IRandomSource r)
        {
            Box(p, PartRole.Stone, Vector3.zero, V(2.4f, 0.5f, 2.4f));
            Box(p, PartRole.Wall, Up(0.5f), V(1.3f, 1.4f, 1.3f));
            Add(p, PartShape.Pyramid, PartRole.Roof, Up(1.9f), V(1.7f, 1.0f, 1.7f), Pick(RoofColors, r));
            Add(p, PartShape.Cone, PartRole.Accent, Up(2.9f), V(0.35f, 0.6f, 0.35f), AccentColor);
            return 1.6f;
        }

        private static float Market(List<PlacedPart> p, IRandomSource r)
        {
            int stalls = 3 + (int)(r.NextUnit() * 2);
            for (int i = 0; i < stalls; i++)
            {
                float a = i / (float)stalls * Mathf.PI * 2f;
                Vector3 o = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * F(r, 2.5f, 3.5f);
                Box(p, PartRole.Wood, o, V(1.6f, 1.0f, 1.0f));
                Add(p, PartShape.Box, PartRole.Cloth, o + Up(1.4f), V(1.9f, 0.1f, 1.4f),
                    new Vector3(-18f, Mathf.Rad2Deg * a, 0f), Pick(ClothColors, r));
            }
            return 4.5f;
        }

        private static float Tents(List<PlacedPart> p, IRandomSource r)
        {
            int tents = 3 + (int)(r.NextUnit() * 3);
            for (int i = 0; i < tents; i++)
            {
                float a = i / (float)tents * Mathf.PI * 2f + F(r, -0.3f, 0.3f);
                Vector3 o = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * F(r, 2f, 3.5f);
                Add(p, PartShape.Cone, PartRole.Cloth, o, V(F(r, 1.6f, 2.2f), F(r, 1.8f, 2.4f), F(r, 1.6f, 2.2f)), Pick(ClothColors, r));
            }
            Add(p, PartShape.Cylinder, PartRole.Wood, Vector3.zero, V(0.8f, 0.25f, 0.8f));
            Add(p, PartShape.Cone, PartRole.Accent, Up(0.25f), V(0.5f, 0.7f, 0.5f), FireColor);
            return 4.5f;
        }

        private static float Dock(List<PlacedPart> p, IRandomSource r)
        {
            float len = F(r, 6f, 9f), w = 2.4f;
            Box(p, PartRole.Wood, V(0, 0.4f, len * 0.5f), V(w, 0.25f, len));
            for (int i = 0; i < 4; i++)
            {
                float zz = i < 2 ? 0.3f : len - 0.3f;
                float xx = i % 2 == 0 ? -w * 0.4f : w * 0.4f;
                Add(p, PartShape.Cylinder, PartRole.Wood, V(xx, 0, zz), V(0.3f, 0.8f, 0.3f));
            }
            Crate(p, r, V(0, 0.5f, 1f));
            return len * 0.6f;
        }

        private static float Arena(List<PlacedPart> p, IRandomSource r)
        {
            float rad = F(r, 6f, 8f);
            int seg = 16;
            Box(p, PartRole.Sand, Vector3.zero, V(rad * 1.8f, 0.2f, rad * 1.8f));
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                Vector3 o = new Vector3(Mathf.Cos(a) * rad, 0, Mathf.Sin(a) * rad);
                Add(p, PartShape.Box, PartRole.Stone, o, V(rad * 0.45f, F(r, 1.6f, 2.2f), 0.6f), new Vector3(0, Mathf.Rad2Deg * a, 0));
            }
            return rad * 1.1f;
        }

        private static float Statue(List<PlacedPart> p, IRandomSource r, Vector3 o)
        {
            Box(p, PartRole.Stone, o, V(2f, 0.8f, 2f));
            Box(p, PartRole.Stone, o + Up(0.8f), V(0.9f, 2.2f, 0.7f));
            Box(p, PartRole.Stone, o + Up(3.0f), V(0.8f, 0.8f, 0.8f));
            return 1.4f;
        }

        private static float StandingStones(List<PlacedPart> p, IRandomSource r)
        {
            int n = 5 + (int)(r.NextUnit() * 3);
            float rad = F(r, 3f, 4.5f);
            for (int i = 0; i < n; i++)
            {
                float a = i / (float)n * Mathf.PI * 2f;
                Vector3 o = new Vector3(Mathf.Cos(a) * rad, 0, Mathf.Sin(a) * rad);
                Add(p, PartShape.Box, PartRole.Stone, o, V(F(r, 0.7f, 1.1f), F(r, 2.5f, 3.8f), F(r, 0.7f, 1.1f)),
                    new Vector3(F(r, -6f, 6f), Mathf.Rad2Deg * a, F(r, -6f, 6f)));
            }
            return rad + 1f;
        }

        private static float Ruin(List<PlacedPart> p, IRandomSource r)
        {
            float gap = 2.4f, ph = F(r, 2.8f, 3.6f);
            Add(p, PartShape.Cylinder, PartRole.Stone, V(-gap * 0.5f, 0, 0), V(0.7f, ph, 0.7f));
            Add(p, PartShape.Cylinder, PartRole.Stone, V(gap * 0.5f, 0, 0), V(0.7f, ph * 0.7f, 0.7f), new Vector3(0, 0, 8f));
            Add(p, PartShape.Box, PartRole.Stone, Up(ph), V(gap + 1.2f, 0.6f, 0.9f), new Vector3(0, 0, -4f));
            return gap;
        }

        private static float Crate(List<PlacedPart> p, IRandomSource r, Vector3 o)
        {
            Box(p, PartRole.Wood, o, V(1.0f, 1.0f, 1.0f));
            Box(p, PartRole.Wood, o + Up(1.0f), V(0.7f, 0.7f, 0.7f));
            return 0.8f;
        }

        // ---- helpers ------------------------------------------------------------------------

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static Vector3 Up(float y) => new Vector3(0f, y, 0f);
        private static float F(IRandomSource r, float a, float b) => a + (float)r.NextUnit() * (b - a);
        private static Color Pick(Color[] arr, IRandomSource r) => arr[(int)(r.NextUnit() * arr.Length) % arr.Length];

        private static void Box(List<PlacedPart> p, PartRole role, Vector3 pos, Vector3 size)
            => p.Add(new PlacedPart(PartShape.Box, role, pos, size));
        private static void Add(List<PlacedPart> p, PartShape s, PartRole role, Vector3 pos, Vector3 size)
            => p.Add(new PlacedPart(s, role, pos, size));
        private static void Add(List<PlacedPart> p, PartShape s, PartRole role, Vector3 pos, Vector3 size, Color tint)
            => p.Add(new PlacedPart(s, role, pos, size, default, tint));
        private static void Add(List<PlacedPart> p, PartShape s, PartRole role, Vector3 pos, Vector3 size, Vector3 euler)
            => p.Add(new PlacedPart(s, role, pos, size, euler));
        private static void Add(List<PlacedPart> p, PartShape s, PartRole role, Vector3 pos, Vector3 size, Vector3 euler, Color tint)
            => p.Add(new PlacedPart(s, role, pos, size, euler, tint));

        private static void Rescale(List<PlacedPart> p, float s)
        {
            for (int i = 0; i < p.Count; i++)
            {
                var q = p[i];
                p[i] = new PlacedPart(q.Shape, q.Role, q.LocalPosition * s, q.Size * s, q.EulerRotation, q.Tint);
            }
        }

        private static readonly Color[] RoofColors =
        {
            new Color(0.80f, 0.24f, 0.18f), // red
            new Color(0.16f, 0.55f, 0.55f), // teal
            new Color(0.22f, 0.42f, 0.72f), // blue
            new Color(0.34f, 0.55f, 0.26f), // green
            new Color(0.85f, 0.52f, 0.18f)  // orange
        };

        private static readonly Color[] ClothColors =
        {
            new Color(0.95f, 0.92f, 0.82f),
            new Color(0.82f, 0.30f, 0.28f),
            new Color(0.30f, 0.45f, 0.72f),
            new Color(0.42f, 0.62f, 0.34f),
            new Color(0.92f, 0.82f, 0.34f)
        };

        private static readonly Color WindowColor = new Color(0.55f, 0.78f, 0.92f);
        private static readonly Color AccentColor = new Color(0.92f, 0.78f, 0.30f);
        private static readonly Color FireColor   = new Color(0.95f, 0.55f, 0.20f);
    }
}
