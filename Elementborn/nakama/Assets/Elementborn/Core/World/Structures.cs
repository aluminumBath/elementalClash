using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>Primitive shapes a structure is assembled from (kept simple for a Wind-Waker look).</summary>
    public enum PartShape { Box, Cylinder, Pyramid, Cone }

    /// <summary>Material/colour role for a part (maps to a flat toon colour on the Unity side).</summary>
    public enum PartRole { Wall, Floor, Roof, Wood, Stone, Accent, Foliage, Cloth, Sand }

    /// <summary>
    /// One placed building piece in a structure's local space. Convention: a piece's local origin is
    /// its base centre, and its mesh is unit-sized (height 1, footprint 1x1), scaled by <see cref="Size"/>.
    /// </summary>
    public readonly struct PlacedPart
    {
        public readonly PartShape Shape;
        public readonly PartRole Role;
        public readonly Vector3 LocalPosition;
        public readonly Vector3 EulerRotation;
        public readonly Vector3 Size;
        public readonly Color? Tint;   // overrides the role colour when set (WW colour variety)

        public PlacedPart(PartShape shape, PartRole role, Vector3 localPosition, Vector3 size,
            Vector3 eulerRotation = default, Color? tint = null)
        {
            Shape = shape; Role = role; LocalPosition = localPosition; Size = size;
            EulerRotation = eulerRotation; Tint = tint;
        }
    }

    /// <summary>What kind of structure to assemble.</summary>
    public enum StructureKind
    {
        Cottage, Village, Town, Hall, Tower, Temple, Shrine,
        MarketStalls, Tents, Dock, Arena, Statue, StandingStones, RuinEntrance, Crate
    }

    /// <summary>A procedurally-authored structure: a named list of placed parts plus a rough footprint.</summary>
    public sealed class BuildingPlan
    {
        public string Name { get; }
        public StructureKind Kind { get; }
        public IReadOnlyList<PlacedPart> Parts { get; }
        public float FootprintRadius { get; }

        public BuildingPlan(string name, StructureKind kind, IReadOnlyList<PlacedPart> parts, float footprintRadius)
        {
            Name = name; Kind = kind; Parts = parts; FootprintRadius = footprintRadius;
        }
    }
}
