namespace Elementborn.Core
{
    /// <summary>
    /// General purpose marker categories for the player's world map.
    /// Keep this broad enough for runtime discoveries, player-owned entities, quest tracking, and custom pins.
    /// </summary>
    public enum MapMarkerType
    {
        Unknown = 0,

        // Core player state
        Player = 1,
        Boat = 2,
        LastRiddenCreature = 3,
        ActiveCompanion = 4,
        CurrentObjective = 5,
        QuestObjective = CurrentObjective,

        // Player-owned / placed
        Camp = 10,
        HomeBase = 11,
        StorageChest = 12,
        CraftingStation = 13,
        Stable = 14,

        // Items / loot
        QuestItem = 20,
        RareItem = 21,
        Weapon = 22,
        ResourceNode = 23,
        Treasure = 24,

        // NPCs
        VendorNpc = 30,
        GuideNpc = 31,
        TrainerNpc = 32,
        HealerNpc = 33,
        QuestGiverNpc = 34,
        Vendor = VendorNpc,

        // Threats
        RareEnemySighting = 40,
        BossLair = 41,
        EnemyCamp = 42,
        DangerZone = 43,
        SeaMonsterSighting = 44,

        // Places / discoveries
        Dock = 50,
        FastTravel = 51,
        Shrine = 52,
        Dungeon = 53,
        Cave = 54,
        Puzzle = 55,
        LockedDoor = 56,
        FishingSpot = 57,
        UnderwaterRuin = 58,
        CoralReef = 59,
        WindCurrent = 60,

        // Player-authored
        CustomPin = 90
    }
}
