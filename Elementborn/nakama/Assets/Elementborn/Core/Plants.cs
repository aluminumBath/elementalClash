namespace Elementborn.Core
{
    /// <summary>The controllable / interactive flora.</summary>
    public enum PlantKind
    {
        Snaptrap,    // giant carnivorous trap — grabs and bites on its own
        Vine,        // trips or holds; climbable; a plant user can lash it at foes
        Spore,       // mushroom cluster; a plant user puffs it to disorient (and is immune)
        Gleamlily,   // rare sparkly lily; a plant user harvests a healing fruit from it
        Heartfruit,  // the silver-skinned, blood-red healing fruit a Gleamlily yields
        Venomlily,   // poisonous look-alike of the Gleamlily — purple sparkles, harmful to touch
        WillowGate   // weeping-willow gate only a plant user can open
    }

    public readonly struct PlantInfo
    {
        public readonly string Name;
        public readonly bool Hostile;          // acts against people on its own
        public readonly bool NeedsPlantUser;   // only a plant user can command / use / open it
        public readonly string Note;

        public PlantInfo(string name, bool hostile, bool needsPlantUser, string note)
        {
            Name = name;
            Hostile = hostile;
            NeedsPlantUser = needsPlantUser;
            Note = note;
        }
    }

    /// <summary>Names and rules per plant. Tweak here; add a plant by extending the enum + this switch.</summary>
    public static class PlantCatalog
    {
        public static PlantInfo For(PlantKind kind)
        {
            switch (kind)
            {
                case PlantKind.Snaptrap:
                    return new PlantInfo("Maw Snaptrap", true, false, "Grabs and bites anyone who strays too close.");
                case PlantKind.Vine:
                    return new PlantInfo("Grasping Vine", false, false, "Trips the unwary and can be climbed; a plant user lashes it at foes.");
                case PlantKind.Spore:
                    return new PlantInfo("Hazecap Spores", false, true, "A plant user puffs them to disorient foes, unaffected themselves.");
                case PlantKind.Gleamlily:
                    return new PlantInfo("Gleamlily", false, true, "Rare gold-and-pink bloom; a plant user coaxes a healing Heartfruit from it.");
                case PlantKind.Heartfruit:
                    return new PlantInfo("Heartfruit", false, false, "Silver-skinned, blood-red within; heals and cures whoever it's used on.");
                case PlantKind.Venomlily:
                    return new PlantInfo("Venom Lily", false, false, "A poisonous look-alike of the Gleamlily — purple-sparkled, harmful to the touch, and bears no fruit.");
                case PlantKind.WillowGate:
                    return new PlantInfo("Willow Gate", false, true, "A weeping-willow gate only a plant user can open; common in the marshes.");
                default:
                    return new PlantInfo("Plant", false, false, "");
            }
        }
    }

    /// <summary>
    /// The plant-control gate. A "plant user" is a Channeler with the Verdancy specialty (the Water+Earth
    /// combination). Pure, so it unit-tests and any system can ask the same question.
    /// </summary>
    public static class PlantControl
    {
        public static bool IsPlantUser(ChannelerLoadout loadout) =>
            loadout != null && loadout.HasSubArt(SubArt.Verdancy);

        /// <summary>A steam/healer is a Channeler with the Steamcraft specialty (the Water+Fire combination).</summary>
        public static bool IsSteamHealer(ChannelerLoadout loadout) =>
            loadout != null && loadout.HasSubArt(SubArt.Steamcraft);

        /// <summary>Both plant users and steam/healers can make a Gleamlily bloom and take its Heartfruit.</summary>
        public static bool CanTendLily(ChannelerLoadout loadout) =>
            IsPlantUser(loadout) || IsSteamHealer(loadout);

        /// <summary>Plant users open willow gates and are immune to spore haze.</summary>
        public static bool CanOpenGate(ChannelerLoadout loadout) => IsPlantUser(loadout);
        public static bool ResistsSpores(ChannelerLoadout loadout) => IsPlantUser(loadout);
    }

    /// <summary>Tuning for the plant behaviours.</summary>
    public static class PlantTuning
    {
        // Snaptrap
        public const float SnaptrapRange = 2.6f;
        public const float SnaptrapHold = 1.5f;   // Control duration
        public const float SnaptrapDamage = 12f;
        public const float SnaptrapCooldown = 2.5f;

        // Vine
        public const float VineTripStun = 0.6f;    // brief Stun (a stumble)
        public const float VineSnareHold = 2.2f;   // Control when lashed by a plant user
        public const float VineSlow = 0.5f;        // Slow magnitude
        public const float VineSlowDuration = 2.5f;
        public const float VineClimbSpeed = 2.6f;

        // Spore
        public const float SporeRadius = 4.5f;
        public const float SporeDisorient = 2.5f;  // disorient (Control) duration
        public const float SporeAutoRadius = 1.6f; // a non-plant-user brushing it triggers a puff

        // Gleamlily / Heartfruit
        public const float LilyHarvestCooldown = 30f;
        public const float HeartfruitHeal = 45f;

        // Willow gate
        public const float GateOpenRange = 3.2f;

        // Venom Lily (poisonous Gleamlily look-alike)
        public const float VenomContactDamage = 7f;
        public const float VenomTickInterval = 1f;
        public const float VenomSlow = 0.4f;
        public const float VenomSlowDuration = 3f;
    }
}
