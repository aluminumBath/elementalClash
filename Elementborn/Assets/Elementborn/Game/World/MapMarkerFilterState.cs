using System;
using System.Collections.Generic;
using Elementborn.Core;

namespace Elementborn.Game
{
    [Serializable]
    public class MapMarkerFilterState
    {
        public bool ShowPlayer = true;
        public bool ShowOwned = true;
        public bool ShowItems = true;
        public bool ShowNpcs = true;
        public bool ShowThreats = true;
        public bool ShowPlaces = true;
        public bool ShowCustomPins = true;
        public bool ShowCompleted = true;

        public bool Allows(MapMarkerType type)
        {
            switch (type)
            {
                case MapMarkerType.Player:
                    return ShowPlayer;

                case MapMarkerType.Boat:
                case MapMarkerType.LastRiddenCreature:
                case MapMarkerType.ActiveCompanion:
                case MapMarkerType.Camp:
                case MapMarkerType.HomeBase:
                case MapMarkerType.StorageChest:
                case MapMarkerType.CraftingStation:
                case MapMarkerType.Stable:
                    return ShowOwned;

                case MapMarkerType.QuestItem:
                case MapMarkerType.RareItem:
                case MapMarkerType.Weapon:
                case MapMarkerType.ResourceNode:
                case MapMarkerType.Treasure:
                    return ShowItems;

                case MapMarkerType.VendorNpc:
                case MapMarkerType.GuideNpc:
                case MapMarkerType.TrainerNpc:
                case MapMarkerType.HealerNpc:
                case MapMarkerType.QuestGiverNpc:
                    return ShowNpcs;

                case MapMarkerType.RareEnemySighting:
                case MapMarkerType.BossLair:
                case MapMarkerType.EnemyCamp:
                case MapMarkerType.DangerZone:
                case MapMarkerType.SeaMonsterSighting:
                    return ShowThreats;

                case MapMarkerType.Dock:
                case MapMarkerType.FastTravel:
                case MapMarkerType.Shrine:
                case MapMarkerType.Dungeon:
                case MapMarkerType.Cave:
                case MapMarkerType.Puzzle:
                case MapMarkerType.LockedDoor:
                case MapMarkerType.FishingSpot:
                case MapMarkerType.UnderwaterRuin:
                case MapMarkerType.CoralReef:
                case MapMarkerType.WindCurrent:
                case MapMarkerType.CurrentObjective:
                    return ShowPlaces;

                case MapMarkerType.CustomPin:
                    return ShowCustomPins;

                default:
                    return true;
            }
        }
    }
}
