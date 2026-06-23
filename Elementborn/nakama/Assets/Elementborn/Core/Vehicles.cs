namespace Elementborn.Core
{
    /// <summary>How a mount or vehicle moves.</summary>
    public enum LocomotionType { Ground, Water, Flying }

    /// <summary>Craft the player can buy and ride. Boats are open to anyone; the flying ships are
    /// element-locked (fire galleons, air skiffs). Earth users get boats plus horses (a creature).</summary>
    public enum VehicleKind { FireGalleon, AirSkiff, Rowboat, Sailboat }

    public readonly struct VehicleInfo
    {
        public readonly string Name;
        public readonly Element? RequiredElement; // null = anyone may use it
        public readonly LocomotionType Locomotion;
        public readonly long Price;
        public readonly int Capacity;

        public VehicleInfo(string name, Element? requiredElement, LocomotionType locomotion, long price, int capacity)
        {
            Name = name;
            RequiredElement = requiredElement;
            Locomotion = locomotion;
            Price = price;
            Capacity = capacity;
        }
    }

    public static class VehicleCatalog
    {
        public static VehicleInfo For(VehicleKind kind)
        {
            switch (kind)
            {
                case VehicleKind.FireGalleon:
                    return new VehicleInfo("Fire Galleon", Element.Fire, LocomotionType.Flying, 8000, 4);
                case VehicleKind.AirSkiff:
                    return new VehicleInfo("Air Skiff", Element.Air, LocomotionType.Flying, 1400, 2);
                case VehicleKind.Rowboat:
                    return new VehicleInfo("Rowboat", null, LocomotionType.Water, 250, 2);
                case VehicleKind.Sailboat:
                    return new VehicleInfo("Sailboat", null, LocomotionType.Water, 1200, 4);
                default:
                    return new VehicleInfo("Unknown", null, LocomotionType.Ground, 0, 1);
            }
        }
    }

    /// <summary>How each creature moves when ridden/roaming.</summary>
    public static class Locomotion
    {
        public static LocomotionType For(CreatureKind kind)
        {
            switch (kind)
            {
                case CreatureKind.FireDragon:
                case CreatureKind.WaterDragon:
                case CreatureKind.AirDragonfly:
                case CreatureKind.AirJellyfish:
                case CreatureKind.Phoenix:
                case CreatureKind.Roc:
                case CreatureKind.Thunderbird:
                case CreatureKind.Ridgewing:
                case CreatureKind.Glidewisp:
                case CreatureKind.Skytyrant:
                    return LocomotionType.Flying;
                case CreatureKind.Mermaid:
                case CreatureKind.WaterCat:
                case CreatureKind.Eel:
                case CreatureKind.Goldkoi:
                case CreatureKind.Skimfin:
                case CreatureKind.Gillcloak:
                case CreatureKind.Tidewarden:
                    return LocomotionType.Water;
                default:
                    return LocomotionType.Ground;
            }
        }
    }
}
